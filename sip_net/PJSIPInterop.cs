using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace renstech.NET.SIPUA
{
    public static class PJSIPInterop
    {
        private const string dllPath = @"sipdll.dll";

        public const int OK = 0;

        [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention.StdCall)]
        public delegate void cb_log(int level, [System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)] string data, int len);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sua_reg_log_cb(cb_log callback);

        [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention.StdCall)]
        public delegate void cb_reg_state(int acc_id);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sua_reg_reg_state_cb(cb_reg_state callback);

        [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention.StdCall)]
        public delegate void cb_incoming_call(int acc_id, int call_id, [System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)] string remote_uri);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sua_reg_incoming_call_cb(cb_incoming_call callback);

        [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention.StdCall)]
        public delegate void cb_call_audio_media_state(int accId, int call_id, int state);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sua_reg_call_audio_media_state_cb(cb_call_audio_media_state callback);

        [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention.StdCall)]
        public delegate void cb_call_video_media_state(int acc_id, int call_id, int status, int wid);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sua_reg_call_video_media_state_cb(cb_call_video_media_state callback);

        [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention.StdCall)]
        public delegate void cb_call_state(int acc_id, int call_id, int state);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sua_reg_call_state(cb_call_state callback);

        [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention.StdCall)]
        public delegate void cb_pager([System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)] string from, [System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)] string text);

        [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention.StdCall)]
        public delegate void cb_call_media_event(int callId, uint medIdx, int eventType);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sua_reg_call_media_event(cb_call_media_event callback);

        [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention.StdCall)]
        public delegate void cb_player_file_end(int player_id);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sua_reg_player_file_end_cb(cb_player_file_end callback);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sua_reg_pager(cb_pager callback);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sua_config_set_max_call(int param0);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sua_config_set_nameserver(String server);

        [DllImport(dllPath)]
        public static extern void sua_config_set_outbound_proxy(String proxy);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sua_config_set_agentname(String agent);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sua_config_set_log_level(int level);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sua_config_set_log_msg(int enable);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sua_config_set_log_file(String path);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sua_config_set_enableqos(int qos_enable);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sua_config_set_udp_port(uint port);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sua_config_set_notcp();

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sua_config_set_noudp();

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sua_config_set_notone(int enable);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sua_config_set_ringfile(String file);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sua_config_set_autoanswer(int auto_answer);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_create();

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_init();

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_start();

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_destroy();

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_get_state();

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_acc_add(String id, String reg_uri, String user, String passwd, String domain, String proxy, int enable_reg, uint reg_timout, int auto_show_in_vid, ref int acc_id);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_acc_set_registration(int acc_id, int renew);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_acc_get_info(int acc_id, ref sua_acc_info accinfo);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_acc_get_default();

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_call_get_info(int call_id, ref sua_call_info callinfo);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_call_make_call(int acc_id, String dst_uri, int user_data, ref int call_id);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_call_make_call_novid(int acc_id, String dst_uri, ref int call_id);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_call_get_user_data(int call_id);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_call_get_media_status(int call_id, ref int media_status);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_call_get_conf_port(int call_id);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_call_180_ringing(int call_id);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_call_answer(int call_id);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_call_answer_novid(int call_id);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_call_set_hold(int call_id);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_call_reinvite(int call_id, uint options);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_call_redirect(int call_id, String dst_uri);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_call_xfer(int call_id, String dest);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_call_xfer_replaces(int call_id, int dest_call_id, int options);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_call_dial_dtmf(int call_id, [System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)] string digits);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_call_send_im(int call_id, [System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)] string mime_type, byte[] content);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_im_send(int acc_id, [System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)] string to, [System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)] string mime_type, byte[] content);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_call_hangup(int call_id, int code, String reason);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sua_call_hangup_all();

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_get_aud_devs_count();

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_get_aud_devs(uint index, ref media_aud_dev_info info);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_get_snd_dev(ref int capture_dev, ref int playback_dev);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_set_snd_dev(int capture_dev, int playback_dev);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_set_null_snd_dev();

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_set_ec(uint tail_ms, uint options);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_get_ec_tail(ref uint p_tail_ms);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_snd_is_active();

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_get_codecs_count();

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_get_codecs(uint index, ref sua_codec_info id);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_get_vid_codecs_count();

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_get_vid_codecs(uint index, ref sua_codec_info id);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_codec_set_priority([System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)] string codec_id, uint priority);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_vid_codec_set_priority([System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)] string codec_id, uint priority);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_recorder_create(String filename, ref int p_id);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_recorder_get_conf_port(int id);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_recorder_destroy(int id);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_player_create(String filename, uint loop, ref int player_id);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_player_get_conf_port(int id);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_player_destroy(int id);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_conf_connect(int source, int sink);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_conf_adjust_tx_level(int slot, float level);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_conf_adjust_rx_level(int slot, float level);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_conf_get_signal_level(int slot, ref uint tx_level, ref uint rx_level);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_vid_win_get_info(int wid, ref sua_vid_win_info info);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_vid_win_set_show(int wid, int show);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_vid_win_set_pos(int wid, int x, int y);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_vid_win_set_size(int wid, uint width, uint height);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_call_get_vid_stream_idx(int call_id);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_call_get_stream_info(int call_id, uint med_idx, ref sua_stream_info psi);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sua_get_udp_port();

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern String sua_get_host_addr();

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sua_config_set_novad(int enable);
    }
}