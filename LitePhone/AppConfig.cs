using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace renstech.sharpsip
{
    public class APPConfig
    {
        public uint MaxCall { get; set; }
        public string UserAgent { get; set; }
        public string NameServer { get; set; }
        public string OutboundProxy { get; set; }
        public uint LogLevel { get; set; }
        public string LogFileName { get; set; }
        public bool UDPTransportEnable { get; set; }
        public int UDPTransportPort { get; set; }
        public bool TCPTransportEnable { get; set; }

        public APPConfig()
        {
            MaxCall = 4;
            UDPTransportPort = 5060;
            UserAgent = "SpnvSIPLib";
            LogLevel = 4;
        }

        internal bool GetSUAAppConfig(ref sua_config cfg)
        {
            cfg.max_call = MaxCall;
            cfg.nameserver = (NameServer != null) ? NameServer : "";
            cfg.outbound_proxy = (OutboundProxy != null) ? OutboundProxy : "";
            cfg.useragent = UserAgent;
            return true;
        }

        internal bool GetSUALogConfig(ref sua_log_config cfg)
        {
            cfg.level = LogLevel;
            cfg.log_filename = (LogFileName != null) ? LogFileName : "";
            return true;
        }
    }
}
