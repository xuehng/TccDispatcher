using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace renstech.NET.SupernovaDispatcher.Model
{
    public class Group : INotifyPropertyChanged, ICloneable
    {
        #region Type enum

        public enum Type
        {
            None,
            Public,
            Private,
            AllUser,
            Customized,
            Mixed,
            Others
        }

        #endregion

        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Group));

        public static UserInfo UserInfo;
        public static int LineUserCount;
        private ObservableCollection<User> _users = new ObservableCollection<User>();
        private string _name;

        public Group()
        {
            Id = -1;
            IsPendingNew = false;
            IsPendingModified = false;
        }

        public int Id { get; set; }
        public string Name
        {
            get { return _name; } 
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Name"));
                }
            }
        }

        public bool IsModified { get; set; }
        
        public Type GroupType { get; set; }
        
        public bool IsAllUserGroup
        {
            get { return GroupType == Type.AllUser; }
        }

        public bool IsPublicGroup
        {
            get { return GroupType == Type.Public; }
        }

        public bool IsPrivateGroup
        {
            get { return GroupType == Type.Private; }
        }

        public bool IsCustomizedGroup
        {
            get { return GroupType == Type.Customized; }
        }

        public bool IsOtherGroup
        {
            get { return GroupType == Type.Others; }
        }

        public bool IsMixedGroup
        {
            get { return GroupType == Type.Mixed; }
        }

        public bool CanPaging
        {
            get { return GroupType == Type.Public || GroupType == Type.Private || GroupType == Type.AllUser; }
        }

        public bool CanConference
        {
            get { return GroupType == Type.Public || GroupType == Type.Private || GroupType == Type.AllUser || GroupType == Type.Mixed ; }            
        }

        public bool CanFindConf
        {
            get { return GroupType == Type.Public || GroupType == Type.Private || GroupType == Type.AllUser; }                        
        }

        public bool IsPendingNew { get; set; }
        public bool IsPendingModified { get; set; }

        public ObservableCollection<User> GroupUsers
        {
            get { return _users; }
            set { _users = value;
                OnPropertyChanged(new PropertyChangedEventArgs("GroupUsers"));
            }
        }

        public int UserCount
        {
            get { return _users.Count; }
        }

        public List<List<object>> GroupUserLines
        {
            get
            {
                var lines = new List<List<object>>();

                //LineUserCount出现过为0的情况，所以此处增加条件
                if (LineUserCount == 0)
                    LineUserCount = 7;
                int lineCount = _users.Count/LineUserCount;
                for (int i = 0; i < lineCount; i++)
                {
                    var line = new List<object>();
                    for (int j = i*LineUserCount; j < i*LineUserCount + LineUserCount; j++)
                    {
                        line.Add(_users[j]);
                    }
                    lines.Add(line);
                }

                if (_users.Count%LineUserCount != 0)
                {
                    var line = new List<object>();
                    for (int i = lineCount*LineUserCount; i < _users.Count; i++)
                    {
                        line.Add(_users[i]);
                    }
                    lines.Add(line);
                }

                return lines;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        public User GetUser(int id)
        {
            try
            {
                return _users.FirstOrDefault(user => user.Id == id);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
        }

        public User GetUserByName(string name)
        {
            try
            {
                return _users.FirstOrDefault(user => user.Name == name);
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }

        public User GetUserByNumber(string number)
        {
            if (string.IsNullOrEmpty(number))
                return null;

            try
            {
                return _users.FirstOrDefault(user => user.Number == number);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
        }

        public bool UserMoveUp(int userId, int curIndex)
        {
            if (curIndex <= 0 || curIndex > UserCount - 1)
                return false;
            
            User user = GetUser(userId);
            if (user == null)
                return false;

            _users.Move(curIndex, curIndex - 1);
            IsModified = true;
            return true;
        }

        public bool UserMoveDown(int userId, int curIndex)
        {
            if (curIndex < 0 || curIndex >= UserCount - 1)
                return false;

            User user = GetUser(userId);
            if (user == null)
                return false;

            _users.Move(curIndex, curIndex + 1);
            IsModified = true;
            return true;
        }

        public bool AddUser(int userId)
        {
            if (GetUser(userId) != null)
                return false;

            User usr = UserInfo.UserGet(userId, GroupType == Type.Customized ? User.Type.Customized : User.Type.Normal);
            if (usr != null)
            {
                _users.Add(usr);
                OnPropertyChanged(new PropertyChangedEventArgs("GroupUsers"));
                OnPropertyChanged(new PropertyChangedEventArgs("GroupUserLines"));
                OnPropertyChanged(new PropertyChangedEventArgs("UserCount"));
                IsModified = true;
                return true;
            }

            return false;
        }

        public bool AddUser(string userNumber)
        {
            if (GetUserByNumber(userNumber) != null)
                return false;

            User usr = UserInfo.UserGetByNumber(userNumber, GroupType == Type.Customized ? User.Type.Customized : User.Type.Normal);
            if (usr != null)
            {
                _users.Add(usr);
                OnPropertyChanged(new PropertyChangedEventArgs("GroupUsers"));
                OnPropertyChanged(new PropertyChangedEventArgs("GroupUserLines"));
                OnPropertyChanged(new PropertyChangedEventArgs("UserCount"));
                IsModified = true;
                return true;
            }

            return false;
        }

        public bool AddUser(User user)
        {
            if (user != null)
            {
                _users.Add(user);
                OnPropertyChanged(new PropertyChangedEventArgs("GroupUsers"));
                OnPropertyChanged(new PropertyChangedEventArgs("GroupUserLines"));
                OnPropertyChanged(new PropertyChangedEventArgs("UserCount"));
                IsModified = true;
                return true;
            }

            return false;
        }

        public bool DelUser(int userId)
        {
            User usr = GetUser(userId);
            if (usr == null)
                return false;

            if (_users.Remove(usr))
            {
                IsModified = true;
                OnPropertyChanged(new PropertyChangedEventArgs("GroupUsers"));
                OnPropertyChanged(new PropertyChangedEventArgs("GroupUserLines"));
                OnPropertyChanged(new PropertyChangedEventArgs("UserCount"));
                return true;
            }
            return false;
        }

        public bool DelUser(string userNumber)
        {
            User usr = GetUserByNumber(userNumber);
            if (usr == null)
                return false;

            if (_users.Remove(usr))
            {
                IsModified = true;
                OnPropertyChanged(new PropertyChangedEventArgs("GroupUsers"));
                OnPropertyChanged(new PropertyChangedEventArgs("GroupUserLines"));
                OnPropertyChanged(new PropertyChangedEventArgs("UserCount"));
                return true;
            }
            return false;
        }

        public object Clone()
        {
            var group = new Group();

            group.GroupType = GroupType;
            //Group的ID不能克隆
            //group.Id = Id;
            group.IsPendingModified = IsPendingModified;
            group.IsPendingNew = IsPendingNew;
            group.Name = Name;

            return group;
        }
    }
}