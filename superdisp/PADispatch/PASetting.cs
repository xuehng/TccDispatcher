using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using renstech.NET.SupernovaDispatcher.Model;

namespace renstech.NET.SupernovaDispatcher.PADispatch
{
    public class PASetting : SettingItem
    {
        public PASetting()
        {
            Version = "00001";
            LineZoneCount = 8;
            LineZoneHeight = 80;
            PAServerAddr = "192.168.0.226";
            NIDSPort = 7085;
            NIDSEventPort = 9332;
            DialPrefix = "*81";
            EventNode = "paEvent@pubsub.renstech.com";
        }

        public string Version;
        public string PAServerAddr;
        public int NIDSPort;
        public int NIDSEventPort;

        public int LineZoneHeight;
        public int LineZoneCount;

        public string DialPrefix;

        public string EventNode;
    }
}
