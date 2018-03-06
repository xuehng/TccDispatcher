using System;
using renstech.NET.SIPUA;

namespace renstech.NET.SupernovaDispatcher.Model.Handset
{
    public class HandsetCallStateArgs : EventArgs
    {
        public HandsetCallStateArgs(string uri, sua_role_e role, sua_inv_state state)
        {
            RemoteUri = uri;
            Role = role;
            State = state;
        }

        public string RemoteUri { get; private set; }
        public sua_role_e Role { get; set; }
        public sua_inv_state State { get; set; }
    }
}
