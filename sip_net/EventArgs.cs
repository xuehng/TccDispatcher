using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace renstech.NET.SIPUA
{
    public class LogEventArgs : EventArgs
    {
        public LogEventArgs(int level, string data)
        {
            Level = level;
            Data = data;
        }

        public int Level { get; private set; }
        public string Data { get; private set; }
    }

    public class IncomingCallArgs : EventArgs
    {
        public IncomingCallArgs(int accId, int callId, string remoteUri)
        {
            AccountId = accId;
            CallId = callId;
            RemoteUri = remoteUri;
        }

        public int AccountId { get; private set; }
        public int CallId { get; private set; }
        public string RemoteUri { get; private set; }
    }

    public class RegStateArgs : EventArgs
    {
        public RegStateArgs(int accId, int status, string status_text)
        {
            AccountId = accId;
            StatusCode = status;
            Status = status_text;
        }

        public int AccountId { get; private set; }
        public int StatusCode { get; private set; }
        public string Status { get; private set; } 
    }

    public class CallStateArgs : EventArgs
    {
        public CallStateArgs(int accId, int callId, sua_inv_state state, string remote, sua_role_e role, int lastStatus)
        {
            AccountId = accId;
            CallId = callId;
            STATE = state;
            RemoteUri = remote;
            ROLE = role;
            LastStatus = lastStatus;
        }

        public int AccountId { get; private set; }
        public int CallId { get; private set; }
        public sua_inv_state STATE { get; private set; }
        public string RemoteUri { get; private set; }
        public sua_role_e ROLE { get; private set; }
        public int LastStatus { get; private set; }
    }

    public class MediaStateArgs : EventArgs
    {
        public MediaStateArgs(int accId, int callId, sua_call_media_status state)
        {
            AccountId = accId;
            CallId = callId;
            MEDIASTATE = state;
        }

        public int AccountId { get; private set; }
        public int CallId { get; private set; }
        public sua_call_media_status MEDIASTATE { get; private set; }
    }

    public class VideoMediaStateArgs : EventArgs
    {
        public VideoMediaStateArgs(int accId, int callId, sua_call_media_status state, int wid)
        {
            AccountId = accId;
            CallId = callId;
            MEDIASTATE = state;
            WinId = wid;
        }

        public int AccountId { get; private set; }
        public int CallId { get; private set; }
        public sua_call_media_status MEDIASTATE { get; private set; }
        public int WinId { get; private set; }
    }

    public class PagerArgs : EventArgs
    {
        public PagerArgs(string from, string msg)
        {
            From = from;
            Msg = msg; 
        }

        public string From { get; private set; }
        public string Msg { get; private set; }
    }

    public class CallMediaEventArgs : EventArgs
    {
        public CallMediaEventArgs(int callId, uint mediaIndex, pjmedia_event_type type)
        {
            CallId = callId;
            MediaIndex = mediaIndex;
            Type = type;
        }

        public int CallId { get; private set; }
        public uint MediaIndex { get; private set; }
        public pjmedia_event_type Type { get; private set; }
    }

    public class PlayerFileEndArgs : EventArgs
    {
        public PlayerFileEndArgs(int playerId)
        {
            PlayerId = playerId;
        }

        public int PlayerId { get; private set; }
    }
}