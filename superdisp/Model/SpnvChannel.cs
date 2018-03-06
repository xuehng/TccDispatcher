using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using renstech.NET.PJSIP;
using renstech.NET.SIPUA;
using renstech.NET.nids.RemoteProc;

namespace renstech.NET.SupernovaDispatcher.Model
{
    public class Channel : INotifyPropertyChanged
    {
        private int _callId;
        private bool _blinking;
        private bool _ringing;
        private bool _connected;
        private bool _hold;
        private bool _hasvideo;
        private bool _cananswer;
        private bool _canhold;
        private bool _incomingcall;
        private string _callusername;
        private string _callusernumber;
        private sua_inv_state _callstate;
        private sua_call_media_status _mediastate;
        private sua_call_media_status _videomediastate;
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Channel));

        public Channel(Button button, int id)
        {
            ChannelId = id;
            ChannelButton = button;

            _callId = -1;
            _callstate = sua_inv_state.PJSIP_INV_STATE_NULL;
            _mediastate = sua_call_media_status.SUA_CALL_MEDIA_NONE;
            _videomediastate = sua_call_media_status.SUA_CALL_MEDIA_NONE;

            RecorderId = -1;
            RingPlayId = -1;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        public int ChannelId { get; private set; }
        public Button ChannelButton { get; private set; }
        public int RecorderId { get; private set; }
        public int RingPlayId { get; private set; }
        public int VideoWinId { get; set; }

        public bool IsGroupCall { get; set; }
        
        public bool IsChannelBusy 
        { 
            get { return (CallId != -1); } 
        }

        public bool IsIncomingCall
        {
            get { return _incomingcall; }
            set
            {
                _incomingcall = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsIncomingCall"));
            }
        }

        public bool IsRediect2Handset { get; set; }
        public bool IsAnswered { get; set; }
        public string Name { get; set; }

        public string CallPartyDisplayName
        {
            get { return _callusername; }
            set
            {
                _callusername = value;
                OnPropertyChanged(new PropertyChangedEventArgs("CallPartyDisplayName"));
            }
        }

        public string CallPartyDisplayNumber
        {
            get { return _callusernumber; }
            set
            {
                _callusernumber = value;
                OnPropertyChanged(new PropertyChangedEventArgs("CallPartyDisplayNumber"));
            }
        }

        public string CallDestNum { get; set; }

        public int CallId
        {
            get { return _callId; }
            set
            {
                _callId = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsChannelBusy"));
            }
        }

        public sua_inv_state CallState
        {
            get { return _callstate; }
            set
            {
                _callstate = value;

                switch (_callstate)
                {
                    case sua_inv_state.PJSIP_INV_STATE_CALLING:
                    case sua_inv_state.PJSIP_INV_STATE_EARLY:
                    case sua_inv_state.PJSIP_INV_STATE_CONNECTING:
                    case sua_inv_state.PJSIP_INV_STATE_INCOMING:
                        if (IsIncomingCall)
                        {
                            IsStateBlinking = true;
                        }
                        else
                        {
                            IsStateRinging = true;
                        }
                        break;
                    case sua_inv_state.PJSIP_INV_STATE_CONFIRMED:
                        IsStateRinging = false;
                        IsStateBlinking = false;
                        IsStateConnected = true;
                        break;
                    case sua_inv_state.PJSIP_INV_STATE_DISCONNECTED:
                        IsStateConnected = false;
                        IsStateBlinking = false;
                        IsStateRinging = false;
                        IsStateHold = false;
                        break;
                }

                switch (_callstate)
                {
                    case sua_inv_state.PJSIP_INV_STATE_INCOMING:
                    case sua_inv_state.PJSIP_INV_STATE_EARLY:
                        CanAnswer = true;
                        CanHold = false;
                        break;
                    case sua_inv_state.PJSIP_INV_STATE_CONFIRMED:
                        CanAnswer = false;
                        CanHold = true;
                        break;
                    default:
                        CanAnswer = false;
                        CanHold = false;
                        break;
                }
            }
        }

        public sua_call_media_status MediaState
        {
            get { return _mediastate; }
            set
            {
                _mediastate = value;

                switch (_mediastate)
                {
                    case sua_call_media_status.SUA_CALL_MEDIA_LOCAL_HOLD:
                        IsStateHold = true;
                        break;
                    case sua_call_media_status.SUA_CALL_MEDIA_ACTIVE:
                        IsStateHold = false;
                        break;
                }
            }
        }

        public sua_call_media_status VideoMediaState
        {
            get { return _videomediastate; }
            set
            {
                _videomediastate = value;

                HasVideo = _videomediastate == sua_call_media_status.SUA_CALL_MEDIA_ACTIVE;
            }
        }

        public bool IsStateBlinking
        {
            get { return _blinking; }
            set
            {
                _blinking = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsStateBlinking"));
            }
        }

        public bool IsStateRinging
        {
            get { return _ringing; }
            set
            {
                _ringing = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsStateRinging"));
            }
        }

        public bool IsStateConnected
        {
            get { return _connected; }
            set
            {
                _connected = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsStateConnected"));
            }
        }

        public bool IsStateHold
        {
            get { return _hold; }
            set
            {
                _hold = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsStateHold"));
            }
        }

        public bool CanAnswer
        {
            get { return _cananswer; }
            set
            {
                _cananswer = value;
                OnPropertyChanged(new PropertyChangedEventArgs("CanAnswer"));
            }
        }

        public bool CanHold
        {
            get { return _canhold; }
            set
            {
                _canhold = value;
                OnPropertyChanged(new PropertyChangedEventArgs("CanHold"));
            }
        }

        public bool HasVideo
        {
            get { return _hasvideo;  }
            set
            {
                _hasvideo = value;
                OnPropertyChanged(new PropertyChangedEventArgs("HasVideo"));
            }
        }

        public void Reset()
        {
            _callId = -1;
            _callstate = sua_inv_state.PJSIP_INV_STATE_NULL;
            _mediastate = sua_call_media_status.SUA_CALL_MEDIA_NONE;
            _videomediastate = sua_call_media_status.SUA_CALL_MEDIA_NONE;

            CallPartyDisplayName = string.Empty;
            CallPartyDisplayNumber = string.Empty;

            RecorderId = -1;
            RingPlayId = -1;

            IsAnswered = false;
            IsRediect2Handset = false;
            IsGroupCall = false;
        }


        public void Release()
        {
            CallId = -1;
            RingPlayId = -1;
            CallPartyDisplayName = "";
            CallPartyDisplayNumber = "";
            IsGroupCall = false;
            IsIncomingCall = false;
            CallState = sua_inv_state.PJSIP_INV_STATE_NULL;
            MediaState = sua_call_media_status.SUA_CALL_MEDIA_NONE;
            VideoMediaState = sua_call_media_status.SUA_CALL_MEDIA_NONE;
        }

        public bool Ring(string file)
        {
            Log.Debug("Protocal Stack Log: (Channel)Ring, Start");

            if (CallId == -1)
            {
                Log.Debug("Protocal Stack Log: (Channel)Ring, CallId == -1, False End");
                return false;
            }
                

            if (RingPlayId != -1)
            {
                Log.Debug("Protocal Stack Log: (Channel)Ring, RingPlayId != -1, False End");
                return false;
            }

            if (string.IsNullOrEmpty(file))
            {
                Log.Debug("Protocal Stack Log: (Channel)Ring, wrong file, False End");
                return false;
            }

            RingPlayId = App.SIPUA.StartRing(CallId, file);

            Log.Debug(String.Format("Protocal Stack Log: (Channel)Ring, StartRing:{0},{1}", CallId, file));

            Log.Debug("Protocal Stack Log: (Channel)Ring, End");

            return (RingPlayId != -1);
        }

        public void StopRing()
        {
            Log.Debug("Protocal Stack Log: (Channel)StopRing, Start");

            if (RingPlayId == -1)
                return;

            App.SIPUA.StopRing(RingPlayId);

            Log.Debug(String.Format("Protocal Stack Log: (Channel)StopRing, StopRing:{0}", RingPlayId));

            RingPlayId = -1;

            Log.Debug("Protocal Stack Log: (Channel)StopRing, End");
        }

        public bool StartRecording(string dir)
        {
            if (!IsChannelBusy || RecorderId != -1)
                return false;

            sua_call_info info = new sua_call_info();
            App.SIPUA.GetCallInfo(CallId, ref info);

            SIPUri remteUri = new SIPUri(info.remote_uri);
            SIPUri localUri = new SIPUri(info.local_uri);

            string caller, callee;

            if ((sua_role_e)info.call_role == sua_role_e.SUA_ROLE_UAS)
            {
                caller = remteUri.User;
                callee = localUri.User;
            }
            else
            {
                callee = remteUri.User;
                caller = localUri.User;
            }

            string filename = RecordingFiles.BuildRecordingFileName(dir, DateTime.Now, caller, callee);

            int recordId = -1;
            if (App.SIPUA.StartRecording(CallId, filename, ref recordId))
            {
                RecorderId = recordId;
                return true;
            }

            return false;
        }

        public bool StopRecording()
        {
            if (RecorderId == -1)
                return false;

            App.SIPUA.StopRecord(RecorderId);
            RecorderId = -1;
            return true;
        }

    }

    public class SpnvChannels
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(SpnvChannels));

        private readonly SpnvSubSystem _system;
        private readonly List<Channel> _channels = new List<Channel>();

        public SpnvChannels(SpnvSubSystem system)
        {
            _system = system;
        }

        public List<Channel> Channels { get { return _channels; } }

        public Channel AddChannel(Button channelButton)
        {
            Channel channel = new Channel(channelButton, _channels.Count);
            _channels.Add(channel);
            channelButton.DataContext = channel;
            return channel;
        }

        public Channel GetChannel(Button button)
        {
            try
            {
                return _channels.FirstOrDefault(channel => channel.ChannelButton == button);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
        }

        public Channel GetChannel(int callId)
        {
            try
            {
                return _channels.FirstOrDefault(channel => channel.CallId == callId);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
        }

        public Channel GetChannelByDestination(string number)
        {
            try
            {
                return _channels.FirstOrDefault(channel => channel.IsChannelBusy == true && channel.CallDestNum == number);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
        }

        public Channel GetIdleChannel()
        {
            foreach (Channel channel in _channels.Where(channel => channel.IsChannelBusy == false))
            {
                channel.Reset();
                return channel;
            }
            return null;
        }

        public Channel GetActiveChannel()
        {
            try
            {
                return _channels.FirstOrDefault(channel => channel.IsStateConnected && !channel.IsStateHold);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
        }

        public Channel GetInboundRingingChannel()
        {
            try
            {
                return _channels.FirstOrDefault(channel => channel.IsStateBlinking && channel.IsIncomingCall);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
        }

        public Channel GetGroupCallChannel()
        {
            try
            {
                return _channels.FirstOrDefault(channel => channel.IsChannelBusy && channel.IsGroupCall);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
        }

        public Channel GetChannel(DialPrefixType type)
        {
            foreach (Channel channel in _channels)
            {
                if (!channel.IsChannelBusy)
                    continue;

                string remote = channel.CallDestNum;

                DialPrefixType tmp = _system.PrefixInfo.GetPrefixType(remote);
                if (tmp == type)
                    return channel;
            }
            return null;
        }

        public void Answer(int callId)
        {
            HoldActiveCall();

            App.SIPUA.Answer(callId);
        }

        public bool Ring(Channel channel, string file)
        {
            Channel active = GetActiveChannel();
            if (active != null)
            {
                return false;
            }

            channel.Ring(file);
            return true;
        }

        public bool MakeCall(int accoutId, string dest, ref int callId, bool nogroupcall = false)
        {
            Log.Debug("Protocal Stack Log: (SpnvChannels)MakeCall, Start");

            if (App.HandsetMgr != null)
            {
                Handset.Handset handset = App.HandsetMgr.GetPreparedHandset();
                if (handset != null && !handset.IsBusy)
                {
                    Log.Debug(String.Format("Protocal Stack Log: (SpnvChannels)MakeCall, Switch To Handset, dest:{0}, End",dest));
                    return handset.MakeCall(dest);
                }
            }

            if (GetIdleChannel() == null)
            {
                Log.Debug("Protocal Stack Log: (SpnvChannels)MakeCall, NoIdleChannel, False End");
                return false;
            }

            if (nogroupcall && GetGroupCallChannel() != null)
            {
                Log.Debug("Protocal Stack Log: (SpnvChannels)MakeCall, nogroupcall && GetGroupCallChannel() != null, False End");
                return false;
            }

            HoldActiveCall();

            callId = App.SIPUA.MakeCall(accoutId, dest, 0);

            //Log.Debug(string.Format("Protocal Stack Log: App.SIPUA.MakeCall:{0},{1},{2},{3}", accoutId, dest, 0, callId));

            Log.Debug(string.Format("Protocal Stack Log: (SpnvChannels)MakeCall, MakeCall:{0},{1},{2}", accoutId, dest, 0));

            Log.Debug("Protocal Stack Log: (SpnvChannels)MakeCall, End");
            return true;
        }

        public void HoldActiveCall()
        {
            Log.Debug("Protocal Stack Log: (SpnvChannels)HoldActiveCall, Start");

            foreach (Channel channel in Channels)
            {
                if (channel.IsChannelBusy && !channel.IsStateHold &&
                    channel.IsStateConnected)
                {
                    App.SIPUA.Hold(channel.CallId);
                    Log.Debug(String.Format("Protocal Stack Log: (SpnvChannels)HoldActiveCall, Hold:{0}", channel.CallId));
                }
            }

            Log.Debug("Protocal Stack Log: (SpnvChannels)HoldActiveCall, End");
        }

        public bool AddConfMember(string user)
        {
            var channel = GetChannel(DialPrefixType.DialConference);
            if (channel == null)
            {
                return false;
            }

            try
            {
                SpnvRemoteProc proxy = new SpnvRemoteProc(_system.Setting.NidsAddr, _system.Setting.NidsInstructPort);
                proxy.AddConfMember(_system.Setting.AccountUser, user);
            }
            catch (Exception)
            {
                return false;            	
            }
            return true;
        }

        public bool DelConMember(string user)
        {
            try
            {
                SpnvRemoteProc proxy = new SpnvRemoteProc(_system.Setting.NidsAddr, _system.Setting.NidsInstructPort);
                proxy.DelConfMember(_system.Setting.AccountUser, user);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}