using System;
using renstech.NET.InterProcPipe;

namespace renstech.NET.SupernovaDispatcher.Model.Handset
{
    public class HandsetMsgArgs : EventArgs
    {
        public HandsetMsgArgs(HandsetMsg msg)
        {
            Message = msg;
        }

        public HandsetMsg Message { get; private set; }
    }
}