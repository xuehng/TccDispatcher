using System.Collections.Generic;
using System.Linq;
using renstech.NET.nids;
using renstech.NET.nids.RemoteProc;

namespace renstech.NET.SupernovaDispatcher.Model
{
    internal class ServerInfo
    {
        private readonly SpnvRemoteProc _proxy;

        public ServerInfo(string addr, int port)
        {
            _proxy = new SpnvRemoteProc(addr, port);
        }

        public List<User> GetRemoteUsers()
        {
            if (_proxy == null)
                return null;

            var users = _proxy.GetUsers();
            if (users == null)
                return null;

            return (from user in users
                    where user.user_name != "superadmin"
                    select new User(user.user_id, user.first_name, user.last_name, user.user_name)).ToList();
        }

        public List<Group> GetRemoteGroups()
        {
            if (_proxy == null)
                return null;

            xmlrpcGroup[] groups = _proxy.GetGroups();
            if (groups == null)
                return null;

            List<Group> localgroups = new List<Group>();
            foreach (xmlrpcGroup group in groups)
            {
                if (group.group_name == "default" || group.group_name == "administrators")
                    continue;

                xmlrpcGroupUser[] users = _proxy.GetGroupUsers(group.group_id);

                Group tmp = new Group {Id = @group.group_id, Name = @group.group_name, GroupType = Group.Type.Public};

                foreach (var user in users)
                {
                    tmp.AddUser(user.user_id);
                }

                localgroups.Add(tmp);
            }

            return localgroups;
        }

        public List<Group> GetRemotePrivateGroups(int userId)
        {
            if (_proxy == null)
                return null;

            xmlrpcGroup[] groups = _proxy.GetPrivateGroups(userId);
            if (groups == null)
                return null;

            List<Group> localgroups = new List<Group>();
            foreach (xmlrpcGroup group in groups)
            {
                xmlrpcGroupUser[] users = _proxy.GetPrivateGroupUsers(group.group_id);

                Group grp = new Group { Id = @group.group_id, Name = @group.group_name, GroupType = Group.Type.Private };

                foreach (xmlrpcGroupUser user in users)
                {
                    grp.AddUser(user.user_id);
                }

                localgroups.Add(grp);
            }

            return localgroups;
        }

        public string[] GetOnlineUsers()
        {
            if (_proxy == null)
                return null;

            return _proxy.GetOnlineUsers();
        }

        public xmlrpcChannel[] GetChannels()
        {
            return _proxy.GetChannels();
        }

        public xmlrpcPrefix GetPrefix()
        {
            return _proxy.GetPrefix();
        }
    }
}