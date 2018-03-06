using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace renstech.NET.InterProcPipe
{
    public enum HANDSETMSG_TYPE
    {
        MSG_REQUEST,
        MSG_RESPONSE,
        MSG_NOTIFY,
    }

    public enum HANDSETMSG_REQ_CODE
    {
        MSG_REQ_NONE,
        MSG_REQ_RUN,
        MSG_REQ_ADDACCOUNT,
        MSG_REQ_MAKECALL,
        MSG_REQ_ANSWER,
        MSG_REQ_HANGUP,
        MSG_REQ_RECORDINGDIRCHANGED,
    }

    public enum HANDSETMSG_NOTIFY_CODE
    {
        MSG_NOTIFY_NONE,
        MSG_NOTIFY_ONLINE,
        MSG_NOTIFY_INFO,
        MSG_NOTIFY_CALLSTATE,
    }

    public enum HANDSETMSG_REQ_RESULT
    {
        MSG_RES_NONE,
        MSG_RES_OK,
        MSG_RES_FAIL,
    }

    [Serializable()]
    public class HandsetMsg
    {
        public HANDSETMSG_TYPE Type { get; set; }
    }

    [Serializable()]
    public class HandsetReqMsg : HandsetMsg
    {
        public HandsetReqMsg()
        {
            Type = HANDSETMSG_TYPE.MSG_REQUEST;
            MsgRequestCode = HANDSETMSG_REQ_CODE.MSG_REQ_NONE;
        }

        public HANDSETMSG_REQ_CODE MsgRequestCode { get; set; }
    }

    [Serializable()]
    public class HandsetResMsg : HandsetMsg
    {
        public HandsetResMsg()
        {
            Type = HANDSETMSG_TYPE.MSG_RESPONSE;
            MsgRequestCode = HANDSETMSG_REQ_CODE.MSG_REQ_NONE;
            MsgResponseResult = HANDSETMSG_REQ_RESULT.MSG_RES_NONE;
        }

        public HANDSETMSG_REQ_CODE MsgRequestCode { get; set; }
        public HANDSETMSG_REQ_RESULT MsgResponseResult { get; set; }
    }

    [Serializable()]
    public class HandsetNotifyMsg : HandsetMsg
    {
        public HandsetNotifyMsg()
        {
            Type = HANDSETMSG_TYPE.MSG_NOTIFY;
            MsgNotifyCode = HANDSETMSG_NOTIFY_CODE.MSG_NOTIFY_NONE;
        }

        public HANDSETMSG_NOTIFY_CODE MsgNotifyCode { get; set; }
    }

    [Serializable()]
    public class HandsetReqUAStartMsg : HandsetReqMsg
    {
        public HandsetReqUAStartMsg()
        {
            MsgRequestCode = HANDSETMSG_REQ_CODE.MSG_REQ_RUN;
        }

        public int CaptureDevice { get; set; }
        public int PlaybackDevice { get; set; }
    }

    [Serializable()]
    public class HandsetReqAddAccountMsg : HandsetReqMsg
    {
        public HandsetReqAddAccountMsg()
        {
            MsgRequestCode = HANDSETMSG_REQ_CODE.MSG_REQ_ADDACCOUNT;
        }

        public string User { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        public string Proxy { get; set; }
        public bool IsReg { get; set; }
        public bool IsAutoAnswer { get; set; }
    }

    [Serializable()]
    public class HandsetReqMakeCallMsg : HandsetReqMsg
    {
        public HandsetReqMakeCallMsg()
        {
            MsgRequestCode = HANDSETMSG_REQ_CODE.MSG_REQ_MAKECALL;
        }

        public string DestUri { get; set; }
    }

    [Serializable()]
    public class HandsetReqAnswerMsg : HandsetReqMsg
    {
        public HandsetReqAnswerMsg()
        {
            MsgRequestCode = HANDSETMSG_REQ_CODE.MSG_REQ_ANSWER;
        }
    }

    [Serializable()]
    public class HandsetReqHangupMsg : HandsetReqMsg
    {
        public HandsetReqHangupMsg()
        {
            MsgRequestCode = HANDSETMSG_REQ_CODE.MSG_REQ_HANGUP;
        }
    }

    [Serializable()]
    public class HandsetNotifyInfoMsg : HandsetNotifyMsg
    {
        public HandsetNotifyInfoMsg()
        {
            MsgNotifyCode = HANDSETMSG_NOTIFY_CODE.MSG_NOTIFY_INFO;
        }

        public int udp_port { get; set; }
        public string host_addr { get; set; }
    }

    [Serializable()]
    public class HandsetNotifyCallStateMsg : HandsetNotifyMsg
    {
        public HandsetNotifyCallStateMsg()
        {
            MsgNotifyCode = HANDSETMSG_NOTIFY_CODE.MSG_NOTIFY_CALLSTATE;
        }

        public string remoteUri { get; set; }
        public int state { get; set; }
        public int role { get; set; }
    }

    [Serializable()]
    public class HandsetReqRecordingDirChangedMsg : HandsetReqMsg
    {
        public HandsetReqRecordingDirChangedMsg()
        {
            MsgRequestCode = HANDSETMSG_REQ_CODE.MSG_REQ_RECORDINGDIRCHANGED;
        }

        public string newRecordingDir { get; set; }
    }
}