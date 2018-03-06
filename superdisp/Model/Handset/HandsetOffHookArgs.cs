using System;

namespace renstech.NET.SupernovaDispatcher.Model.Handset
{
    public class HandsetOffHookArgs : EventArgs
    {
        public HandsetOffHookArgs(int id, bool offhook)
        {
            Id = id;
            IsOffhook = offhook;
        }

        public int Id { get; private set; }
        public bool IsOffhook { get; private set; }
    }
}
