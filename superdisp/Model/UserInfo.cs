using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using renstech.NET.SupernovaDispatcher.Properties;

namespace renstech.NET.SupernovaDispatcher.Model
{
    public class UserInfo
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(UserInfo));

        public const int GroupAllUsrId = 888;
        public const int GroupCustomId = 889;
        public const int GroupMixedId = 890;

        private readonly List<User> _user = new List<User>();
        private readonly List<User> _customizedUsers = new List<User>();

        private readonly ObservableCollection<Group> _groups = new ObservableCollection<Group>();
        private readonly ObservableCollection<Group> _mixedGroups = new ObservableCollection<Group>();
        private readonly ObservableCollection<Group> _privateGroups = new ObservableCollection<Group>();
        private readonly List<Group> _publicGroups = new List<Group>();

        public UserInfo()
        {
            Group.UserInfo = this;
        }

        public List<User> Users
        {
            get { return _user; }
        }

        public List<User> AllWithCustomeUsers
        {
            get
            {
                var allUsers = new List<User>();
                foreach (var user in _user)
                {
                    allUsers.Add(user);
                }
                foreach (var user in _customizedUsers)
                {
                    allUsers.Add(user);
                }
                return allUsers;
            }
        }

        public List<User> CustomizedUsers
        {
            get { return _customizedUsers; }
        }

        public ObservableCollection<Group> Groups
        {
            get { return _groups; }
        }

        public ObservableCollection<Group> MixedGroups
        {
            get { return _mixedGroups; }
        }

        private List<Group> PublicGroups
        {
            get { return _publicGroups; }
        }

        public ObservableCollection<Group> PrivateGroups
        {
            get { return _privateGroups; }
        }

        public void ClearUserInfo()
        {
            _user.Clear();
            
            //因为自定义用户组的创建在先，所以不可以把groups直接清空
            //_groups.Clear();
            for (int i = 0; i < _groups.Count; i++)
            {
                if(_groups[i].GroupType != Group.Type.Customized)
                    _groups.RemoveAt(i);
            }
            _publicGroups.Clear();
            _privateGroups.Clear();
            _mixedGroups.Clear();
        }

        public void InitializeUsers(List<User> users)
        {
            _user.Clear();

            users.ForEach(i =>
                              {
                                  i.UserType = User.Type.Normal;
                                  _user.Add(i);
                              });
        }

        public void CustomizedUsersInitialize(List<User> users)
        {
            if (users.Count == 0)
                return;

            _customizedUsers.Clear();

            Group group = GroupGet(GroupCustomId, Group.Type.Customized) ?? CreateCustomizedGroup();
            if (group == null)
                return;

            users.ForEach(i =>
                              {
                                  i.Id = _customizedUsers.Count;
                                  i.UserType = User.Type.Customized;
                                  _customizedUsers.Add(i);//自定义用户，
                                  group.AddUser(i.Number);//自定义用户组
                              });
        }

        public bool CustomizedUsersAdd(string name, string extension)
        {
            if (_customizedUsers.Any(usr => usr.Number == extension))
            {
                return false;
            }

            Group group = GroupGet(GroupCustomId, Group.Type.Customized) ?? CreateCustomizedGroup();
            if (group == null)
                return false;

            User user = new User {Id = _customizedUsers.Count, LastName = name, Number = extension};
            _customizedUsers.Add(user);

            bool result = group.AddUser(extension);
            return result;
        }

        public bool CustomizedUsersDel(User user)
        {
            Group group = GroupGet(GroupCustomId, Group.Type.Customized);
            if (group == null)
                return false;

            try
            {
                User userCustomized = _customizedUsers.FirstOrDefault(userFind => userFind.Number == user.Number);
                if (userCustomized != null && _customizedUsers.Remove(userCustomized))
                {
                    group.DelUser(user.Id);
                    foreach (var cUser in _customizedUsers)
                    {
                        cUser.Id = _customizedUsers.IndexOf(cUser);
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return false;
            }
        }

        public User UserGet(int userId, User.Type type = User.Type.Normal)
        {
            try
            {
                return _user.FirstOrDefault(user => user.Id == userId) ??
                    _customizedUsers.FirstOrDefault(user => user.Id == userId);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
        }

        public User UserGetByNumber(string number, User.Type type = User.Type.Normal)
        {
            try
            {
                return Users.FirstOrDefault(user => user.Number == number) ??
                _customizedUsers.FirstOrDefault(user => user.Number == number);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
        }

        public User UserGetByName(string name)
        {
            try
            {
                return Users.FirstOrDefault(user => user.Name == name);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }            
        }

        public Group GroupGet(int id, Group.Type type)
        {
            try
            {
                if (type != Group.Type.Mixed)
                    return _groups.FirstOrDefault(@group => @group.Id == id && @group.GroupType == type);
                return _mixedGroups.FirstOrDefault(@group => @group.Id == id);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
        }

        public Group GroupGet(string name)
        {
            try
            {
                return Groups.FirstOrDefault(@group => @group.Name == name);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
        }

        public Group GroupGet(string name, Group.Type type)
        {
            try
            {
                return Groups.FirstOrDefault(@group => @group.Name == name && @group.GroupType == type);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
        }

        public void CreateAllUserGroup()
        {
            var grpall = new Group
                             {
                                 Id = GroupAllUsrId,
                                 Name = Resources.IDS_GROUP_ALLUSER,
                                 GroupType = Group.Type.AllUser,
                             };

            foreach (User user in Users)
            {
                grpall.AddUser(user.Id);
            }

            AddGroup(grpall);
        }

        public Group CreateCustomizedGroup()
        {
            Group cust = GroupGet(GroupCustomId, Group.Type.Customized);
            if (cust != null)
                return cust;

            cust = new Group
                       {
                           Id = GroupCustomId,
                           Name = Resources.IDS_GROUP_CUSTOMIZED,
                           GroupType = Group.Type.Customized,
                       };

            if (!AddGroup(cust))    
                return null;

            return cust;
        }

        public bool AddGroup(Group group)
        {
            if (GroupGet(group.Name,group.GroupType) != null)
                return false;

            int index = 0;
            switch (group.GroupType)
            {
                case Group.Type.None:
                    return false;
                case Group.Type.AllUser:
                    Groups.Insert(0, group);
                    PublicGroups.Insert(0, group);
                    break;
                case Group.Type.Public:
                    for (index = 0; index < _groups.Count; index ++ )
                    {
                        if (_groups[index].GroupType == Group.Type.Customized)
                            break;

                        if (_groups[index].GroupType == Group.Type.Private)
                            break;
                    }
                    _groups.Insert(index, group);
                    _publicGroups.Add(group);

                    break;
                case Group.Type.Private:
                    Groups.Add(group);
                    PrivateGroups.Add(group);
                    break;
                case Group.Type.Customized:
                    for (index = 0; index < _groups.Count; index++)
                    {
                        if (_groups[index].GroupType == Group.Type.Private)
                            break;
                    }
                    _groups.Insert(index, group);
                    break;
                case Group.Type.Mixed:
                    _mixedGroups.Add(group);
                    break;
            }

            return true;
        }

        public bool GroupDel(string name)
        {
            Group group = GroupGet(name);
            switch (group.GroupType)
            {
                case Group.Type.AllUser:
                    return false;
                case Group.Type.Public:
                    return false;
                case Group.Type.Private:
                    PrivateGroups.Remove(group);
                    break;
                case Group.Type.Mixed:
                    MixedGroups.Remove(group);
                    break;
            } 
            return Groups.Remove(group);
        }
    }
}