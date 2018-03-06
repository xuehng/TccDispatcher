using System;
using CookComputing.XmlRpc;

namespace renstech.NET.nids.RemoteProc
{
    public class SpnvRemoteProc
    {
        private readonly ISPNVXmlRpc _proxy;

        public SpnvRemoteProc(string addr, int port)
        {
            Addr = addr;
            Port = port;

            _proxy = (ISPNVXmlRpc)XmlRpcProxyGen.Create<ISPNVXmlRpc>();
            _proxy.Url = string.Format("http://{0}:{1}/", addr, port);
            _proxy.XmlEncoding = new System.Text.UTF8Encoding(); 
        }

        public string Addr { get; set; }
        public int Port { get; set; }

        public XmlrpcUser[] GetUsers()
        {
            return _proxy.get_all_users();
        }

        public xmlrpcGroup[] GetGroups()
        {
            return _proxy.get_groups();
        }

        public xmlrpcGroupUser[] GetGroupUsers(int grpId)
        {
            return _proxy.get_group_member(grpId);
        }

        public int? CreatePrivateGroup(string name, int ownerId)
        {
            return _proxy.create_private_group(name, ownerId);
        }

        public bool DeletePrivateGroup(int groupId, int ownerId)
        {
            if (_proxy.delete_private_group(groupId, ownerId) == "1")
                return true;
            return false;
        }

        public xmlrpcGroup[] GetPrivateGroups(int ownerId)
        {
            return _proxy.get_private_groups(ownerId.ToString());
        }

        public xmlrpcGroupUser[] GetPrivateGroupUsers(int groupId)
        {
            return _proxy.get_private_group_member(groupId.ToString());
        }

        public string ClearPrivateGroup(int groupId)
        {
            return  _proxy.clear_private_group_member(groupId.ToString());
        }

        public bool AddPrivateGroupMember(int groupId, int userId)
        {
            if(_proxy.add_private_group_member(groupId, userId) == "1")
                return true;
            return false;
        }

        public string[] GetOnlineUsers()
        {
            return _proxy.get_all_online_users();
        }

        public xmlrpcCall[] GetCalls()
        {
            return _proxy.get_calls();
        }

        public xmlrpcChannel[] GetChannels()
        {
            return _proxy.get_channels();
        }

        public xmlrpcPrefix GetPrefix()
        {
            xmlrpcPrefix prefix = new xmlrpcPrefix
                                      {
                                          intercept_prefix = _proxy.get_intercept_prefix(),
                                          threeway_prefix = _proxy.get_threeway_prefix(),
                                          eavesdrop_prefix = _proxy.get_eavesdrop_prefix(),
                                          intercom_prefix = _proxy.get_intercom_prefix(),
                                          pickup_prefix = _proxy.get_pickup_prefix(),
                                          conference_prefix = _proxy.get_conference_prefix(),
                                          paging_prefix = _proxy.get_paging_prefix(),
                                          findcall_prefix = _proxy.get_findcall_prefix(),
                                          broadcast_prefix = _proxy.get_broadcast_prefix()
                                      };
            return prefix;
        }

        public bool IsServerConncted()
        {
            int result;
            try
            {
                result = _proxy.get_server_type();
            }
            catch (Exception)
            {
                return false;
            }
            if (result == 1)
            {
                return true;
            }
            return false;
        }

        public void AddConfMember(string moderator, string member)
        {
            _proxy.add_conf_member(moderator, member);
        }

        public void DelConfMember(string moderator, string member)
        {
            _proxy.del_conf_member(moderator, member);
        }

        public xmlrpcMixedGroup[] GetMixedGroups()
        {
            return _proxy.get_mixed_groups();
        }

        public xmlrpcMixedGroup[] GetMixedGroups(int owner_id)
        {
            return _proxy.get_mixed_groups(owner_id);
        }

        public xmlrpcUserMixedGroup[] GetUserMixedGroup()
        {
            return _proxy.get_user_mixed_group();
        }

        public int? GetUserMixedGroupId(string group_name, int owner_id)
        {
            return _proxy.get_user_mixed_group_id(group_name, owner_id);
        }

        public string GetMixedGroupOwner(int group_id)
        {
            return _proxy.get_mixed_group_owner(group_id);
        }

        public string[] GetMixedGroupMember(int group_id)
        {
            return _proxy.get_mixed_group_memeber(group_id);
        }

        public int? CreateMixedGroup(string group_name, int owner_id)
        {
            return _proxy.create_mixed_group(group_name, owner_id);
        }

        public bool AddMixedGroupMember(int group_id, string user_num)
        {
            if (_proxy.add_mixed_group_member(group_id, user_num) == "1")
                return true;
            return false;
        }

        public bool DeleteMixedGroup(int group_id, int owner_id)
        {
            if (_proxy.delete_mixed_group(group_id, owner_id) == "1")
                return true;
            return false;
        }
        
        public bool RemoveMixedGroupMember(int group_id, string user_num)
        {
            if (_proxy.remove_mixed_group_member(group_id, user_num) == "1")
                return true;
            return false;
        }

        public bool RemovePrivateGroupMember(int group_id, string user_num)
        {
            if (_proxy.remove_private_group_member(group_id, user_num) == "1")
                return true;
            return false;
        }

        public string ClearMixedGroupMember(int group_id)
        {
            return _proxy.clear_mixed_group_member(group_id);
        }

        public bool IsMixedGroupExist(int group_id, int ownerid)
        {
            return _proxy.is_mixed_group_exist(group_id, ownerid);
        }

        public bool UpdateMixedGroupName(int groupid, string groupname)
        {
            return _proxy.update_mixed_group_name(groupid, groupname);
        }

        public bool SaveMixedGroupMember(int groupid, string[] members)
        {
            return _proxy.save_mixed_group_member(groupid, members);
        }

        public bool SaveMixedGroupOrder(int[] groupids)
        {
            return _proxy.save_mixed_group_order(groupids);
        }
    }
}