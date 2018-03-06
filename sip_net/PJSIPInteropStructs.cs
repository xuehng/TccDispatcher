namespace renstech.NET.SIPUA
{
    public enum sua_call_flag
    {
        SUA_CALL_UNHOLD = 1,

        SUA_CALL_UPDATE_CONTACT = 2,

        SUA_CALL_INCLUDE_DISABLED_MEDIA = 4
    }

    public enum sua_role_e
    {
        SUA_ROLE_UAC,	/**< Role is UAC. */

        SUA_ROLE_UAS,	/**< Role is UAS. */
    };

    public enum sua_inv_state
    {
        PJSIP_INV_STATE_NULL,	    /**< Before INVITE is sent or received  */

        PJSIP_INV_STATE_CALLING,	    /**< After INVITE is sent		    */

        PJSIP_INV_STATE_INCOMING,	    /**< After INVITE is received.	    */

        PJSIP_INV_STATE_EARLY,	    /**< After response with To tag.	    */

        PJSIP_INV_STATE_CONNECTING,	    /**< After 2xx is sent/received.	    */

        PJSIP_INV_STATE_CONFIRMED,	    /**< After ACK is sent/received.	    */

        PJSIP_INV_STATE_DISCONNECTED,   /**< Session is terminated.		    */
    };


    /**
    * Top most media type. See also #pjmedia_type_name().
    */
    public enum pjmedia_type
    {
        /** Type is not specified. */
        PJMEDIA_TYPE_NONE,

        /** The media is audio */
        PJMEDIA_TYPE_AUDIO,

        /** The media is video. */
        PJMEDIA_TYPE_VIDEO,

        /** The media is application. */
        PJMEDIA_TYPE_APPLICATION,

        /** The media type is unknown or unsupported. */
        PJMEDIA_TYPE_UNKNOWN

    };

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct sua_call_media_info
    {
        /// int
        public int media_status;

        /// int
        public int media_type;

        /// int
        public int dir;

        /// int
        public int conf_slot;

        /// int
        public int win_in;

        /// int
        public int cap_dev;
    }


    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
    public struct sua_call_info
    {
        /// int
        public int id;

        /// int
        public int call_role;

        /// int
        public int acc_id;

        /// char[255]
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 255)]
        public string local_uri;

        /// char[255]
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 255)]
        public string local_contact;

        /// char[255]
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 255)]
        public string remote_uri;

        /// char[255]
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 255)]
        public string remote_contact;

        /// char[255]
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 255)]
        public string call_id;

        /// int
        public int state;

        public int last_status;

        /// int
        public int media_count;

        /// sua_call_media_info[16]
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = System.Runtime.InteropServices.UnmanagedType.Struct)]
        public sua_call_media_info[] media;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
    public struct sua_acc_info
    {
        /// int
        public int is_default;

        /// int
        public int has_registration;

        /// int
        public int expires;

        /// int
        public int status;

        /// char[255]
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 255)]
        public string status_text;

        /// int
        public int reg_last_err;
    }

    public enum sua_call_media_status
    {
        SUA_CALL_MEDIA_NONE,

        SUA_CALL_MEDIA_ACTIVE,

        SUA_CALL_MEDIA_LOCAL_HOLD,

        SUA_CALL_MEDIA_REMOTE_HOLD,

        SUA_CALL_MEDIA_ERROR,
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
    public struct media_aud_dev_info
    {
        /// char[64]
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 64)]
        public string name;

        /// unsigned int
        public uint input_count;

        /// unsigned int
        public uint output_count;

        /// unsigned int
        public uint default_samples_per_sec;

        /// char[32]
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 32)]
        public string driver;

        /// unsigned int
        public uint caps;

        /// unsigned int
        public uint routes;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
    public struct sua_codec_info
    {
        /// char[255]
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 255)]
        public string codec_id;

        /// unsigned int
        public uint priority;

        /// char[255]
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 255)]
        public string desc;

        /// char[64]
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 64)]
        public string buf_;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct sua_vid_win_info
    {

        /// int
        public int is_native;

        /// unsigned int
        public uint hwnd;

        /// int
        public int show;

        /// int
        public int x;

        /// int
        public int y;

        /// int
        public int width;

        /// int
        public int height;
    }


    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct vid_media_format
    {

        /// unsigned int
        public uint vid_width;

        /// unsigned int
        public uint vid_height;

        /// unsigned int
        public uint avg_bps;

        /// unsigned int
        public uint max_bps;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct media_vid_stream_info
    {

        /// vid_media_format
        public vid_media_format dec_fmt;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct sua_stream_info
    {

        /// int
        public int type;

        /// media_vid_stream_info
        public media_vid_stream_info vid;
    }

    public enum pjmedia_event_type
    {
        PJMEDIA_EVENT_NONE,
        PJMEDIA_EVENT_FMT_CHANGED	= 1212370246,
        PJMEDIA_EVENT_WND_RESIZED	= 1515343447,
    };
}