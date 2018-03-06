using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using renstech.NET.PJSIP;
using renstech.NET.SIPUA;
using renstech.NET.SupernovaDispatcher.Interface;
using renstech.NET.SupernovaDispatcher.Model;
using renstech.NET.nids;
using renstech.NET.nids.AsynEvent.pa;
using renstech.NET.nids.RemoteProc;

namespace renstech.NET.SupernovaDispatcher.PADispatch
{
    public class PASubSystem : Subsystem
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(PASubSystem));

        PADispatchUserControl _page;
        readonly PAChannel _paChannel;
        PAEventSubscriber _eventSubscriber;

        readonly List<PASection> _sections = new List<PASection>();
        List<ShortCutKey> _shortcuts = new List<ShortCutKey>();

        public PASubSystem(SubsysId id, string name) : base(id, name)
        {
            _paChannel = new PAChannel(this);
        }

        public PASetting Setting { get; private set; }
        internal PAChannel Channel { get { return _paChannel; } }
        internal List<PASection> Stations { get { return _sections; } }

        public override bool Initialize(ref string errMsg)
        {
            InitResult = InitResult.InitFail;

            Setting = App.ConfigManager.GetSetting(typeof(PASetting), true)
                as PASetting;
            if (Setting == null)
            {
                Log.Error("PASetting can not be instantiated");
                return false;
            }

            App.SIPUA.CallStateInfo += OnCallStateChanged;

            try
            {
                LoadServerInfo();
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.ToString());
                ClearServerInfo();

                errMsg = ex.Message;
                return false;
            }

            DialRule.DialPrefix = Setting.DialPrefix;

            InitializeEventWatcher();

            InitResult = InitResult.InitOk;
            return true;
        }

        public bool InitializeEventWatcher()
        {
            if (string.IsNullOrEmpty(Setting.PAServerAddr) || 
                string.IsNullOrEmpty(Setting.EventNode) ||
                Setting.NIDSEventPort == 0)
            {
                Log.Error(string.Format("pa event watcher failed to initialize due to incorrect parameters!"));
                return false;
            }

            _eventSubscriber = new PAEventSubscriber();
            _eventSubscriber.ServerAddr = Setting.PAServerAddr;
            _eventSubscriber.ServrtPort = Setting.NIDSEventPort;
            _eventSubscriber.EventTag = Setting.EventNode;
            _eventSubscriber.EventInfo += new System.EventHandler<PAEventArgs>(OnEventReceived);
            _eventSubscriber.StartNidsEvent();

            return true;
        }

        public int GetAccountId()
        {
            return App.SIPUA.GetDefaultAccountId();
        }

        private void ClearServerInfo()
        {
            _sections.Clear();
        }

        public PARemoteProc GetPARpc()
        {
            PARemoteProc proxy = new PARemoteProc("192.168.0.226", 7085);
            return proxy;
        }

        private bool LoadServerInfo()
        {
            ClearServerInfo();

            PARemoteProc proxy = GetPARpc();

            pa_section[] sections = proxy.GetSections();
            if (sections != null)
            {
                foreach (pa_section section in sections)
                {
                    PASection se = new PASection(section.pa_section_id, 
                        section.pa_section_name);
                    _sections.Add(se);

                    GetSectionZones(proxy, se);

                    GetSectionMusic(proxy, se);

                    GetSectionCDMusic(proxy, se);

                    GetSectionMusicChannel(proxy, se);
                }
            }

            DialRule.SectionLength = proxy.GetPASectionIdLength();
            DialRule.ZoneLength = proxy.GetPAZoneIdLength();
            DialRule.MusicFolderLength = proxy.GetMusicFolderIdLength();
            DialRule.MusicFileLength = proxy.GetMusicFileIdLength();
            
            return true;
        }

        private void GetSectionZones(PARemoteProc proxy, PASection section)
        {
            pa_zone[] zones = proxy.GetZones(section.Id);
            if (zones == null)
            {
                Log.Error(string.Format("no zone found in PA section {0}", section.Name));
                return;
            }

            foreach (pa_zone zone in zones)
            {
                PAZone zo = new PAZone(zone.pa_zone_id, 
                    zone.pa_zone_name);
                section.AddPAZone(zo);
            }
        }

        private void GetSectionMusic(PARemoteProc proxy, PASection section)
        {
            music_folder[] folders = proxy.GetMusicFolders(section.Id);
            if (folders == null)
            {
                return;
            }

            foreach (music_folder folder in folders)
            {
                BKMusicFolder fo = new BKMusicFolder(folder.music_folder_id,
                    folder.music_folder_name);

                section.AddBKMusic(fo);

                GetFolderMusic(proxy, section, fo);
            }
        }

        private void GetSectionCDMusic(PARemoteProc proxy, PASection section)
        {
            cd_music_file[] files = proxy.GetMusicCDFiles(section.Id);
            if (files == null)
            {
                return;
            }

            foreach (cd_music_file file in files)
            {
                BKMusicCDFile fi = new BKMusicCDFile(file.cd_music_id,
                    file.cd_music_name);
                section.MusicCDFiles.Add(fi);
            }
        }

        private void GetSectionMusicChannel(PARemoteProc proxy, PASection section)
        {
            music_channel[] channels = proxy.getMusicChannels(section.Id);
            if (channels == null)
            {
                return;
            }

            foreach (music_channel channel in channels)
            {
                BKMusicRadioChannel ch = new BKMusicRadioChannel(channel.radio_channel_id,
                    channel.radio_channel_name);
                section.MusicRadioChannels.Add(ch);
            }
        }

        private void GetFolderMusic(PARemoteProc proxy, PASection section, BKMusicFolder folder)
        {
            music_file[] files = proxy.GetMusicFiles(section.Id, folder.Id);
            if (files == null)
            {
                return;
            }

            foreach (music_file file in files)
            {
                BKMusicFile fi = new BKMusicFile(file.music_file_id,
                    file.music_file_name);
                folder.AddMusicFile(fi);
            }
        }

        public override void Uninitialize()
        {
        }

        public override Account GetSIPAccount()
        {
            return null;
        }

        public override IDispatchPage GetDispatchPage()
        {
            return _page ?? (_page = new PADispatchUserControl(this));
        }

        public PASection GetPASectionById(int id)
        {
            return _sections.FirstOrDefault(section => section.Id == id);
        }

        public void OnCallStateChanged(object sender, CallStateArgs e)
        {
            if (AccountId != e.AccountId)
            {
                return;
            }

            int userData = App.SIPUA.GetCallUserData(e.CallId);
            if (userData != 0 && userData != (int)Id)
            {
                return;
            }

            if (e.STATE == sua_inv_state.PJSIP_INV_STATE_CONFIRMED)
            {
                Channel.IsBusy = true;
                Channel.CallId = e.CallId;

                SIPUri uri = new SIPUri(e.RemoteUri);
                Channel.SectionName = uri.User;

            }
            else if (e.STATE == sua_inv_state.PJSIP_INV_STATE_DISCONNECTED)
            {
                Channel.CallId = -1;
                Channel.IsBusy = false;
                Channel.SectionName = string.Empty;
                Channel.ZoneName = string.Empty;
            }
        }

        private void OnEventReceived(object sender, PAEventArgs args)
        {

        }
    }
}