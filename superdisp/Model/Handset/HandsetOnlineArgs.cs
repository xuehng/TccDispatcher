using System;

namespace renstech.NET.SupernovaDispatcher.Model.Handset
{
    public class HandsetOnlineArgs : EventArgs
    {
        public HandsetOnlineArgs(int id)
        {
            Id = id;
        }

        public int Id { get; private set; }
    }
}
