using CookComputing.XmlRpc;
using renstech.NET.nids.RemoteProc;

namespace renstech.NET.nids
{
    public class IPSRemoteProc
    {
        private readonly IIPSRemoteProc _proxy;

        public IPSRemoteProc(string addr, int port)
        {
            Addr = addr;
            Port = port;

            _proxy = XmlRpcProxyGen.Create<IIPSRemoteProc>();
            _proxy.Url = string.Format("http://{0}:{1}/", addr, port);
        }

        public string Addr { get; set; }
        public int Port { get; set; }

        public int GetFtpPort()
        {
            return _proxy.get_ftp_port();
        }

        public MapDesc[] GetMapsDesc()
        {
            return _proxy.get_map_list();
        }

        public DeviceTag[] GetDeviceTags()
        {
            return _proxy.get_device_tag_list();
        }
    }
}