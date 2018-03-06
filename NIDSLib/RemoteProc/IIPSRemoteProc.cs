using CookComputing.XmlRpc;

namespace renstech.NET.nids.RemoteProc
{
    public class MapDesc
    {
        public int id { get; set; }
        public int map_ver { get; set; }
        public string map_name { get; set; }
        public string map_filename { get; set; }
    }

    public class DeviceTag
    {
        public int id { get; set; }
        public string description { get; set; }
        public string extension { get; set; }
        public string mac_addr { get; set; }
    }

    public interface IIPSRemoteProc : IXmlRpcProxy
    {
        [XmlRpcMethod("get_ftp_port")]
        int get_ftp_port();

        [XmlRpcMethod("get_map_list")]
        MapDesc[] get_map_list();

        [XmlRpcMethod("get_device_tag_list")]
        DeviceTag[] get_device_tag_list();
    }
}
