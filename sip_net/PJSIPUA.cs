using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using renstech.NET.SIPUA;

namespace renstech.NET.PJSIP
{
    public class SIPUA
    {
        private readonly List<Account> _accounts = new List<Account>();

        private readonly PJSIPInterop.cb_log _cbLogInfo;
        private readonly PJSIPInterop.cb_reg_state _cbRegState;
        private readonly PJSIPInterop.cb_incoming_call _cbIncomingCall;
        private readonly PJSIPInterop.cb_call_state _cbCallState;
        private readonly PJSIPInterop.cb_call_audio_media_state _cbMediaState;
        private readonly PJSIPInterop.cb_call_video_media_state _cbVideoMediaState;
        private readonly PJSIPInterop.cb_pager _cbPager;
        private readonly PJSIPInterop.cb_call_media_event _cbCallMediaEvent;
        private readonly PJSIPInterop.cb_player_file_end _cbPlayedEnd;

        public string CaptureDevice { get; set; }
        public string PlaybakDevice { get; set; }
        public int CaptureDeviceId { get; set; }
        public int PlaybakDeviceId { get; set; }

        public event EventHandler<LogEventArgs> LogInfo;
        public event EventHandler<RegStateArgs> RegStateInfo;
        public event EventHandler<IncomingCallArgs> IncomingCallInfo;
        public event EventHandler<CallStateArgs> CallStateInfo;
        public event EventHandler<MediaStateArgs> MediaStateInfo;
        public event EventHandler<VideoMediaStateArgs> VideoMediaStateInfo;
        public event EventHandler<PagerArgs> PagerInfo;
        public event EventHandler<CallMediaEventArgs> CallMediaEventInfo;
        public event EventHandler<PlayerFileEndArgs> PlayerFileEnd; 

        public SIPUA()
        {
            CaptureDeviceId = -1;
            PlaybakDeviceId = -1;

            _cbLogInfo = OnLogUpdate;
            _cbRegState = OnRegStateChanged;
            _cbIncomingCall = OnIncomingCall;
            _cbCallState = OnCallState;
            _cbMediaState = OnMediaState;
            _cbVideoMediaState = OnMediaVideoState;
            _cbPager = OnPager;
            _cbCallMediaEvent = OnCallMediaEvent;
            _cbPlayedEnd = OnPlayerFileEndEvent;
        }

        private void OnLogUpdate(int level, string data, int len)
        {
            if (LogInfo != null) 
                LogInfo(this, new LogEventArgs(level, data));
        }

        private void OnRegStateChanged(int accId)
        {
            sua_acc_info info = new sua_acc_info();
            PJSIPInterop.sua_acc_get_info(accId, ref info);

            Account account = GetAccount(accId);
            if (account != null)
                account.IsRegistered = (info.status == 200);

            if (RegStateInfo != null) 
                RegStateInfo(this, new RegStateArgs(accId, info.status, info.status_text));
        }

        private void OnIncomingCall(int accId, int callId, string remoteUri)
        {
            if (IncomingCallInfo != null) 
                IncomingCallInfo(this, new IncomingCallArgs(accId, callId, remoteUri));
        }

        private void OnCallState(int accId, int callId, int state)
        {
            if (CallStateInfo != null)
            {
                sua_call_info info = new sua_call_info();
                PJSIPInterop.sua_call_get_info(callId, ref info);

                CallStateInfo(this, new CallStateArgs(accId, callId, (sua_inv_state)state, 
                    info.remote_uri, (sua_role_e)info.call_role, info.last_status));
            }
        }

        private void OnMediaState(int accId, int callId, int state)
        {
            if (MediaStateInfo != null) 
                MediaStateInfo(this, new MediaStateArgs(accId, callId, (sua_call_media_status)state));
        }

        private void OnMediaVideoState(int accId, int callId, int state, int wid)
        {
            if (VideoMediaStateInfo != null)
                VideoMediaStateInfo(this, new VideoMediaStateArgs(accId, callId, (sua_call_media_status)state, wid));
        }

        private void OnPager(string from, string msg)
        {
            if (PagerInfo != null)
                PagerInfo(this, new PagerArgs(from, msg));
        }

        private void OnCallMediaEvent(int callId, uint mediaIndex, int type)
        {
            pjmedia_event_type eventType = pjmedia_event_type.PJMEDIA_EVENT_NONE;
            switch (type)
            {
                case (int)pjmedia_event_type.PJMEDIA_EVENT_FMT_CHANGED:
                    eventType = pjmedia_event_type.PJMEDIA_EVENT_FMT_CHANGED;
                    break;
                case (int)pjmedia_event_type.PJMEDIA_EVENT_WND_RESIZED:
                    eventType = pjmedia_event_type.PJMEDIA_EVENT_WND_RESIZED;
                    break;
            }

            if (CallMediaEventInfo != null)
                CallMediaEventInfo(this, new CallMediaEventArgs(callId, mediaIndex, eventType));
        }

        private void OnPlayerFileEndEvent(int playerId)
        {
            if (PlayerFileEnd != null)
                PlayerFileEnd(this, new PlayerFileEndArgs(playerId));
        }

        public bool Start()
        {
            int result = PJSIPInterop.sua_create();
            if (result != PJSIPInterop.OK)
            {
                return false;
            }

            PJSIPInterop.sua_reg_log_cb(_cbLogInfo);
            PJSIPInterop.sua_reg_reg_state_cb(_cbRegState);
            PJSIPInterop.sua_reg_incoming_call_cb(_cbIncomingCall);
            PJSIPInterop.sua_reg_call_state(_cbCallState);
            PJSIPInterop.sua_reg_call_audio_media_state_cb(_cbMediaState);
            PJSIPInterop.sua_reg_call_video_media_state_cb(_cbVideoMediaState);
            PJSIPInterop.sua_reg_pager(_cbPager);
            PJSIPInterop.sua_reg_call_media_event(_cbCallMediaEvent);
            PJSIPInterop.sua_reg_player_file_end_cb(_cbPlayedEnd);

            PJSIPInterop.sua_config_set_notone(1);
            PJSIPInterop.sua_config_set_novad(1);

            result = PJSIPInterop.sua_init();
            if (result != PJSIPInterop.OK)
            {
                return false;
            }

            if (CaptureDeviceId  == -1)
            {
                CaptureDeviceId = GetAudioDeviceId(CaptureDevice);
            }

            if (PlaybakDeviceId == -1)
            {
                PlaybakDeviceId = GetAudioDeviceId(PlaybakDevice); 
            }

            if (CaptureDeviceId != -1 && PlaybakDeviceId != -1)
            {
                PJSIPInterop.sua_set_snd_dev(CaptureDeviceId, PlaybakDeviceId);
            }

            result = PJSIPInterop.sua_start();
            if (result != PJSIPInterop.OK)
            {
                return false;
            }

            return true;
        }

        public bool AddAccount(Account account)
        {
            SIPUri uri = new SIPUri();
            uri.User = account.User;
            uri.Host = account.Domain;
            string userUri = uri.Uri;

            uri = new SIPUri {Host = account.Domain};
            string regUri = uri.Uri;

            string proxyUri = null;
            if (!account.IsDomainRegistration && !string.IsNullOrEmpty(account.Proxy))
            {
                uri.Host = account.Proxy;
                proxyUri = uri.Uri;
                proxyUri += ";hide";
            }

            int accId = -1;
            int status = PJSIPInterop.sua_acc_add(userUri, regUri, account.User, account.Password,
                account.Domain, proxyUri, account.IsRegistrationEnabled ? 1 : 0, 
                account.RegistrationExpire, account.IsAutoShowInboundVideo ? 1 : 0, ref accId);
            if (status != PJSIPInterop.OK)
            {
                return false;
            }

            account.Id = accId;
            _accounts.Add(account);
            return true;
        }

        public int GetDefaultAccountId()
        {
            return PJSIPInterop.sua_acc_get_default();
        }

        public Account GetAccount(int accId)
        {
            return _accounts.FirstOrDefault(account => account.Id == accId);
        }

        public void SetAccountRegistration(int accId, bool register)
        {
            PJSIPInterop.sua_acc_set_registration(accId, register ? 1 :0);
        }

        public bool Stop()
        {
            int result = PJSIPInterop.sua_destroy();
            return ( result == PJSIPInterop.OK);
        }

        public bool GetAudioDevice(ref int capture, ref int playback)
        {
            int result = PJSIPInterop.sua_get_snd_dev(ref capture, ref playback);
            return (result == PJSIPInterop.OK);
        }

        public int GetAudioDeviceId(string name)
        {
            int count = PJSIPInterop.sua_get_aud_devs_count();
            for (uint i = 0; i < count; i++)
            {
                media_aud_dev_info dev = new media_aud_dev_info();
                PJSIPInterop.sua_get_aud_devs(i, ref dev);
                
                if (dev.name == name) 
                    return (int)i;
            }
            return -1;
        }

        public List<Device> GetAudioDevices()
        {
            List<Device> devices = new List<Device>();

            int count = PJSIPInterop.sua_get_aud_devs_count();
            for (uint i = 0; i < count; i++)
            {
                media_aud_dev_info dev = new media_aud_dev_info();
                PJSIPInterop.sua_get_aud_devs(i, ref dev);

                Device device = new Device();
                device.Id = i;
                device.Name = dev.name;
                device.InputCount = dev.input_count;
                device.OutputCount = dev.output_count;
                devices.Add(device);
            }
            return devices;
        }

        public List<Codec> GetAudioCodecs()
        {
            List<Codec> codecs = new List<Codec>();

            int count = PJSIPInterop.sua_get_codecs_count();
            for (uint i = 0; i < count; i++)
            {
                sua_codec_info info = new sua_codec_info();
                PJSIPInterop.sua_get_codecs(i, ref info);

                Codec codec = new Codec();
                codec.Name = info.codec_id;
                codec.Priority = info.priority;
                codecs.Add(codec);
            }
            return codecs;
        }

        public List<Codec> GetVideoCodecs()
        {
            List<Codec> codecs = new List<Codec>();

            int count = PJSIPInterop.sua_get_vid_codecs_count();
            for (uint i = 0; i < count; i++)
            {
                sua_codec_info info = new sua_codec_info();
                PJSIPInterop.sua_get_vid_codecs(i, ref info);

                Codec codec = new Codec();
                codec.Name = info.codec_id;
                codec.Priority = info.priority;
                codecs.Add(codec);
            }
            return codecs;
        }

        public string GetHostAddr()
        {
            return PJSIPInterop.sua_get_host_addr();
        }

        public int GetUdpPort()
        {
            return PJSIPInterop.sua_get_udp_port();
        }

        public bool GetSignalLevel(ref uint tx_level, ref uint rx_level)
        {
            if (PJSIPInterop.sua_conf_get_signal_level(0, ref tx_level, ref rx_level)
                 != PJSIPInterop.OK)
            {
                return false;
            }
            return true;
        }

        public bool MuteMicrophone(bool mute)
        {
            if (mute)
            {
                if (PJSIPInterop.sua_conf_adjust_rx_level(0, 0) != PJSIPInterop.OK)
                    return false;
            }
            else
            {
                if (PJSIPInterop.sua_conf_adjust_rx_level(0, 1) != PJSIPInterop.OK)
                    return false;
            }
            return true;
        }

        public bool GetCallInfo(int callId, ref sua_call_info info)
        {
            int status = PJSIPInterop.sua_call_get_info(callId, ref info);
            return (status == PJSIPInterop.OK);
        }

        private string NormalizeRequestUri(int accId, string dest)
        {
            if (dest.IndexOf('@') != -1)
            {
                return dest;
            }

            SIPUri uri = new SIPUri();
            uri.User = dest;

            Account account = GetAccount(accId);
            if (account == null)
            {
                return dest;
            }

            uri.Host = account.Domain;
            return uri.Uri;
        }

        public int MakeCall(int accId, string dest, int user_data)
        {
            string requri = NormalizeRequestUri(accId, dest);
            
            int callId = -1;
            PJSIPInterop.sua_call_make_call(accId, requri, user_data, ref callId);
            return callId;
        }

        public int MakeCall_NoVideo(int accId, string dest)
        {
            string requri = NormalizeRequestUri(accId, dest);

            int callId = -1;
            PJSIPInterop.sua_call_make_call_novid(accId, requri, ref callId);
            return callId;
        }

        public int GetCallUserData(int callId)
        {
            return PJSIPInterop.sua_call_get_user_data(callId);
        }

        public bool Ringing(int callId)
        {
            int result = PJSIPInterop.sua_call_180_ringing(callId);
            return (result == PJSIPInterop.OK);
        }

        public bool Answer(int callId)
        {
            int result = PJSIPInterop.sua_call_answer(callId);
            return ( result == PJSIPInterop.OK);
        }

        public bool Answer_NoVideo(int callId)
        {
            int result = PJSIPInterop.sua_call_answer_novid(callId);
            return (result == PJSIPInterop.OK);
        }

        public bool Xfer(int accId, int callId, string dest)
        {
            string requri = NormalizeRequestUri(accId, dest);
            int result = PJSIPInterop.sua_call_xfer(callId, requri);
            return (result == PJSIPInterop.OK);
        }

        public bool Redirect(int accId, int callId, string dest)
        {
            string requri = NormalizeRequestUri(accId, dest);
            return (PJSIPInterop.sua_call_redirect(callId, requri) == PJSIPInterop.OK);
        }

        public bool SendMessage(int callId, string content)
        {
            int result = PJSIPInterop.sua_call_send_im(callId, "text/plain", Encoding.UTF8.GetBytes(content));
            return ( result == PJSIPInterop.OK);
        }

        public bool SendMessage(int accId, string dest, string content)
        {
            string destUri = NormalizeRequestUri(accId, dest);
            return (PJSIPInterop.sua_im_send(accId, destUri, "text/plain", Encoding.UTF8.GetBytes(content)) == PJSIPInterop.OK);
        }

        public bool DialDTMF(int callId, string digit)
        {
            int result = PJSIPInterop.sua_call_dial_dtmf(callId, digit);
            return (result == PJSIPInterop.OK);
        }

        public bool Hold(int callId)
        {
            return (PJSIPInterop.sua_call_set_hold(callId) == PJSIPInterop.OK);
        }

        public bool Unhold(int callId)
        {
            int result = PJSIPInterop.sua_call_reinvite(callId, (uint)sua_call_flag.SUA_CALL_UNHOLD);
            return ( result == PJSIPInterop.OK);
        }

        public bool Hangup(int callId)
        {
            return (PJSIPInterop.sua_call_hangup(callId, 0, "") == PJSIPInterop.OK);
        }

        public void HangupAll()
        {
            PJSIPInterop.sua_call_hangup_all();
        }

        public bool StartRecording(int callId, string filename, ref int recordId)
        {
            int status = PJSIPInterop.sua_recorder_create(filename, ref recordId);
            if (status != PJSIPInterop.OK)
            {
                return false;
            }

            int callslot = PJSIPInterop.sua_call_get_conf_port(callId);
            int recslot = PJSIPInterop.sua_recorder_get_conf_port(recordId);

            PJSIPInterop.sua_conf_connect(callslot, recslot);
            PJSIPInterop.sua_conf_connect(0, recslot);

            return true;
        }

        public bool StopRecord(int recordId)
        {
            return (PJSIPInterop.sua_recorder_destroy(recordId) == PJSIPInterop.OK);
        }

        public int PlayFile(int callId, string filename, bool loop)
        {
            uint option = loop ? (uint)0 : (uint)1;

            int playerId = -1;
            int status = PJSIPInterop.sua_player_create(filename, option, ref playerId);
            if (status != PJSIPInterop.OK)
            {
                return -1;
            }

            int callslot = PJSIPInterop.sua_call_get_conf_port(callId);
            int playerslot = PJSIPInterop.sua_player_get_conf_port(playerId);

            PJSIPInterop.sua_conf_connect(playerslot, callslot);
            return playerId;            
        }

        public int StartRing(int callId, string filename)
        {
            int playerId = -1;
            int status = PJSIPInterop.sua_player_create(filename, 0, ref playerId);
            if (status != PJSIPInterop.OK)
            {
                return -1;
            }

            int callslot = PJSIPInterop.sua_call_get_conf_port(callId);
            int playerslot = PJSIPInterop.sua_player_get_conf_port(playerId);

            PJSIPInterop.sua_conf_connect(playerslot, 0);
            return playerId;
        }

        public bool StopRing(int playId)
        {
            int status = PJSIPInterop.sua_player_destroy(playId);
            return (status == PJSIPInterop.OK);
        }

        private int GetCallVideoWinId(int callId)
        {
            sua_call_info info = new sua_call_info();
            PJSIPInterop.sua_call_get_info(callId, ref info);

            for (int i = 0; i < info.media_count; i++)
            {
                if (((pjmedia_type)info.media[i].media_type) == pjmedia_type.PJMEDIA_TYPE_VIDEO)
                {
                    return info.media[i].win_in;
                }
            }
            return -1;
        }

        public bool GetVidWindowInfo(int callId, ref sua_vid_win_info info)
        {
            int wid = GetCallVideoWinId(callId);
            if (wid == -1)
            {
                return false;
            }

            int status = PJSIPInterop.sua_vid_win_get_info(wid, ref info);
            if (status != PJSIPInterop.OK)
            {
                return false;
            }
            return true; 
        }

        public bool ShowVideoWindow(int callId, bool show)
        {
            int wid = GetCallVideoWinId(callId);
            if (wid == -1)
            {
                return false;
            }

            sua_vid_win_info info = new sua_vid_win_info();
            PJSIPInterop.sua_vid_win_get_info(wid, ref info);

            if (info.show == 1 && show == false)
            {
                PJSIPInterop.sua_vid_win_set_show(wid, 0);
            }
            else if ( info.show == 0 && show == true )
            {
                PJSIPInterop.sua_vid_win_set_show(wid, 1);
            }
            return true;
        }

        public bool SetVideoWindowSize(int callId, uint width, uint height)
        {
            int wid = GetCallVideoWinId(callId);
            if (wid == -1)
            {
                return false;
            }

            int status = PJSIPInterop.sua_vid_win_set_size(wid, width, height);
            return (status == PJSIPInterop.OK);
        }

        public bool GetVideoWindowDefaultSize(int callId, ref uint width, ref uint height)
        {
            int mediaIndex = PJSIPInterop.sua_call_get_vid_stream_idx(callId);
            if (mediaIndex == -1)
            {
                return false;
            }

            sua_stream_info info = new sua_stream_info();
            int status = PJSIPInterop.sua_call_get_stream_info(callId, (uint)mediaIndex, ref info);
            if (status != PJSIPInterop.OK)
            {
                return false;
            }

            if (info.type != (int)pjmedia_type.PJMEDIA_TYPE_VIDEO)
            {
                return false;
            }

            width = info.vid.dec_fmt.vid_width;
            height = info.vid.dec_fmt.vid_height;
            return true;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SetWindowPos(IntPtr hwnd, IntPtr
        hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

        public enum SpecialWindowHandles
        {
            // ReSharper disable InconsistentNaming
            /// <summary>
            ///     Places the window at the bottom of the Z order. If the hWnd parameter identifies a topmost window, the window loses its topmost status and is placed at the bottom of all other windows.
            /// </summary>
            HWND_TOP = 0,
            /// <summary>
            ///     Places the window above all non-topmost windows (that is, behind all topmost windows). This flag has no effect if the window is already a non-topmost window.
            /// </summary>
            HWND_BOTTOM = 1,
            /// <summary>
            ///     Places the window at the top of the Z order.
            /// </summary>
            HWND_TOPMOST = -1,
            /// <summary>
            ///     Places the window above all non-topmost windows. The window maintains its topmost position even when it is deactivated.
            /// </summary>
            HWND_NOTOPMOST = -2
            // ReSharper restore InconsistentNaming
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int BringWindowToTop(IntPtr hwnd);

        public bool SetVideoWindowPos(int callId, int x, int y, int width, int height)
        {
            int wid = GetCallVideoWinId(callId);
            if (wid == -1)
            {
                return false;
            }

            sua_vid_win_info info = new sua_vid_win_info();
            int status = PJSIPInterop.sua_vid_win_get_info(wid, ref info);
            if (status != PJSIPInterop.OK)
            {
                return false;
            }

            //PJSIPInterop.sua_vid_win_set_pos(wid, x, y);

            SetWindowPos((IntPtr)info.hwnd, (IntPtr)SpecialWindowHandles.HWND_TOPMOST, x, y, width, height, (int)(0x40));
            return true;
        }
    }
}