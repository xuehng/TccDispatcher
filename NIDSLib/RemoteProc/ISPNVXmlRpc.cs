using CookComputing.XmlRpc;

namespace renstech.NET.nids
{
    public class XmlrpcUser
    {
        public string user_name;
        public string first_name;
        public string last_name;
        public int user_id;
        public string user_type;
        public int auth_level;
    }

    public class xmlrpcGroup
    {
        public int group_id;
        public string group_name;
    }

    public class xmlrpcGroupUser
    {
        public string user_name;
        public int user_id;
    }

    public class xmlrpcCall
    {
        public string callee_num;
        public string caller_num;
        public string callee_uuid;
        public string caller_uuid;
    }

    public class xmlrpcChannel
    {
        public string uuid;
        public string channel_caller;
        public string channel_callee;
        public string channel_app;
        public string channel_state;
        public string channel_direct;
    }

    public class xmlrpcPrefix
    {
        public string intercept_prefix;
        public string threeway_prefix;
        public string eavesdrop_prefix;
        public string intercom_prefix;
        public string pickup_prefix;
        public string conference_prefix;
        public string paging_prefix;
        public string findcall_prefix;
        public string broadcast_prefix;
    }

    public class xmlrpcMixedGroup
    {
        public int group_id;
        public string group_name;
        public int owner_id;
    }

    public class xmlrpcUserMixedGroup
    {
        public int group_id;
        public string user_num;
    }

    public class xmlrpcUserOnlyNumber
    {
        public string user_num;
    }

    public interface ISPNVXmlRpc : IXmlRpcProxy
    {
        [XmlRpcMethod("supernova.get_all_users")]
        XmlrpcUser[] get_all_users();

        [XmlRpcMethod("supernova.get_groups")]
        xmlrpcGroup[] get_groups();

        [XmlRpcMethod("supernova.get_private_groups")]
        xmlrpcGroup[] get_private_groups(string owner_id);

        [XmlRpcMethod("supernova.get_all_online_users")]
        string[] get_all_online_users();

        [XmlRpcMethod("supernova.get_group_member")]
        xmlrpcGroupUser[] get_group_member(int id);

        [XmlRpcMethod("supernova.create_private_group")]
        int? create_private_group(string groupname, int ownerid);

        [XmlRpcMethod("supernova.delete_private_group")]
        string delete_private_group(int group_id, int ownerid);

        [XmlRpcMethod("supernova.get_private_group_member")]
        xmlrpcGroupUser[] get_private_group_member(string id);

        [XmlRpcMethod("supernova.add_private_group_member")]
        string add_private_group_member(int groupid, int userid);

        [XmlRpcMethod("supernova.clear_private_group_member")]
        string clear_private_group_member(string group_id);

        [XmlRpcMethod("supernova.remove_private_group_member")]
        string remove_private_group_member(int group_id, string number);

        [XmlRpcMethod("supernova.get_calls")]
        xmlrpcCall[] get_calls();

        [XmlRpcMethod("supernova.get_channels")]
        xmlrpcChannel[] get_channels();

        [XmlRpcMethod("supernova.get_intercept_prefix")]
        string get_intercept_prefix();

        [XmlRpcMethod("supernova.get_threeway_prefix")]
        string get_threeway_prefix();

        [XmlRpcMethod("supernova.get_eavesdrop_prefix")]
        string get_eavesdrop_prefix();

        [XmlRpcMethod("supernova.get_intercom_prefix")]
        string get_intercom_prefix();

        [XmlRpcMethod("supernova.get_pickup_prefix")]
        string get_pickup_prefix();

        [XmlRpcMethod("supernova.get_conference_prefix")]
        string get_conference_prefix();

        [XmlRpcMethod("supernova.get_paging_prefix")]
        string get_paging_prefix();

        [XmlRpcMethod("supernova.get_findcall_prefix")]
        string get_findcall_prefix();

        [XmlRpcMethod("supernova.get_broadcast_prefix")]
        string get_broadcast_prefix();

        [XmlRpcMethod("supernova.conf_add_member")]
        void add_conf_member(string moderator, string member);

        [XmlRpcMethod("supernova.conf_del_member")]
        void del_conf_member(string moderator, string member);

        [XmlRpcMethod("supernova.get_mixed_groups")]
        xmlrpcMixedGroup[] get_mixed_groups();

        [XmlRpcMethod("supernova.get_mixed_groups")]
        xmlrpcMixedGroup[] get_mixed_groups(int ownerid);

        [XmlRpcMethod("supernova.get_user_mixed_group")]
        xmlrpcUserMixedGroup[] get_user_mixed_group();

        [XmlRpcMethod("supernova.get_user_mixed_group_id")]
        int? get_user_mixed_group_id(string group_name, int owner_id);

        [XmlRpcMethod("supernova.get_mixed_group_owner")]
        string get_mixed_group_owner(int group_id);

        [XmlRpcMethod("supernova.get_mixed_group_member")]
        string[] get_mixed_group_memeber(int group_id);

        [XmlRpcMethod("supernova.create_mixed_group")]
        int? create_mixed_group(string group_name, int owner_id);

        [XmlRpcMethod("supernova.add_mixed_group_member")]
        string add_mixed_group_member(int group_id, string user_num);

        [XmlRpcMethod("supernova.delete_mixed_group")]
        string delete_mixed_group(int group_id, int ownerid);

        [XmlRpcMethod("supernova.remove_mixed_group_member")]
        string remove_mixed_group_member(int group_id, string usernum);

        [XmlRpcMethod("supernova.clear_mixed_group_member")]
        string clear_mixed_group_member(int group_id);

        [XmlRpcMethod("supernova.is_mixed_group_exist")]
        bool is_mixed_group_exist(int group_id, int ownerid);

        [XmlRpcMethod("supernova.update_mixed_group_name")]
        bool update_mixed_group_name(int groupid, string groupname);

        [XmlRpcMethod("supernova.save_mixed_group_member")]
        bool save_mixed_group_member(int groupid, string[] members);

        [XmlRpcMethod("supernova.save_mixed_group_order")]
        bool save_mixed_group_order(int[] groupids);

        [XmlRpcMethod("supernova.get_server_type")]
        int get_server_type();
    }
}
