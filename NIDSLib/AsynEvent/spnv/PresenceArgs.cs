using System;

namespace renstech.NET.nids.AsynEvent.spnv
{
    public class PresenceArgs : EventArgs
    {
        public PresenceArgs(PresenceAction action, string username)
        {
            this.PrecenseAction = action;
            this.UserName = username;
        }

        public PresenceAction PrecenseAction { get; private set; }
        public string UserName { get; private set; }
    }
}
