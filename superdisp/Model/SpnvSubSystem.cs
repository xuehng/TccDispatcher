#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using log4net;
using renstech.NET.nids;
using renstech.NET.nids.AsynEvent;
using renstech.NET.nids.AsynEvent.spnv;
using renstech.NET.nids.RemoteProc;
using renstech.NET.PJSIP;
using renstech.NET.SIPUA;
using renstech.NET.SupernovaDispatcher.Interface;
using renstech.NET.SupernovaDispatcher.Model.Handset;
using renstech.NET.SupernovaDispatcher.Properties;
using renstech.NET.SupernovaDispatcher.Utils;
using renstech.NET.SupernovaDispatcher.xmlrpc;

#endregion

namespace renstech.NET.SupernovaDispatcher.Model
{
    public class SpnvSubSystem : Subsystem
    {
        #region field

        private static readonly ILog Log = LogManager.GetLogger(typeof(SpnvSubSystem));

        private SpnvPage _page;
        private EventSubscriber _eventListener;
        private readonly CallHistory _callHistories = new CallHistory();
        private HandsetInfo _leftHandsetInfo;
        private HandsetInfo _righHandsetInfo;

        private Timer _dialogUpdateTimer;
        private bool _shallUpdateChannelInfo;

        private Timer _callHistorySaveTimer;

        private static readonly Object LockObject = new object();
        private static readonly Queue<ChannelEventArgs> QueueDialog = new Queue<ChannelEventArgs>();
        private static Queue<ChannelEventArgs> DialogQueue
        {
            get
            {
                lock (LockObject)
                {
                    return QueueDialog;
                }
            }
        }
        private static readonly Timer DialogDequeueTimer = new Timer();

        //private DispatcherTimer _testTimer;

        private XmlrpcService XmlrpcService { get; set; }

        private const double DialogUpdateTimerInterval = 20000;

        #endregion

        #region public

        public SpnvChannels Channels { get; private set; }

        public CallHistory CallHisory
        {
            get { return _callHistories; }
        }

        public MessageInbox TextMessages { get; private set; }
        public UserInfo UserInfo { get; private set; }
        public DialogInfoDb DialogInfo { get; private set; }
        public SupernovaSetting Setting { get; private set; }
        public DialPrefix PrefixInfo { get; private set; }
        public SpnvRemoteProc Proxy { get; set; }
        public User LocalUser { get; set; }

        public HandsetInfo LeftHandsetInfo
        {
            get { return _leftHandsetInfo; }
        }

        public HandsetInfo RightHandsetInfo
        {
            get { return _righHandsetInfo; }
        }

        #endregion

        public SpnvSubSystem(SubsysId id, string name)
            : base(id, name)
        {
            Setting = null;
            Channels = new SpnvChannels(this);
            DialogInfo = new DialogInfoDb();
            UserInfo = new UserInfo();
            TextMessages = new MessageInbox();
            PrefixInfo = new DialPrefix();

            _dialogUpdateTimer = new Timer(DialogUpdateTimerInterval);
            _dialogUpdateTimer.Elapsed += UpdateChannelInfoTimerEx;
            _dialogUpdateTimer.Start();

            _callHistorySaveTimer = new Timer();
            _callHistorySaveTimer.Interval = 10 * 60 * 1000;
            _callHistorySaveTimer.Elapsed += CallHistorySaveTimer;
            _callHistorySaveTimer.Start();
        }

        #region 对外接口

        public override bool Initialize(ref string errMsg)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            InitResult = InitResult.InitFail;

            if (!InitializeSupernovaSetting())
                return false;

            Log.Info(string.Format("TimeSpendCounter__SpnvSys.InitSpnvSet__{0}", stopWatch.ElapsedMilliseconds));
            stopWatch.Restart();

            InitializeCustomizeUser();

            Log.Info(string.Format("TimeSpendCounter__SpnvSys.InitCustomUser__{0}", stopWatch.ElapsedMilliseconds));
            stopWatch.Restart();

            SIPUAMgr.RegStateInfo += OnRegState;
            SIPUAMgr.IncomingCallInfo += OnInComingCall;
            SIPUAMgr.CallStateInfo += OnCallStateChanged;
            SIPUAMgr.MediaStateInfo += OnAudioMediaStateChanged;
            SIPUAMgr.VideoMediaStateInfo += OnVideoMediaStateChanged;
            SIPUAMgr.PagerInfo += OnIncomingPagerMessage;

            InitializeHandset();

            Log.Info(string.Format("TimeSpendCounter__SpnvSys.InitHandset__{0}", stopWatch.ElapsedMilliseconds));
            stopWatch.Restart();

            Proxy = new SpnvRemoteProc(Setting.NidsAddr, Setting.NidsInstructPort);

            Log.Info(string.Format("TimeSpendCounter__SpnvSys.InitProxy__{0}", stopWatch.ElapsedMilliseconds));
            stopWatch.Restart();

            if (!InitializeServerData(ref errMsg))
            {
                return false;
            }

            Log.Info(string.Format("TimeSpendCounter__SpnvSys.InitSvrData__{0}", stopWatch.ElapsedMilliseconds));
            stopWatch.Restart();

            InitializeDirectory();

            Log.Info(string.Format("TimeSpendCounter__SpnvSys.InitDirectory__{0}", stopWatch.ElapsedMilliseconds));
            stopWatch.Restart();

            InitializeCallHistories();

            Log.Info(string.Format("TimeSpendCounter__SpnvSys.InitCallHistory__{0}", stopWatch.ElapsedMilliseconds));
            stopWatch.Restart();

            InitializeEvents();

            Log.Info(string.Format("TimeSpendCounter__SpnvSys.InitEvts__{0}", stopWatch.ElapsedMilliseconds));
            stopWatch.Restart();

            InitializeXmlrpcService();

            stopWatch.Stop();
            Log.Info(string.Format("TimeSpendCounter__SpnvSys.InitRpc__{0}", stopWatch.ElapsedMilliseconds));

            InitResult = InitResult.InitOk;

            return true;
        }

        public override Account GetSIPAccount()
        {
            //存在Domain为空时，程序启动失败的问题，所以要考虑判定Domain是否为空的条件
            //但是，添加判定条件后，有可能导致用户状态、拨打电话失败
            if (string.IsNullOrEmpty(Setting.AccountUser) ||
                string.IsNullOrEmpty(Setting.AccountDomain))
            {
                return null;
            }

            //if (string.IsNullOrEmpty(Setting.AccountUser))
            //{
            //    return null;
            //}

            var account = new Account
            {
                User = Setting.AccountUser,
                Password = Setting.AccountPassword,
                Domain = Setting.AccountDomain,
                Proxy = Setting.AccountProxy,
                IsRegistrationEnabled = true,
                IsDomainRegistration = Setting.IsDomainReg,
                IsAutoShowInboundVideo = false,
                RegistrationExpire = (uint)Setting.RegExpire
            };
            return account;
        }

        public override IDispatchPage GetDispatchPage()
        {
            return _page ?? (_page = new SpnvPage(this));
        }

        public bool CustomizedUserInfoSave()
        {
            try
            {
                var serializer = new XmlFileSerializer<List<User>>(Setting.CustomUsersFile);
                serializer.Save(UserInfo.CustomizedUsers);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return false;
            }
            return true;
        }

        #endregion

        #region 初始化

        protected bool InitializeSupernovaSetting()
        {
            var configured = ConfigMgr.Contains(typeof(SupernovaSetting).FullName);

            Setting = ConfigMgr.GetSetting(typeof(SupernovaSetting), true) as SupernovaSetting;
            if (Setting == null)
            {
                Log.Error("Supernova Setting can not be instantiated");
                return false;
            }

            if (!configured)
            {
                Log.Error("Supernova subsystem has not been configured");
                return true;
            }

            return true;
        }

        protected void InitializeXmlrpcService()
        {
            if (string.IsNullOrEmpty(Setting.IpAddress) || Setting.XmlrpcPort == 0 ||
                string.IsNullOrEmpty(Setting.TccXmlrpcServerAddr) || Setting.TccXmlrpcServerPort == 0)
            {
                Log.Error("invalid xmlrpc service parameters");
                return;
            }

            XmlrpcService = new XmlrpcService(this, Setting.IpAddress, Setting.XmlrpcPort,
                Setting.TccXmlrpcServerAddr, Setting.TccXmlrpcServerPort);
            XmlrpcService.StartService();
        }

        protected void InitializeDirectory()
        {
            try
            {
                if (!Directory.Exists(Settings.Default.RecordingFileDir))
                    Directory.CreateDirectory(Settings.Default.RecordingFileDir);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        protected void InitializeCallHistories()
        {
            _callHistories.FileName = Setting.CallHistoryFile;
            try
            {
                _callHistories.Load();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        protected void InitializeHandset()
        {
            if (LeftHandset != null)
            {
                _leftHandsetInfo = new HandsetInfo(LeftHandset, Resources.IDS_HANDSET_LEFT);
                LeftHandset.OnHandsetOnline += OnHandsetOnline;
                LeftHandset.OnCallStateChanged += OnHandserCallStateChanged;
            }

            if (RightHandset != null)
            {
                _righHandsetInfo = new HandsetInfo(RightHandset, Resources.IDS_HANDSET_RIGHT);
                RightHandset.OnHandsetOnline += OnHandsetOnline;
                RightHandset.OnCallStateChanged += OnHandserCallStateChanged;
            }
        }

        protected bool InitializeServerData(ref string errmsg)
        {
            var stopWatch = new Stopwatch();

            if (string.IsNullOrEmpty(Setting.NidsAddr))
            {
                errmsg = "NIDS服务器地址为空";
                Log.Error(errmsg);
                return false;
            }

            var server = new ServerInfo(Setting.NidsAddr, Setting.NidsInstructPort);

            ClearServerData();

            try
            {
                stopWatch.Start();

                //所有用户
                if (!LoadServerUsersAll(server))
                {
                    throw new Exception();
                }

                Log.Info(string.Format("TimeSpendCounter__InitSvrData.SvrUsersAll__{0}", stopWatch.ElapsedMilliseconds));

                //当前用户
                LocalUser = UserInfo.UserGetByNumber(Setting.AccountUser);

                stopWatch.Restart();

                //Public组
                if (!LoadServerUsersPublic(server))
                {
                    throw new Exception();
                }

                Log.Info(string.Format("TimeSpendCounter__InitSvrData.SvrUsersPublic__{0}",
                    stopWatch.ElapsedMilliseconds));

                //Private组
                if (LocalUser != null)
                {
                    stopWatch.Restart();

                    if (!LoadServerUsersPrivate(server))
                    {
                        throw new Exception();
                    }

                    Log.Info(string.Format("TimeSpendCounter__InitSvrData.SvrUsersPrivate__{0}",
                        stopWatch.ElapsedMilliseconds));
                }

                stopWatch.Restart();

                //在线状态
                LoadServerOnlineUsersInfo(server);

                Log.Info(string.Format("TimeSpendCounter__InitSvrData.OnlineUsers__{0}", stopWatch.ElapsedMilliseconds));
                stopWatch.Restart();

                //通话状态
                LoadServerDialogChannelsInfo(server);

                Log.Info(string.Format("TimeSpendCounter__InitSvrData.DialogInfo__{0}", stopWatch.ElapsedMilliseconds));
                stopWatch.Restart();

                //前缀
                LoadServerPrefixInfo(server);

                Log.Info(string.Format("TimeSpendCounter__InitSvrData.Prefix__{0}", stopWatch.ElapsedMilliseconds));
                stopWatch.Restart();

                //Mixed组
                LoadServerUsersMixed();

                stopWatch.Stop();
                Log.Info(string.Format("TimeSpendCounter__InitSvrData.MixedGroups__{0}", stopWatch.ElapsedMilliseconds));
            }
            catch (Exception ex)
            {
                ClearServerData();
                Log.Error(ex.ToString());
                errmsg = string.Format(Resources.IDS_NIDS_SERVER_ERROR_FORMAT
                    , Setting.NidsAddr);
                return false;
            }

            return true;
        }

        public bool InitializeCustomizeUser()
        {
            if (!File.Exists(Setting.CustomUsersFile))
                return false;

            try
            {
                var serializer = new XmlFileSerializer<List<User>>(Setting.CustomUsersFile);

                var users = serializer.Load();

                UserInfo.CustomizedUsersInitialize(users);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return false;
            }

            return true;
        }

        protected bool InitializeEvents()
        {
            if (string.IsNullOrEmpty(Setting.NidsAddr))
            {
                return false;
            }

            _eventListener = new EventSubscriber
            {
                ServerAddr = Setting.NidsAddr,
                ServrtPort = Setting.NidsEventPort,
                IsDetectServerSwitchover = Setting.IsFailOverDetect,
                EchoPort = Setting.NidsEchoPort,
                PresenceEventTag = Setting.PresenceTag,
                DialogEventTag = Setting.DialogTag,
                HeartbeatEventTag = Setting.HeartbeatTag,
                DialogEventEventUuidPrevious = "-1",
                DialogEventEventUuidCurrent = "-1",
                DialogEventSequencePrevious = "-1",
                DialogEvnetSequenceCurrent = "-1",
                DialogEventServerUuidPrevious = "-1",
                DialogEventServerUuidCurrent = "-1"
            };
            _eventListener.PresenceInfo += OnPresenceUpdated;
            _eventListener.DialogInfo += (sender, args) => DialogQueue.Enqueue(args);
            _eventListener.LogInfo += OnNidsLogInfo;

            DialogDequeueTimer.Interval = 500;
            DialogDequeueTimer.Elapsed += delegate
            {
                try
                {
                    for (var i = 0; i < 10; i++)
                    {
                        if (DialogQueue.Count != 0 && DialogQueue.Peek() != null)
                        {
                            Log.Debug(string.Format("DialogDequeueTimer, before dequeue count: {0}", DialogQueue.Count));
                            var dialog = DialogQueue.Dequeue();
                            Log.Debug(string.Format("DialogDequeueTimer, after dequeue count: {0}", DialogQueue.Count));
                            Application.Current.Dispatcher.Invoke(
                                new Action(() => OnDialogInfoUpdated(this, dialog)));
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            };
            DialogDequeueTimer.Start();

            return _eventListener.StartNidsEvent();
        }

        #endregion

        #region 解初始化

        public override void Uninitialize()
        {
            if (InitResult != InitResult.InitOk)
                return;

            try
            {
                _callHistories.Save();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            if (_eventListener != null)
                _eventListener.StopNIDSEvent();

            if (XmlrpcService != null)
                XmlrpcService.StopService();
        }

        #endregion

        #region On...EventListener

        private void OnPresenceUpdated(object sender, PresenceArgs e)
        {
            var user = UserInfo.UserGetByNumber(e.UserName);
            if (user == null)
            {
                return;
            }

            switch (e.PrecenseAction)
            {
                case PresenceAction.PRESENCE_ACTION_ADD:
                case PresenceAction.PRESENCE_ACTION_UPDTAE:
                    user.IsRegistered = true;
                    break;
                case PresenceAction.PRESENCE_ACTION_DELETE:
                    user.IsRegistered = false;
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void OnDialogInfoUpdated(object sender, ChannelEventArgs args)
        {
            Log.Debug(String.Format("Func:OnDialogInfoUpdated, callback Enter. " +
                                    "From:{0}, To:{1}, Direction:{2}, Event:{3}, uuid:{4}",
                                    args.From, args.To, args.Direction, args.Event, args.UUID));

            //如果丢包，让计时器去重新获取包
            if (args.IsSequenceLost || args.IsEventUuidChanged || args.IsServerUuidChanged)
            {
                Log.Debug(String.Format("Func:OnDialogInfoUpdated, ShallUpdateChannelInfo, " +
                                        "IsSequenceLost:{0}, " +
                                        "IsEventUuidChanged:{1}, " +
                                        "IsServerUuidChanged:{2}",
                    args.IsSequenceLost, args.IsEventUuidChanged, args.IsServerUuidChanged));
                _shallUpdateChannelInfo = true;
                //不需要return；此时的args可以去更新调度台
                //return语句只在测试时使用，为了测试更新状态的计时器，能否正确的更新状态；发布时，务必要取消掉
                //return;
            }

            //Record_start事件
            if (args.Event == ChannelEvent.RECORD_START && XmlrpcService != null &&
                args.From == Setting.AccountUser)
            {
                var channel = Channels.GetChannelByDestination(args.To);
                if (channel != null)
                    XmlrpcService.NotifyRecordFileName(channel.CallId, args.RecordFileName);
                return;
            }

            //构建一个Dialog
            var dialog = new DialogInfo();
            dialog.ChannelUuid = args.UUID;
            dialog.ALegDialog = (args.Direction == ChannelDirection.CHANNEL_DIRECTION_INBOUND);
            dialog.From = args.From;
            dialog.To = args.To;
            dialog.DialogState = args.Event;

            var fromUser = UserInfo.UserGetByNumber(args.From) ??
                           UserInfo.UserGetByNumber(args.From, User.Type.Customized);
            if (fromUser == null)
                fromUser = UserInfo.UserGetByNumber(Setting.OutCallPrefix + args.From) ??
                           UserInfo.UserGetByNumber(Setting.OutCallPrefix + args.From, User.Type.Customized);
            dialog.FromName = fromUser != null ? fromUser.Name : args.From;

            var toUser = UserInfo.UserGetByNumber(args.To) ??
                         UserInfo.UserGetByNumber(args.To, User.Type.Customized);
            if (toUser == null)
                toUser = UserInfo.UserGetByNumber(Setting.OutCallPrefix + args.To) ??
                         UserInfo.UserGetByNumber(Setting.OutCallPrefix + args.To, User.Type.Customized);
            dialog.ToName = toUser != null ? toUser.Name : args.To;

            //因为Dialog还没有加到DialogInfo里面去，所以这个是不大可能会发生的情况
            var existInfo = DialogInfo.GetDialogByUuid(args.UUID);
            if (existInfo != null)
            {
                if (existInfo.From != dialog.From)
                {
                    var user = UserInfo.UserGetByNumber(dialog.From) ??
                               UserInfo.UserGetByNumber(dialog.From, User.Type.Customized);

                    if (user != null)
                        user.DialogInfo.Reset();
                }

                if (existInfo.To != dialog.To)
                {
                    var user = UserInfo.UserGetByNumber(dialog.To) ??
                               UserInfo.UserGetByNumber(dialog.To, User.Type.Customized);

                    if (user != null)
                        user.DialogInfo.Reset();
                }
            }

            //将构建的Dialog，更新到DialogInfo里面去，以便在下面找User对应的Dialog时，可以找到正确的Dialog
            DialogInfo.HandleDialogUpdate(dialog);

            //Inbound更新主叫
            if (args.Direction == ChannelDirection.CHANNEL_DIRECTION_INBOUND && fromUser != null)
            {
                //更新前的User
                Log.Debug(String.Format("Func:OnDialogInfoUpdated, fromUser, before update,\t user details: " +
                                        "fromUser:{0},{1},\t" +
                                        "user.IsAnswered:{2},\t" +
                                        "user.IsInCall:{3},\t" +
                                        "user.IsInbound:{4},\t" +
                                        "user.IsRinging:{5}",
                                        fromUser.Name, fromUser.Number,
                                        fromUser.DialogInfo.IsAnswered,
                                        fromUser.DialogInfo.IsInCall,
                                        fromUser.DialogInfo.IsInbound,
                                        fromUser.DialogInfo.IsRinging));

                var info = DialogInfo.GetCallerDialog(args.From, Setting.OutCallPrefix);
                //要给User更新的Dialog
                if (info != null)
                {
                    Log.Debug(String.Format("Func:OnDialogInfoUpdated, fromUser, before update,\t info details: " +
                                            "fromUser:{0},{1},\t" +
                                            "info.uuid:{2},\t " +
                                            "info.IsDialogAnswered:{3},\t" +
                                            "info.IsDialogHanguped:{4},\t" +
                                            "info.IsDialogRinging:{5}",
                                            fromUser.Name, fromUser.Number,
                                            info.ChannelUuid,
                                            info.IsDialogAnswered,
                                            info.IsDialogHanguped,
                                            info.IsDialogRinging));
                }
                else
                {
                    Log.Debug(String.Format("Func:OnDialogInfoUpdated, fromUser, before update,\t info is null."));
                }

                if (args.From == Setting.AccountUser || args.To == Setting.AccountUser)
                    fromUser.DialogInfo.IsAccountRelated = true;
                UpdateUserDialogInfo(fromUser, info);

                //更新后的User
                Log.Debug(String.Format("Func:OnDialogInfoUpdated, fromUser, after update,\t user details: " +
                                        "fromUser:{0},{1},\t" +
                                        "user.IsAnswered:{2},\t" +
                                        "user.IsInCall:{3},\t" +
                                        "user.IsInbound:{4},\t" +
                                        "user.IsRinging:{5}",
                                        fromUser.Name, fromUser.Number,
                                        fromUser.DialogInfo.IsAnswered,
                                        fromUser.DialogInfo.IsInCall,
                                        fromUser.DialogInfo.IsInbound,
                                        fromUser.DialogInfo.IsRinging));
            }
            else
            {
                Log.Debug(String.Format("Func:OnDialogInfoUpdated，fromUser is null, or not target user. " +
                                        "from:{0}, to:{1}, args.uuid:{2}, args.direct: {3}",
                                        args.From, args.To, args.UUID, args.Direction));
            }

            //Outbound更新被叫
            if (args.Direction == ChannelDirection.CHANNEL_DIRECTION_OUTBOUND && toUser != null)
            {
                //更新前的User
                Log.Debug(String.Format("Func:OnDialogInfoUpdated, toUser, before update,\t user details: " +
                                        "toUser:{0},{1},\t" +
                                        "user.IsAnswered:{2},\t" +
                                        "user.IsInCall:{3},\t" +
                                        "user.IsInbound:{4},\t" +
                                        "user.IsRinging:{5}",
                                        toUser.Name, toUser.Number,
                                        toUser.DialogInfo.IsAnswered,
                                        toUser.DialogInfo.IsInCall,
                                        toUser.DialogInfo.IsInbound,
                                        toUser.DialogInfo.IsRinging));

                var info = DialogInfo.GetCalleeDialog(args.To, Setting.OutCallPrefix);
                //要给User更新的Dialog
                if (info != null)
                {
                    Log.Debug(String.Format("Func:OnDialogInfoUpdated, toUser, before update,\t info details: " +
                                            "toUser:{0}, {1},\t" +
                                            "info.uuid: {2},\t" +
                                            "info.IsDialogAnswered:{3},\t" +
                                            "info.IsDialogHanguped:{4},\t" +
                                            "info.IsDialogRinging:{5}",
                                            toUser.Name, toUser.Number,
                                            info.ChannelUuid,
                                            info.IsDialogAnswered,
                                            info.IsDialogHanguped,
                                            info.IsDialogRinging));
                }
                else
                {
                    Log.Debug(String.Format("Func:OnDialogInfoUpdated, toUser, before update,\t info is null."));
                }

                if (args.From == Setting.AccountUser || args.To == Setting.AccountUser)
                    toUser.DialogInfo.IsAccountRelated = true;
                UpdateUserDialogInfo(toUser, info);

                //更新后的User
                Log.Debug(String.Format("Func:OnDialogInfoUpdated, toUser, after update,\t user details: " +
                                        "toUser:{0},{1},\t" +
                                        "user.IsAnswered:{2},\t" +
                                        "user.IsInCall:{3},\t" +
                                        "user.IsInbound:{4},\t" +
                                        "user.IsRinging:{5}",
                                        toUser.Name, toUser.Number,
                                        toUser.DialogInfo.IsAnswered,
                                        toUser.DialogInfo.IsInCall,
                                        toUser.DialogInfo.IsInbound,
                                        toUser.DialogInfo.IsRinging));
            }
            else
            {
                Log.Debug(String.Format("Func:OnDialogInfoUpdated, toUser is null, or not targetUser: " +
                                        "from{0}, to{1}, args.uuid:{2}, args.direction: {3}",
                                        args.From, args.To, args.UUID, args.Direction));
            }
        }

        private void OnNidsLogInfo(object sender, LogArgs args)
        {
            switch (args.Level)
            {
                case LOGLEVEL.DEBUG:
                    Log.Debug(args.Info);
                    break;
                case LOGLEVEL.INFO:
                    Log.Info(args.Info);
                    break;
                case LOGLEVEL.ERROR:
                    Log.Error(args.Info);
                    break;
            }
        }

        #endregion

        #region On...SIPUAMgr

        private void OnCallStateChanged(object sender, CallStateArgs e)
        {
            Log.Debug("Protocal Stack Log: (SpnvSubSystem)OnCallStateChanged, Start");

            if (AccountId != e.AccountId)
            {
                Log.Debug("Protocal Stack Log: (SpnvSubSystem)OnCallStateChanged, AccountId != e.AccountId, End");
                return;
            }

            var userData = App.SIPUA.GetCallUserData(e.CallId);
            if (userData != 0 && userData != (int)Id)
            {
                Log.Debug(
                    "Protocal Stack Log: (SpnvSubSystem)OnCallStateChanged, userData != 0 && userData != (int)Id, End");
                return;
            }

            if (XmlrpcService != null)
                XmlrpcService.OnCallStateChanged(e.CallId, e.STATE, e.LastStatus);

            var allocateChannel = e.STATE == sua_inv_state.PJSIP_INV_STATE_INCOMING ||
                                  e.STATE == sua_inv_state.PJSIP_INV_STATE_CALLING;

            var channel = GetChannel(e.CallId, allocateChannel);
            if (channel == null)
            {
                Log.Error(string.Format("no channel available for {0}", e.RemoteUri));
                App.SIPUA.Hangup(e.CallId);

                Log.Debug("Protocal Stack Log: (SpnvSubSystem)OnCallStateChanged, No Available Channel, End");

                return;
            }

            channel.CallState = e.STATE;

            if (e.STATE == sua_inv_state.PJSIP_INV_STATE_CONFIRMED)
            {
                channel.StopRing();

                //不论IsRecording的值为何，都进行录制
                //不删除功能代码，只是更改执行流程
                var result = channel.StartRecording(Setting.RecordingFileDir);
                channel.IsAnswered = true;

                Log.Debug(String.Format("Protocal Stack Log: (SpnvSubSystem)OnCallStateChanged, StartRecording:{0}",
                    result));
            }
            else if (e.STATE == sua_inv_state.PJSIP_INV_STATE_DISCONNECTED)
            {
                AddCallHistory(channel);

                var result = channel.StopRecording();
                channel.StopRing();
                channel.Release();

                Log.Debug(String.Format("Protocal Stack Log: (SpnvSubSystem)OnCallStateChanged, StopRecording:{0}",
                    result));
            }

            Log.Debug("Protocal Stack Log: (SpnvSubSystem)OnCallStateChanged, End");
        }

        private void OnRegState(object sender, RegStateArgs e)
        {
            Log.Debug(string.Format("_____SpnvSubSystem__OnRegState__e.StatusCode__{0}", e.StatusCode));
        }
        private void OnInComingCall(object sender, IncomingCallArgs e)
        {
            Log.Debug("Protocal Stack Log: (SpnvSubSystem)OnInComingCall, Start");

            if (AccountId != e.AccountId)
            {
                Log.Debug("Protocal Stack Log: (SpnvSubSystem)OnInComingCall, AccountId != e.AccountId, End");
                return;
            }

            var channel = GetChannel(e.CallId, true);
            if (channel == null)
            {
                Log.Error(string.Format("no channel available for {0}", e.RemoteUri));

                App.SIPUA.Hangup(e.CallId);

                Log.Debug("Protocal Stack Log: (SpnvSubSystem)OnInComingCall, No Available Channel, End");
                return;
            }

            channel.CallState = sua_inv_state.PJSIP_INV_STATE_INCOMING;

            if (Setting.IsAutoAnswer)
            {
                Channels.Answer(e.CallId);

                Log.Debug(String.Format(
                    "Protocal Stack Log: (SpnvSubSystem)OnInComingCall, AutoAnswer, Answer:{0}, End", e.CallId));
            }
            else if (Setting.IsAutoPhoneRedirect)
            {
                App.SIPUA.Redirect(AccountId, e.CallId, Setting.AutoPhoneRedirectNumber);

                Log.Debug(
                    String.Format(
                        "Protocal Stack Log: (SpnvSubSystem)OnInComingCall, AutoRedirect, Redirect:{0},{1},{2}, End",
                        AccountId, e.CallId, Setting.AutoPhoneRedirectNumber));
            }
            else
            {
                App.SIPUA.Ringing(e.CallId);

                Log.Debug(String.Format("Protocal Stack Log: (SpnvSubSystem)OnInComingCall, Ringing:{0}, End", e.CallId));

                Channels.Ring(channel, Setting.GetRingFilePath());
            }

            Log.Debug("Protocal Stack Log: (SpnvSubSystem)OnInComingCall, End");
        }

        private void OnAudioMediaStateChanged(object sender, MediaStateArgs e)
        {
            if (AccountId != e.AccountId)
            {
                return;
            }

            try
            {
                var channel = Channels.GetChannel(e.CallId);
                if (channel == null)
                {
                    return;
                }

                channel.MediaState = e.MEDIASTATE;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private void OnVideoMediaStateChanged(object sender, VideoMediaStateArgs e)
        {
            if (AccountId != e.AccountId)
            {
                return;
            }

            try
            {
                var channel = Channels.GetChannel(e.CallId);
                if (channel == null)
                {
                    return;
                }

                channel.VideoMediaState = e.MEDIASTATE;
                channel.VideoWinId = e.WinId;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private void OnIncomingPagerMessage(object sender, PagerArgs e)
        {
            var uri = new SIPUri(e.From);

            _page.Dispatcher.Invoke(DispatcherPriority.Normal,
                new Action(
                    () => TextMessages.AddMessage(uri.User, e.Msg)
                    ));
        }

        #endregion

        #region 加载服务器信息

        private bool LoadServerUsersAll(ServerInfo server)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            List<User> users = null;
            try
            {
                users = server.GetRemoteUsers();
            }
            catch (Exception)
            {
                return false;
            }

            if (users == null)
            {
                Log.Error("users cannot be retrieved from server.");
                return false;
            }

            Log.Info(string.Format("TimeSpendCounter__SvrUsersAll.Rpc__{0}", stopWatch.ElapsedMilliseconds));
            stopWatch.Restart();

            UserInfo.InitializeUsers(users);
            UserInfo.CreateAllUserGroup();

            stopWatch.Stop();
            Log.Info(string.Format("TimeSpendCounter__SvrUsersAll.Init__{0}", stopWatch.ElapsedMilliseconds));

            return true;
        }

        private bool LoadServerUsersPublic(ServerInfo server)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            List<Group> publicGroups;
            try
            {
                publicGroups = server.GetRemoteGroups();
            }
            catch (Exception)
            {
                return false;
            }

            if (publicGroups == null)
                return false;

            Log.Info(string.Format("TimeSpendCounter__SvrUsersPublic.Rpc__{0}", stopWatch.ElapsedMilliseconds));
            stopWatch.Restart();

            foreach (var group in publicGroups)
            {
                UserInfo.AddGroup(group);
            }

            stopWatch.Stop();
            Log.Info(string.Format("TimeSpendCounter__SvrUsersPublic.Init__{0}", stopWatch.ElapsedMilliseconds));

            return true;
        }

        private bool LoadServerUsersPrivate(ServerInfo server)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            List<Group> privateGroups = null;
            try
            {
                privateGroups = server.GetRemotePrivateGroups(LocalUser.Id);
            }
            catch (Exception)
            {
                return false;
            }

            if (privateGroups == null)
            {
                return false;
            }

            Log.Info(string.Format("TimeSpendCounter__SvrUsersPrivate.Rpc__{0}", stopWatch.ElapsedMilliseconds));
            stopWatch.Restart();

            foreach (var group in privateGroups)
            {
                UserInfo.AddGroup(group);
            }

            stopWatch.Stop();
            Log.Info(string.Format("TimeSpendCounter__SvrUsersPrivate.Init__{0}", stopWatch.ElapsedMilliseconds));

            return true;
        }

        private void LoadServerUsersMixed()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            //-->获得组
            xmlrpcMixedGroup[] mixGroups = null;
            try
            {
                if (LocalUser == null)
                {
                    mixGroups = Proxy.GetMixedGroups();
                }
                else
                {
                    mixGroups = Proxy.GetMixedGroups(LocalUser.Id);
                }
            }
            catch (Exception)
            {
                return;
            }

            Log.Info(string.Format("TimeSpendCounter__SvrUsersMixed.Rpc__{0}", stopWatch.ElapsedMilliseconds));
            stopWatch.Restart();

            if (mixGroups != null)
            {
                foreach (var mixedGroup in mixGroups)
                {
                    try
                    {
                        var tempGroup = new Group
                        {
                            Id = mixedGroup.group_id,
                            GroupType = Group.Type.Mixed,
                            Name = mixedGroup.group_name
                        };

                        //-->由组获得组内成员
                        var tempGroupMembers = Proxy.GetMixedGroupMember(tempGroup.Id);
                        foreach (var member in tempGroupMembers)
                        {
                            var tempUser = UserInfo.UserGetByNumber(member);
                            if (tempUser != null)
                            {
                                //将成员添加到临时组
                                //在GroupSetting界面中，把自定义用户添加到混编组是按照Number来添加的
                                //TCC现场，会出现重启后用户被系统内用户替换的问题。
                                //所以，此处也该为按照Number添加，尝试下。
                                //tempGroup.AddUser(tempUser.Id);\
                                tempGroup.AddUser(tempUser.Number);
                            }
                        }

                        //将临时组添加到混编组
                        UserInfo.AddGroup(tempGroup);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            stopWatch.Stop();
            Log.Info(string.Format("TimeSpendCounter__SvrUsersMixed.Init__{0}", stopWatch.ElapsedMilliseconds));
        }

        private void LoadServerOnlineUsersInfo(ServerInfo proxy)
        {
            try
            {
                foreach (var userNum in proxy.GetOnlineUsers())
                {
                    var user = UserInfo.UserGetByNumber(userNum);
                    if (user != null)
                        user.IsRegistered = true;
                }
            }
            catch (Exception)
            {
            }
        }

        private void LoadServerDialogChannelsInfo(ServerInfo proxy)
        {
            xmlrpcChannel[] channels;
            try
            {
                channels = proxy.GetChannels();
            }
            catch (Exception)
            {
                return;
            }

            if (channels == null)
            {
                return;
            }

            foreach (var channel in channels)
            {
                var channelEvent = SigMediaEvent.GetChannelEventbyChannelState(channel.channel_state);
                var direct = SigMediaEvent.GetChannelDirection(channel.channel_direct);

                var args = new ChannelEventArgs(channelEvent, direct,
                    channel.uuid, channel.channel_caller, channel.channel_callee);
                OnDialogInfoUpdated(this, args);
            }
        }

        private void LoadServerPrefixInfo(ServerInfo proxy)
        {
            var prefix = proxy.GetPrefix();

            PrefixInfo.AddPrefix(DialPrefixType.DialIntercept, prefix.intercept_prefix, Resources.IDS_PREFIX_INTERCEPTE);
            PrefixInfo.AddPrefix(DialPrefixType.DialThreeway, prefix.threeway_prefix, Resources.IDS_PREFIX_THREEWAY);
            PrefixInfo.AddPrefix(DialPrefixType.DialEavesdrop, prefix.eavesdrop_prefix, Resources.IDS_PREFIX_EAVESDROP);
            PrefixInfo.AddPrefix(DialPrefixType.DialIntercom, prefix.intercom_prefix);
            PrefixInfo.AddPrefix(DialPrefixType.DialPickup, prefix.pickup_prefix);
            PrefixInfo.AddPrefix(DialPrefixType.DialConference, prefix.conference_prefix, Resources.IDS_PREFIX_CONF);
            PrefixInfo.AddPrefix(DialPrefixType.DialPaging, prefix.paging_prefix, Resources.IDS_PREFIX_PAGE);
            PrefixInfo.AddPrefix(DialPrefixType.DialFindcall, prefix.findcall_prefix, Resources.IDS_PREFIX_FINDCALL);
            PrefixInfo.AddPrefix(DialPrefixType.DialBroadcast, prefix.broadcast_prefix, Resources.IDS_PREFIX_BROADCAST);
        }

        #endregion

        #region Timer

        private void UpdateChannelInfoTimerEx(object sender, ElapsedEventArgs e)
        {
            if (!_shallUpdateChannelInfo)
            {
                var dialogs = GetServerDialogs();
                if (dialogs == null || !dialogs.Any())
                {
                    Log.Debug("UpdateChannelInfoTimerEx, server dialog null. gonna reset all");
                    DialogInfo.ClearAllDialog();
                    foreach (var user in UserInfo.Users.Where(user => user.DialogInfo.IsInCall))
                    {
                        user.DialogInfo.Reset();
                    }
                }
                else
                {
                    Log.Debug("UpdateChannelInfoTimerEx, server dialog not null. but no packet lost or server exchanged");
                }
                return;
            }

            _dialogUpdateTimer.Stop();

            Log.Debug("UpdateChannelInfoTimerEx");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            DelFromLocalDialogs();
            AddToLocalDialogs();

            _shallUpdateChannelInfo = false;

            stopWatch.Stop();
            Log.Info(string.Format("TimeSpendCounter__UpdateChannelInfoTimerEx__{0}", stopWatch.ElapsedMilliseconds));

            _dialogUpdateTimer.Start();
        }

        /// <summary>
        /// 从本地通话中删除不存在于服务器的通话
        /// </summary>
        private void DelFromLocalDialogs()
        {
            var serverDialogs = GetServerDialogs();
            if (serverDialogs == null || !serverDialogs.Any())
            {
                Log.Debug("DelFromServerDialogs, serverDialogs null or 0 count. nothing to delete. RETURN");
                DialogInfo.ClearAllDialog();
                foreach (var user in UserInfo.Users.Where(user => user.DialogInfo.IsInCall))
                {
                    user.DialogInfo.Reset();
                }
                return;
            }
            var serverUuids = serverDialogs.Select(dialog => dialog.uuid);
            var localDialogs = GetLocalDialogs();
            if (localDialogs == null || !localDialogs.Any())
            {
                Log.Debug("DelFromLocalDialogs, localDialogs null or 0 count. nothing to delete. RETURN");
                return;
            }
            var localUuids = localDialogs.Select(dialog => dialog.Key);
            //delete
            var delUuids = localUuids.Where(localUuid => !serverUuids.Contains(localUuid)).ToList();
            if (!delUuids.Any())
            {
                Log.Debug("DelFromLocalDialogs, delUuids count 0, nothing to delete. RETURN");
                return;
            }
            foreach (var delUuid in delUuids)
            {
                var args = new ChannelEventArgs
                {
                    Event = ChannelEvent.CHANNEL_HANGUP,
                    Direction = ChannelDirection.CHANNEL_DIRECTION_UNKOWN,
                    UUID = delUuid,
                    From = localDialogs[delUuid].From,
                    To = localDialogs[delUuid].To,
                    IsEventUuidChanged = false,
                    IsSequenceLost = false,
                    IsServerUuidChanged = false
                };
                Log.Debug(String.Format("DelFromLocalDialogs, fake publish hangup event:" +
                                        "event: hangup, direction: unknown, uuid:{0}, from:{1}, to:{2}", args.UUID,
                                        args.From, args.To));
                Application.Current.Dispatcher.Invoke(new Action(() => OnDialogInfoUpdated(this, args)));
            }

            //update
            var updateUuids = serverUuids.Intersect(localUuids.ToList()).ToList();
            if (!updateUuids.Any())
            {
                Log.Debug("DelFromLocalDialogs:updateLocalUuids: !updateUuids.Any()==true");
                return;
            }
            try
            {
                foreach (var updateUuid in updateUuids)
                {
                    var serverDialog = serverDialogs.FirstOrDefault(dialog => dialog.uuid == updateUuid);
                    var localDialog = localDialogs.FirstOrDefault(dialog => dialog.Key == updateUuid).Value;
                    if (serverDialog == null || localDialog == null)
                    {
                        Log.Error("DelFromLocalDialogs:updateLocalUuids: serverDialog == null || localDialog == null");
                        return;
                    }
                    if (SigMediaEvent.GetChannelEventbyChannelState(serverDialog.channel_state) != localDialog.DialogState)
                    {
                        Log.Debug("DelFromLocalDialogs:updateLocalUuids;");
                        var args = new ChannelEventArgs()
                        {
                            IsEventUuidChanged = false,
                            IsSequenceLost = false,
                            IsServerUuidChanged = false,
                            UUID = updateUuid,
                            From = serverDialog.channel_caller,
                            To = serverDialog.channel_callee,
                            Event = SigMediaEvent.GetChannelEventbyChannelState(serverDialog.channel_state),
                            Direction = SigMediaEvent.GetChannelDirection(serverDialog.channel_direct),
                        };
                        Log.Debug(String.Format("updateLocalDialogs, fake public event:" +
                                                "event: {0}, direction: {1}, uuid:{2}, from:{3}, to:{4}",
                                                args.Event, args.Direction, args.UUID, args.From, args.To));
                        Application.Current.Dispatcher.Invoke(new Action(() => OnDialogInfoUpdated(this, args)));
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception);
            }
            

        }

        /// <summary>
        /// 将服务器存在、本地不存在的通话，同步到本地
        /// </summary>
        private void AddToLocalDialogs()
        {
            var serverChannels = GetServerDialogs();
            if (serverChannels == null || !serverChannels.Any())
            {
                Log.Debug("AddToLocalDialogs, server dialogs 0 count. RETURN");
                return;
            }

            var localUuids = GetLocalDialogUuids();
            if (localUuids == null)
            {
                Log.Warn("AddToLocalDialogs, local dialogs null. RETURN");
            }

            foreach (var serverChannel in serverChannels)
            {
                if (localUuids != null && localUuids.Contains(serverChannel.uuid))
                {
                    Log.Debug(string.Format("AddToLocalDialogs, uuid: {0} already exist on local", serverChannel.uuid));
                    continue;
                }

                var args = new ChannelEventArgs()
                {
                    IsEventUuidChanged = false,
                    IsSequenceLost = false,
                    IsServerUuidChanged = false,
                    UUID = serverChannel.uuid,
                    From = serverChannel.channel_caller,
                    To = serverChannel.channel_callee,
                    Event = SigMediaEvent.GetChannelEventbyChannelState(serverChannel.channel_state),
                    Direction = SigMediaEvent.GetChannelDirection(serverChannel.channel_direct),
                };
                Log.Debug(String.Format("AddToLocalDialogs, fake public event:" +
                                        "event: {0}, direction: {1}, uuid:{2}, from:{3}, to:{4}",
                                        args.Event, args.Direction, args.UUID, args.From, args.To));
                Application.Current.Dispatcher.Invoke(new Action(() => OnDialogInfoUpdated(this, args)));
            }
        }

        /// <summary>
        /// 获取本地通话的字典
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, DialogInfo> GetLocalDialogs()
        {
            Log.Debug("GetLocalDialogs");

            var localDialogs = DialogInfo.GetAllDialog();
            foreach (var localDialog in localDialogs)
            {
                Log.Debug(String.Format("GetLocalDialogs, local dialog, " +
                                        "key:{0}, from:{1}, to:{2}, state:{3}",
                    localDialog.Key, localDialog.Value.From, localDialog.Value.To, localDialog.Value.DialogState));
            }
            if (!localDialogs.Any())
            {
                Log.Debug("GetLocalDialogs, no dialog on local, nothing to delete. RETURN");
                return null;
            }

            return localDialogs;
        }

        /// <summary>
        /// 获取本地通话UUID的列表
        /// </summary>
        /// <returns></returns>
        private List<string> GetLocalDialogUuids()
        {
            var localDialogs = GetLocalDialogs();
            if (localDialogs == null || !localDialogs.Any())
            {
                Log.Debug("GetLocalDialogUuids, localDialogs null or 0 count. REUTRN NULL");
                return null;
            }

            Log.Debug("GetLocalDialogUuids");

            return localDialogs.Keys.ToList();
        }

        /// <summary>
        /// 获取服务器的通话列表
        /// </summary>
        /// <returns></returns>
        private List<xmlrpcChannel> GetServerDialogs()
        {
            xmlrpcChannel[] serverDialogs;
            try
            {
                serverDialogs = Proxy.GetChannels();
                foreach (var serverChannel in serverDialogs)
                {
                    Log.Debug(String.Format("GetServerDialogs. server channels," +
                                            "caller:{0}, callee:{1}, direct:{2}, state:{3}, uuid:{4}",
                        serverChannel.channel_caller, serverChannel.channel_callee,
                        serverChannel.channel_direct, serverChannel.channel_state, serverChannel.uuid));
                }
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("GetServerDialogs. get server channels. " +
                                  "exception caught:{0}. RETURN", ex));
                return null;
            }
            if (!serverDialogs.Any())
            {
                Log.Debug("GetServerDialogs, no dialog on server. gonna clear local dialogs. RETURN");
                return null;
            }

            return serverDialogs.ToList();
        }

        /// <summary>
        /// 获取服务器通话的UUID列表
        /// </summary>
        /// <returns></returns>
        private List<string> GetServerDialogUuids()
        {
            var serverDialogs = GetServerDialogs();
            if (serverDialogs == null || !serverDialogs.Any())
            {
                Log.Debug("GetServerDialogUuids, serverDialogs null or 0 count. RETURN");
                return null;
            }

            Log.Debug("GetServerDialogUuids");
            return serverDialogs.Select(dialog => dialog.uuid).ToList();
        }


        private void CallHistorySaveTimer(object sender, EventArgs e)
        {
            Log.Debug("Func: CallHistorySaveTimer. Enter");
            try
            {
                Log.Debug("Func: CallHistorySaveTimer. save, begin");
                _callHistories.Save();
                Log.Debug("Func: CallHistorySaveTimer. save, end");
            }
            catch (Exception ex)
            {
                Log.Debug(String.Format("Func: CallHistorySaveTimer. save, exception caught:{0}", ex));
            }
            Log.Debug("Func: CallHistorySaveTimer. Leave");
        }

        #endregion

        private void OnHandserCallStateChanged(object sender, HandsetCallStateArgs arg)
        {
            Log.Debug("OnHandserCallStateChanged");

            var handset = sender as Handset.Handset;
            if (handset == null)
                return;

            var info = _leftHandsetInfo.Id == handset.Id ? _leftHandsetInfo : _righHandsetInfo;

            if (!string.IsNullOrEmpty(arg.RemoteUri))
            {
                var uri = new SIPUri(arg.RemoteUri);
                if (uri.IsParsedError == false)
                {
                    string disName = null, dispNum = null;
                    GetDestDisplayinfo(uri.User, ref disName, ref dispNum);

                    info.PartyNumber = uri.User;
                    info.PartyDisplayName = disName;
                    info.PartyDisplayNumber = dispNum;
                }
                else
                {
                    info.PartyNumber = arg.RemoteUri;
                    info.PartyDisplayName = arg.RemoteUri;
                    info.PartyDisplayNumber = string.Empty;
                }
                info.Role = arg.Role;
            }
            else
            {
                Log.Error("empty ");
            }

            switch (arg.State)
            {
                case sua_inv_state.PJSIP_INV_STATE_CALLING:
                case sua_inv_state.PJSIP_INV_STATE_EARLY:
                case sua_inv_state.PJSIP_INV_STATE_CONNECTING:
                    info.IsBusy = true;
                    info.IsRinging = true;
                    break;
                case sua_inv_state.PJSIP_INV_STATE_CONFIRMED:
                    info.IsBusy = true;
                    info.IsRinging = false;
                    info.IsAnswered = true;
                    break;
                case sua_inv_state.PJSIP_INV_STATE_DISCONNECTED:
                    AddCallHistory(info);
                    info.ResetState();
                    break;
            }
        }

        private void OnHandsetOnline(object sender, HandsetOnlineArgs args)
        {
            if (_leftHandsetInfo.Id == args.Id)
            {
                _leftHandsetInfo.IsOnLine = true;
            }
            else
            {
                _righHandsetInfo.IsOnLine = true;
            }
        }

        /// <summary>
        ///     将DialogInfo更新至User
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void UpdateUserDialogInfo(User user, DialogInfo info)
        {
            if (info != null)
            {
                Log.Debug(String.Format("UpdateUserDialogInfo, before update,\t user details: " +
                                        "userName:{0}, userNumber:{1}, " +
                                        "info.uuid:{2}, " +
                                        "DialogInfo.IsAnswered:{3}, " +
                                        "DialogInfo.IsInCall:{4}, " +
                                        "DialogInfo.IsInbound:{5}, " +
                                        "DialogInfo.IsRinging:{6}, ",
                    user.Name, user.Number,
                    info.ChannelUuid,
                    user.DialogInfo.IsAnswered,
                    user.DialogInfo.IsInCall,
                    user.DialogInfo.IsInbound,
                    user.DialogInfo.IsRinging));
            }
            else
            {
                Log.Debug(
                    string.Format("UpdateUserDialogInfo, info is null, \tuserName:{0}, userNumber:{1}. Reset, Leave",
                        user.Name, user.Number));
                user.DialogInfo.Reset();
                return;
            }

            if (info.DialogState == ChannelEvent.RECORD_START)
            {
                Log.Debug(string.Format(
                    "UpdateUserDialogInfo, userName:{0}, userNumber:{1}, Event: RECORD_START, Leave", user.Name,
                    user.Number));
                return;
            }

            if (info.IsDialogHanguped)
            {
                Log.Debug(
                    string.Format("UpdateUserDialogInfo, info is Hangup, \tuserName:{0}, userNumber:{1}, Reset, Leave",
                        user.Name, user.Number));
                user.DialogInfo.Reset();
                return;
            }

            if (String.CompareOrdinal(user.Number, info.From) == 0)
            {
                user.DialogInfo.IsInbound = false;
                user.DialogInfo.PartyNumber = info.To;
                user.DialogInfo.PartyName = info.ToName;

                string dispNum = string.Empty, dispName = string.Empty;
                GetDestDisplayinfo(info.To, ref dispName, ref dispNum);

                user.DialogInfo.PartyDisplayName = dispName;
                user.DialogInfo.PartyDisplayNumber = dispNum;
            }
            else
            {
                user.DialogInfo.IsInbound = true;
                user.DialogInfo.PartyNumber = info.From;
                user.DialogInfo.PartyName = info.FromName;

                string dispNum = string.Empty, dispName = string.Empty;
                GetDestDisplayinfo(info.From, ref dispName, ref dispNum);

                user.DialogInfo.PartyDisplayNumber = dispNum;
                user.DialogInfo.PartyDisplayName = dispName;
            }

            user.DialogInfo.IsRinging = (info.DialogState == ChannelEvent.CHANNEL_OUTGOING ||
                                         info.DialogState == ChannelEvent.CHANNEL_CREATE);
            user.DialogInfo.IsAnswered = (info.DialogState == ChannelEvent.CHANNEL_ANSWERED);
            user.DialogInfo.IsInCall = true;

            Log.Debug(String.Format("UpdateUserDialogInfo, after update,\t user details: " +
                                    "userName:{0}, userNumber:{1}, " +
                                    "info.uuid:{2}, " +
                                    "DialogInfo.IsAnswered:{3}, " +
                                    "DialogInfo.IsInCall:{4}, " +
                                    "DialogInfo.IsInbound:{5}, " +
                                    "DialogInfo.IsRinging:{6}, ",
                user.Name, user.Number,
                info.ChannelUuid,
                user.DialogInfo.IsAnswered,
                user.DialogInfo.IsInCall,
                user.DialogInfo.IsInbound,
                user.DialogInfo.IsRinging));
        }

        private void ClearServerData()
        {
            UserInfo.ClearUserInfo();
        }

        public bool SyncPrivateGroupsInfo(List<Group> delGroups)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var proxy = new SpnvRemoteProc(Setting.NidsAddr, Setting.NidsInstructPort);

            var local = UserInfo.UserGetByNumber(Setting.AccountUser);
            if (local == null)
                return false;

            try
            {
                foreach (var group in UserInfo.PrivateGroups)
                {
                    var modified = false;

                    if (group.IsPendingNew)
                    {
                        group.Id = (int)proxy.CreatePrivateGroup(group.Name, local.Id);
                        group.IsPendingNew = false;
                        modified = true;
                    }

                    if (group.IsPendingModified)
                    {
                        proxy.ClearPrivateGroup(group.Id);
                        group.IsPendingModified = false;
                        modified = true;
                    }

                    if (modified == false)
                        continue;

                    foreach (var user in group.GroupUsers)
                    {
                        proxy.AddPrivateGroupMember(group.Id, user.Id);
                    }
                }

                foreach (var group in delGroups)
                {
                    proxy.DeletePrivateGroup(group.Id, local.Id);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw new Exception(Resources.IDS_PRIVATE_GROUP_SYNC_ERR);
            }

            stopWatch.Stop();
            Log.Info(string.Format("TimeSpendCounter__SyncPrivateGroupsInfo__{0}", stopWatch.ElapsedMilliseconds));

            return true;
        }

        private Channel GetChannel(int callId, bool allocate)
        {
            Channel channel = null;

            try
            {
                channel = Channels.GetChannel(callId);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }

            if (channel == null && allocate)
            {
                channel = Channels.GetIdleChannel();

                var info = new sua_call_info();
                App.SIPUA.GetCallInfo(callId, ref info);

                //channel.CallId = callId;
                channel.CallId = info.id;
                channel.IsIncomingCall = ((sua_role_e)info.call_role == sua_role_e.SUA_ROLE_UAS);

                var uri = new SIPUri(info.remote_uri);
                var number = uri.User;
                channel.CallDestNum = number;

                string dispName = string.Empty, dispNum = string.Empty;
                GetDestDisplayinfo(number, ref dispName, ref dispNum);

                channel.CallPartyDisplayName = dispName;
                channel.CallPartyDisplayNumber = dispNum;
                channel.IsGroupCall = PrefixInfo.IsGroupCallPrefix(number);
            }

            return channel;
        }

        private void AddCallHistory(Channel channel)
        {
            if (channel.IsRediect2Handset)
                return;

            var item = new HistoryItem
            {
                PartyDispName = channel.CallPartyDisplayName,
                PartyDispNumber = channel.CallPartyDisplayNumber,
                PartyNumber = channel.CallDestNum,
                DateTime = DateTime.Now,
                IsInbound = channel.IsIncomingCall,
                IsAnswered = channel.IsAnswered
            };

            _callHistories.AddHistory(item);
        }

        private string GetGroupName(string dialnum)
        {
            var groupType = PrefixInfo.GetGroupPrefixType(dialnum);
            if (groupType == DialPrefixGroup.DialGroupNone)
            {
                return null;
            }

            Group.Type type;
            switch (groupType)
            {
                case DialPrefixGroup.DialGroupPublic:
                    type = Group.Type.Public;
                    break;
                case DialPrefixGroup.DialGroupAll:
                    type = Group.Type.AllUser;
                    //当呼叫全体用户时，id为002，导致返回group为null，函数返回值为-1，所以单独处理(lyc)
                    return Resources.IDS_GROUP_ALLUSER;
                //break;
                case DialPrefixGroup.DialGroupPrivate:
                    type = Group.Type.Private;
                    break;
                case DialPrefixGroup.DialGroupMixed:
                    type = Group.Type.Mixed;
                    break;
                default:
                    return null;
            }

            var id = PrefixInfo.GetGroupId(dialnum);
            var group = UserInfo.GroupGet(id, type);
            return group != null ? group.Name : id.ToString(CultureInfo.InvariantCulture);
        }

        public void GetDestDisplayinfo(string dialNum, ref string dispName, ref string dispNum)
        {
            dispName = dialNum;
            dispNum = dialNum;

            var type = PrefixInfo.GetPrefixType(dialNum);
            var prefixname = PrefixInfo.GetPrefixName(type);

            if (PrefixInfo.IsGroupCallPrefix(type))
            {
                var groupName = GetGroupName(dialNum);
                if (!string.IsNullOrEmpty(groupName))
                {
                    dispName = prefixname + groupName;
                    dispName = dispName.Trim();
                    dispNum = string.Empty;
                }
            }
            else
            {
                dialNum = PrefixInfo.StripDialPrefix(dialNum);
                var user = UserInfo.UserGetByNumber(dialNum);
                if (user != null)
                {
                    dispName = prefixname + user.Name;
                    dispName = dispName.Trim();
                    if (type != DialPrefixType.DialNoprefix)
                        dispNum = string.Empty;
                }
            }
        }

        private void AddCallHistory(HandsetInfo info)
        {
            var item = new HistoryItem
            {
                PartyDispName = info.PartyDisplayName,
                PartyDispNumber = info.PartyDisplayNumber,
                PartyNumber = info.PartyNumber,
                IsInbound = (info.Role == sua_role_e.SUA_ROLE_UAS),
                IsAnswered = info.IsAnswered,
                DateTime = DateTime.Now
            };
            _callHistories.AddHistory(item);
        }

        public Handset.Handset GetIdleHandset()
        {
            if (!LeftHandsetInfo.IsBusy)
                return LeftHandset;

            if (!RightHandsetInfo.IsBusy)
                return RightHandset;

            return null;
        }
    }
}