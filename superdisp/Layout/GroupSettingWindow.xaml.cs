using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using renstech.NET.SupernovaDispatcher.Model;
using renstech.NET.SupernovaDispatcher.Utils;
using MessageWindow = renstech.NET.SupernovaDispatcher.Control.MessageWindow;

namespace renstech.NET.SupernovaDispatcher.Layout
{
    /// <summary>
    /// GroupSettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GroupSettingWindow : Window
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(GroupSettingWindow));

        private SpnvSubSystem _system = null;
        private List<User> _allUsers = null;
        private List<User> _allUsersPatternTwo = null;
        private List<Group> _deleteGroups = new List<Group>();
        private bool _isMixedGroupsModified;
        private bool _isMixedGroupsOrderChanged;

        public GroupSettingWindow(SpnvSubSystem system)
        {
            this.InitializeComponent();

            App app = App.Current as App;
            if (app.AppBkBrush != null)
                Background = app.AppBkBrush;

            _system = system;
            _isMixedGroupsModified = false;
            
            _allUsers = new List<User>(system.UserInfo.Users);
            lbxAllUsers.ItemsSource = _allUsers;

            cbxGroup.ItemsSource = _system.UserInfo.PrivateGroups;

            this.LvMixedGroups.ItemsSource = _system.UserInfo.MixedGroups;
            if(this.LvMixedGroups.Items.Count != 0)
                this.LvMixedGroups.SelectedIndex = 0;
            
            _allUsersPatternTwo = new List<User>(system.UserInfo.AllWithCustomeUsers);
            this.lvAllUsersWithCustomizedUser.ItemsSource = _allUsersPatternTwo;

            if (cbxGroup.Items.Count != 0)
            {
                cbxGroup.SelectedIndex = 0;
            }
        }

        private bool IsModified { get; set; }

        private void cbxPrivateGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Group group = cbxGroup.SelectedItem as Group;
            if (group != null)
            {
                lbxGroupUsers.ItemsSource = group.GroupUsers;
            }
            else
            {
                lbxGroupUsers.ItemsSource = null;
            }
        }

        private void LvMixedGroups_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Group group = LvMixedGroups.SelectedItem as Group;
            if (group != null)
            {
                //this.lvUsersOfCustomizedGroup.ItemsSource = group.GroupUsers;
                this.lvUsersOfCustomizedGroup.DataContext = group;
                this.lvUsersOfCustomizedGroup.SelectedIndex = -1;
            }
            else
            {
                //this.lvUsersOfCustomizedGroup.ItemsSource = null;
                this.lvUsersOfCustomizedGroup.DataContext = null;
            }
        }

        private void btnPrivateGroupAdd_Click(object sender, RoutedEventArgs e)
        {
            GroupCreationWindow dialog = new GroupCreationWindow();
            dialog.Owner = this;

            Log.Debug("btnPrivateGroupAdd_Click__GroupCreationWindow.ShowDialog BEFORE");
            if (dialog.ShowDialog() != true)
            {
                return;
            }
            
            Log.Debug("btnPrivateGroupAdd_Click__GroupCreationWindow.ShowDialog AFTER");

            if (_system.UserInfo.GroupGet(dialog.GroupName) != null)
            {
                GroupNameExists();
                return;
            }

            Group group = new Group();
            group.Id = -1;
            group.Name = dialog.GroupName;
            group.GroupType = Group.Type.Private;
            group.IsPendingNew = true;

            try
            {
                int? groupId = _system.Proxy.CreatePrivateGroup(group.Name, _system.LocalUser.Id);
                if ( groupId == null)
                {
                    GroupInfoSaveFailed();
                    return;
                }

                group.Id = (int) groupId;
                _system.UserInfo.AddGroup(group);
                cbxGroup.SelectedItem = group;
            }
            catch (Exception)
            {
                GroupInfoSaveFailed();
            }
        }

        private void btnPrivateGroupDel_Click(object sender, RoutedEventArgs e)
        {
            Group group = cbxGroup.SelectedItem as Group;
            if (group == null)
            {
                return;
            }

            //用户组信息的时时删除
            try
            {
                if (!_system.Proxy.DeletePrivateGroup(group.Id, _system.LocalUser.Id))
                {
                    GroupInfoSaveFailed();
                    return;
                }
            }
            catch (Exception)
            {
                GroupInfoSaveFailed();
                return;
            }

            _system.UserInfo.GroupDel(group.Name);

            if (cbxGroup.Items.Count != 0)
            {
                cbxGroup.SelectedIndex = 0;
            }
        }

        private void btnPrivateGroupUserDel_Click(object sender, RoutedEventArgs e)
        {
            Group group = cbxGroup.SelectedItem as Group;
            if (group == null)
            {
                return;
            }

            User user = lbxGroupUsers.SelectedItem as User;
            if (user == null)
            {
                return;
            }
            
            try
            {
                group.IsPendingModified = true;
                if (!_system.Proxy.RemovePrivateGroupMember(group.Id, user.Number))
                {
                    GroupInfoSaveFailed();
                    return;
                }
                group.DelUser(user.Id);
            }
            catch (Exception)
            {
                GroupInfoSaveFailed();
                return;
            }
        }

        private void btnPrivateGroupUserAdd_Click(object sender, RoutedEventArgs e)
        {
            Group group = cbxGroup.SelectedItem as Group;
            if (group == null)
            {
                return;
            }

            User user = lbxAllUsers.SelectedItem as User;
            if (user == null)
            {
                return;
            }

            if (group.GroupUsers.Contains(user))
            {
                UserExistsInGroup();
                return;
            }

            try
            {
                group.IsPendingModified = true;
                if (!_system.Proxy.AddPrivateGroupMember(group.Id, user.Id))
                {
                    GroupInfoSaveFailed();
                    return;
                }
                group.AddUser(user.Id);
            }
            catch (Exception)
            {
                GroupInfoSaveFailed();
                return;
            }
        }

        private void btnPrivateGroupsClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        //根据根据测字拼音排序得到首字母

        private void btnSearchFromServerUsers_Click(object sender, RoutedEventArgs e)
        {
            SearchUserWindow dialog = new SearchUserWindow(null);
            dialog.Owner = this;

            Log.Debug("btnSearchFromServerUsers_Click__SearchUserWindow.ShowDialog BEFORE");
            if (dialog.ShowDialog() != true)
            {
                return;
            }
            Log.Debug("btnSearchFromServerUsers_Click__SearchUserWindow.ShowDialog AFTER");

            User selectedUser = null;
            foreach (User user in _allUsers)
            {
                if ( !string.IsNullOrEmpty(dialog.UserNumber) &&
                    user.Number == dialog.UserNumber)
                {
                    selectedUser = user;
                    break;
                }

                if (!string.IsNullOrEmpty(dialog.UserName) &&
                    user.Name == dialog.UserName)
                {
                    selectedUser = user;
                    break;
                }
            }

            if (selectedUser != null)
            {
                lbxAllUsers.SelectedItem = selectedUser;
                lbxAllUsers.ScrollIntoView(selectedUser);
            }
        }

        private void btnSearchFromAllUsers_OnClick(object sender, RoutedEventArgs e)
        {
            SearchUserWindow dialog = new SearchUserWindow(null);
            dialog.Owner = this;

            Log.Debug("btnSearchFromAllUsers_OnClick__SearchUserWindow.ShowDialog BEFORE");
            if (dialog.ShowDialog() != true)
            {
                return;
            }

            Log.Debug("btnSearchFromAllUsers_OnClick__SearchUserWindow.ShowDialog AFTER");

            User selectedUser = null;
            foreach (User user in _allUsersPatternTwo)
            {
                if (!string.IsNullOrEmpty(dialog.UserNumber) && user.Number == dialog.UserNumber)
                {
                    selectedUser = user;
                    break;
                }

                if (!string.IsNullOrEmpty(dialog.UserName) && user.Name == dialog.UserName)
                {
                    selectedUser = user;
                    break;
                }
            }

            if (selectedUser != null)
            {
                lvAllUsersWithCustomizedUser.SelectedItem = selectedUser;
                lvAllUsersWithCustomizedUser.ScrollIntoView(selectedUser);
            }
        }

        private void btnMixedGroupsMoveUp_OnClick(object sender, RoutedEventArgs e)
        {
            if (LvMixedGroups.SelectedItem != null)
            {
                Group group = LvMixedGroups.SelectedItem as Group;
                var curIndex = _system.UserInfo.MixedGroups.IndexOf(group);
                if (curIndex != 0)
                {
                    if (!MixedGroupsOrderSave(curIndex, curIndex - 1))
                    {
                        GroupInfoSaveFailed();
                        return;
                    }
                    _system.UserInfo.MixedGroups.Move(curIndex, curIndex - 1);
                    _isMixedGroupsOrderChanged = true;
                }
            }
        }

        private void btnMixedGroupsMoveDown_OnClick(object sender, RoutedEventArgs e)
        {
            if (LvMixedGroups.SelectedItem != null)
            {
                Group group = LvMixedGroups.SelectedItem as Group;
                var curIndex = _system.UserInfo.MixedGroups.IndexOf(group);
                if (curIndex != _system.UserInfo.MixedGroups.Count - 1)
                {
                    if (!MixedGroupsOrderSave(curIndex, curIndex + 1))
                    {
                        GroupInfoSaveFailed();
                        return;
                    }
                    _system.UserInfo.MixedGroups.Move(curIndex, curIndex + 1);
                    _isMixedGroupsOrderChanged = true;
                }
            }
        }

        private bool MixedGroupsOrderSave(int oldIndex, int newIndex)
        {
            //获得当前序列
            int[] groupIds = new int[_system.UserInfo.MixedGroups.Count];
            foreach (var mixedGroup in _system.UserInfo.MixedGroups)
            {
                groupIds[_system.UserInfo.MixedGroups.IndexOf(mixedGroup)] = mixedGroup.Id;
            }

            int temp;
            
            //位置移动
            temp = groupIds[oldIndex];
            groupIds[oldIndex] = groupIds[newIndex];
            groupIds[newIndex] = temp;

            //保存
            try
            {
                //_system.Proxy.SaveMixedGroupOrder(groupIds);
                if(_system.Proxy.SaveMixedGroupOrder(groupIds))
                    return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void btnMixedGroupsAdd_OnClick(object sender, RoutedEventArgs e)
        {
            GroupCreationWindow dialog = new GroupCreationWindow();
            dialog.Owner = this;

            Log.Debug("btnMixedGroupsAdd_OnClick__GroupCreationWindow.ShowDialog BEFORE");
            if (dialog.ShowDialog() != true)
            {
                return;
            }
            Log.Debug("btnMixedGroupsAdd_OnClick__GroupCreationWindow.ShowDialog AFTER");

            Group tempGroup;
            if (_system.LocalUser != null)
            {
                //时时添加混编组
                int? groupId;
                try
                {
                    groupId = _system.Proxy.CreateMixedGroup(dialog.GroupName, _system.LocalUser.Id);
                    if (groupId == null)
                    {
                        GroupInfoSaveFailed();
                    }
                    else
                    {
                        tempGroup = new Group { GroupType = Group.Type.Mixed, Name = dialog.GroupName, Id = unchecked((int)groupId) };

                        _system.UserInfo.MixedGroups.Add(tempGroup);
                        this.LvMixedGroups.SelectedItem = tempGroup;
                    }
                }
                catch (Exception)
                {
                    GroupInfoSaveFailed();
                }
            }
        }

        private void btnMixedGroupsDel_OnClick(object sender, RoutedEventArgs e)
        {
            Group group = LvMixedGroups.SelectedItem as Group;
            if (group != null)
            {
                //时时删除
                try
                {
                    if (_system.Proxy.DeleteMixedGroup(group.Id, _system.LocalUser.Id))
                    {
                        _system.UserInfo.MixedGroups.Remove(group);
                    }
                    else
                    {
                        GroupInfoSaveFailed();
                    }
                }
                catch (Exception)
                {
                    GroupInfoSaveFailed();
                }
            }
        }

        private void btnMixedGroupsEdit_OnClick(object sender, RoutedEventArgs e)
        {
            Group group = LvMixedGroups.SelectedItem as Group;
            if (group != null)
            {
                GroupEditWindow dialog = new GroupEditWindow();
                dialog.Owner = this;
                dialog.GroupName = group.Name;

                Log.Debug("btnMixedGroupsEdit_OnClick__GroupEditWindow.ShowDialog BEFORE");
                if (dialog.ShowDialog() != true || string.IsNullOrEmpty(dialog.GroupName))
                {
                    return;
                }

                Log.Debug("btnMixedGroupsEdit_OnClick__GroupEditWindow.ShowDialog AFTER");

                try
                {
                    //时时更新组名
                    if (_system.Proxy.UpdateMixedGroupName(group.Id, dialog.GroupName))
                    {
                        group.Name = dialog.GroupName;
                        return;
                    }

                    GroupInfoSaveFailed();
                }
                catch (Exception ex)
                {
                    GroupInfoSaveFailed();
                }
            }
        }

        private void btnMixedGroupUserMoveUp_OnClick(object sender, RoutedEventArgs e)
        {
            User user = lvUsersOfCustomizedGroup.SelectedItem as User;
            Group group = LvMixedGroups.SelectedItem as Group;
            if (user != null && group != null)
            {
                var curIndex = group.GroupUsers.IndexOf(user);

                //将用户顺序全部保存到服务器
                if (!MixedGroupUsersSave(group))
                {
                    GroupInfoSaveFailed();
                    return;
                }
                group.UserMoveUp(user.Id, curIndex);
            }
        }

        private void btnMixedGroupUserMoveDown_OnClick(object sender, RoutedEventArgs e)
        {
            User user = lvUsersOfCustomizedGroup.SelectedItem as User;
            Group group = LvMixedGroups.SelectedItem as Group;
            if (user != null && group != null)
            {
                var curIndex = group.GroupUsers.IndexOf(user);

                //将用户顺序全部保存到服务器
                if (!MixedGroupUsersSave(group))
                {
                    GroupInfoSaveFailed();
                    return;
                }
                group.UserMoveDown(user.Id, curIndex);
            }
        }

        private bool MixedGroupUsersSave(Group mixedGroup)
        {
            string[] tempUsers = new string[mixedGroup.GroupUsers.Count];
            foreach (var user in mixedGroup.GroupUsers)
            {
                tempUsers[mixedGroup.GroupUsers.IndexOf(user)] = user.Number;
            }

            try
            {
                if(_system.Proxy.SaveMixedGroupMember(mixedGroup.Id, tempUsers))
                    return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void btnMixedGroupUserAdd_OnClick(object sender, RoutedEventArgs e)
        {
            User selectedUser = lvAllUsersWithCustomizedUser.SelectedItem as User;
            Group group = LvMixedGroups.SelectedItem as Group;
            if (selectedUser != null && group != null)
            {
                foreach (var user in group.GroupUsers)
                {
                    if(selectedUser.Number == user.Number && selectedUser.Name == user.Name)
                        return;
                }
                
                //在服务器端进行添加
                try
                {
                    if (_system.Proxy.AddMixedGroupMember(group.Id, selectedUser.Number))
                    {
                        group.AddUser(selectedUser.Number);
                    }
                    else
                    {
                        GroupInfoSaveFailed();
                    }
                }
                catch
                {
                    GroupInfoSaveFailed();
                }
            }
        }

        private void btnMixedGroupUserDel_OnClick(object sender, RoutedEventArgs e)
        {
            User selectedUser = lvUsersOfCustomizedGroup.SelectedItem as User;
            Group group = LvMixedGroups.SelectedItem as Group;
            if (selectedUser != null && group != null)
            {
                //在服务器端删除
                try
                {
                    if (_system.Proxy.RemoveMixedGroupMember(group.Id, selectedUser.Number))
                    {
                        group.DelUser(selectedUser.Number);
                    }
                    else
                    {
                        GroupInfoSaveFailed();
                    }
                        
                }
                catch (Exception)
                {
                    GroupInfoSaveFailed();
                }
            }
        }

        private void btnMixedGroupClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void btnSortByName_Checked(object sender, RoutedEventArgs e)
        {
            btnSortByNum.IsChecked = false;

            _allUsers.Sort(CompareUserByName);
            lbxAllUsers.Items.Refresh();
        }

        private static string GetOneIndex(string strTest)
        {
            StringBuilder strResult = new StringBuilder();
            foreach (char c in strTest)
            {
                if (((int)c >= 33) && ((int)c <= 126))
                {
                    strResult.Append(c.ToString());//如果是字母或符号，则返回原字符
                }
                else
                {
                    strResult.Append(GetGbkX(c.ToString()));//累加拼音声母
                }
            }
            return strResult.ToString();
        }
        private static string GetGbkX(string str)
        {
            if (str.CompareTo("吖") < 0) return str;
            if (str.CompareTo("八") < 0) return "A";
            if (str.CompareTo("嚓") < 0) return "B";
            if (str.CompareTo("咑") < 0) return "C";
            if (str.CompareTo("妸") < 0) return "D";
            if (str.CompareTo("发") < 0) return "E";
            if (str.CompareTo("旮") < 0) return "F";
            if (str.CompareTo("铪") < 0) return "G";
            if (str.CompareTo("讥") < 0) return "H";
            if (str.CompareTo("咔") < 0) return "J";
            if (str.CompareTo("垃") < 0) return "K";
            if (str.CompareTo("嘸") < 0) return "L";
            if (str.CompareTo("拏") < 0) return "M";
            if (str.CompareTo("噢") < 0) return "N";
            if (str.CompareTo("妑") < 0) return "O";
            if (str.CompareTo("七") < 0) return "P";
            if (str.CompareTo("亽") < 0) return "Q";
            if (str.CompareTo("仨") < 0) return "R";
            if (str.CompareTo("他") < 0) return "S";
            if (str.CompareTo("哇") < 0) return "T";
            if (str.CompareTo("夕") < 0) return "W";
            if (str.CompareTo("丫") < 0) return "X";
            if (str.CompareTo("帀") < 0) return "Y";
            if (str.CompareTo("咗") < 0) return "Z";

            return str;
        }

        private static int CompareUserByNum(User x, User y)
        {
            int xnum = 0; 
            bool xparse = int.TryParse(x.Number, out xnum);
            
            int ynum = 0;
            bool yparse = int.TryParse(y.Number, out ynum);

            if (!xparse && !yparse)
                return 0;

            if (!xparse && yparse)
                return 1;

            if (xparse && !yparse)
                return -1;

            if (xnum == ynum)
                return 0;

            if (xnum > ynum)
                return 1;

            if (xnum < ynum)
                return -1;

            return 0;
        }

        private static int CompareUserByName(User x, User y)
        {
            string xpron = GetOneIndex(x.Name);
            string ypron = GetOneIndex(y.Name);

            if (xpron == ypron)
                return 0;

            return string.CompareOrdinal(xpron, ypron);
        }

        private void btnSortByNum_Checked(object sender, RoutedEventArgs e)
        {
            btnSortByName.IsChecked = false;

            _allUsers.Sort(CompareUserByNum);
            lbxAllUsers.Items.Refresh();
        }

        private void BtnKeyboardClick(object sender, RoutedEventArgs e)
        {
            OnScreenKeyboard.StartOsk(this);
        }

        private void GroupInfoSaveFailed()
        {
            Log.Debug("GroupInfoSaveFailed__MessageWindow.ShowDialog BEFORE");

            MessageWindow dialog = new MessageWindow(
                Properties.Resources.IDS_SERVER_DISCONNECTED,
                Properties.Resources.IDS_GROUP_EDITTING_DENIED,
                MessageWindow.ButtonListType.ButtonOk, MessageWindow.IconType.IconError) { Owner = this };
            dialog.ShowDialog();

            Log.Debug("GroupInfoSaveFailed__MessageWindow.ShowDialog AFTER");
        }

        private void GroupNameExists()
        {
            Log.Debug("GroupNameExists__MessageWindow.ShowDialog BEFORE");

            MessageWindow dialog = new MessageWindow(
                Properties.Resources.IDS_GROUPNAMEEXISTS_TITLE,
                Properties.Resources.IDS_GROUPNAMEEXISTS_CONTENT,
                MessageWindow.ButtonListType.ButtonOk, MessageWindow.IconType.IconError) { Owner = this };
            dialog.ShowDialog();

            Log.Debug("GroupNameExists__MessageWindow.ShowDialog AFTER");
        }

        private void UserExistsInGroup()
        {
            Log.Debug("UserExistsInGroup__MessageWindow.ShowDialog BEFORE");

            MessageWindow dialog = new MessageWindow(
                Properties.Resources.IDS_USEREXISTSINGROUP_TITLE,
                Properties.Resources.IDS_USEREXISTSINGROUP_CONTNET,
                MessageWindow.ButtonListType.ButtonOk, MessageWindow.IconType.IconError) { Owner = this };
            dialog.ShowDialog();

            Log.Debug("UserExistsInGroup__MessageWindow.ShowDialog AFTER");
        }
    }
}