using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace renstech.NET.nids
{
    public enum LOGLEVEL
    {
        DEBUG,
        INFO,
        ERROR
    }

    public class LogArgs : EventArgs
    {
        public LogArgs(LOGLEVEL level, string info)
        {
            Level = level;
            Info = info;
        }

        public LOGLEVEL Level { get; private set; }
        public string Info { get; private set; }
    }
}
