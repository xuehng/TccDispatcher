using System;
using System.Windows;
using renstech.NET.SupernovaDispatcher.Model;
using renstech.NET.SupernovaDispatcher.Utils;
using MessageWindow = renstech.NET.SupernovaDispatcher.Control.MessageWindow;

namespace renstech.NET.SupernovaDispatcher.Layout
{
	/// <summary>
	/// UserCustomizeWindow.xaml 的交互逻辑
	/// </summary>
	public partial class UserCustomizeWindow : Window
	{
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(UserCustomizeWindow));

	    private bool _dataBinded;
	    private readonly SpnvSubSystem _system;

		public UserCustomizeWindow(SpnvSubSystem system)
		{
			InitializeComponent();

            App app = Application.Current as App;
            if (app != null && app.AppBkBrush != null)
                Background = app.AppBkBrush;

		    _system = system;

            //如果在此处重新加载数据，会导致自定义用户在内存中包含多组对象，修改就不能同步
		    //if(_system.InitializeCustomizeUser())
            UpdateUsers();
            
            btnDelUser.IsEnabled = false;
            btnModifyUser.IsEnabled = false;
		}

        public bool IsModified { get; set; }

	    private void UpdateUsers()
        {
            Group group = _system.UserInfo.GroupGet(UserInfo.GroupCustomId, Group.Type.Customized);
            if (group == null)
                return;

            _dataBinded = true;
            lstUsers.ItemsSource = group.GroupUsers;
        }

        private void BtnUserAddClick(object sender, RoutedEventArgs e)
        {
            var dialog = new UserCustomizedNewWindow {Owner = this};

            Log.Debug("BtnUserAddClick__UserCustomizedNewWindow.ShowDialog BEFORE");
            if (dialog.ShowDialog() != true)
                return;

            Log.Debug("BtnUserAddClick__UserCustomizedNewWindow.ShowDialog AFTER");

            if (IsUserExisted(dialog.ExtensionCurrent))
            {
                Log.Debug("BtnUserAddClick__MessageWindow.ShowDialog BEFORE");

                MessageWindow msg = new MessageWindow(
                    Properties.Resources.IDS_ERROR_WINDOW_TITLE,
                    Properties.Resources.IDS_ERR_EXT_EXIST,
                    MessageWindow.ButtonListType.ButtonOk,
                    MessageWindow.IconType.IconError) { Owner = this };
                msg.ShowDialog();

                Log.Debug("BtnUserAddClick__MessageWindow.ShowDialog AFTER");

                return;
            }

            if (_system.UserInfo.CustomizedUsersAdd(dialog.UserName, dialog.ExtensionCurrent))
            {
                IsModified = true;
                UpdateUsers();
            }                
        }

        private void LstUsersSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            bool enable = (lstUsers.SelectedItem != null);
            btnDelUser.IsEnabled = enable;
            btnModifyUser.IsEnabled = enable;
        }

        private bool IsUserExisted(string extension)
        {
            var user = _system.UserInfo.UserGetByNumber(extension, User.Type.None);
            return (user != null);
        }

        private void BtnUserEditClick(object sender, RoutedEventArgs e)
        {
            User user = lstUsers.SelectedItem as User;

            if (user == null)
                return;

            var dialog = new UserCustomizedNewWindow
                {
                    Owner = this,
                    UserName = user.LastName,
                    ExtensionPrevious = user.Number,
                    ExtensionCurrent = user.Number
                };

            Log.Debug("BtnUserEditClick__UserCustomizedNewWindow.ShowDialog BEFORE");

            if (dialog.ShowDialog() != true)
                return;

            Log.Debug("BtnUserEditClick__UserCustomizedNewWindow.ShowDialog AFTER");

            if (dialog.IsExtensionChanged && IsUserExisted(dialog.ExtensionCurrent))
            {
                Log.Debug("BtnUserEditClick__MessageWindow.ShowDialog BEFORE");

                MessageWindow msg = new MessageWindow(
                    Properties.Resources.IDS_ERROR_WINDOW_TITLE,
                    Properties.Resources.IDS_ERR_EXT_EXIST,
                    MessageWindow.ButtonListType.ButtonOk,
                    MessageWindow.IconType.IconError) { Owner = this };
                msg.ShowDialog();

                Log.Debug("BtnUserEditClick__MessageWindow.ShowDialog AFTER");

                return;
            }

            user.LastName = dialog.UserName;
            user.Number = dialog.ExtensionCurrent;

            //先尝试对混编组中用户进行修改
            try
            {
                foreach (var mixedGroup in _system.UserInfo.MixedGroups)
                {
                    if (mixedGroup.GroupUsers.Contains(user))
                    {
                        string[] tempUsers = new string[mixedGroup.GroupUsers.Count];
                        foreach (var userMixed in mixedGroup.GroupUsers)
                        {
                            tempUsers[mixedGroup.GroupUsers.IndexOf(userMixed)] = userMixed.Number;
                        }

                        try
                        {
                            _system.Proxy.SaveMixedGroupMember(mixedGroup.Id, tempUsers);
                        }
                        catch (Exception ex)
                        {
                            Log.Debug("BtnUserEditClick__MessageBox.Show ex BEFORE");
                            MessageBox.Show(String.Format("对应混编组：{0}信息同步失败!",mixedGroup.Name));
                            Log.Debug("BtnUserEditClick__MessageBox.Show ex AFTER"); 
                            return;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
            
            IsModified = true;
        }

        private void BtnUserDelClick(object sender, RoutedEventArgs e)
        {
            User user = lstUsers.SelectedItem as User;
            if (user == null)
                return;
            
            //删除混编组中的自定义用户
            try
            {
                foreach (var mixedGroup in _system.UserInfo.MixedGroups)
                {
                    if (mixedGroup.GroupUsers.Contains(user))
                    {
                        try
                        {
                            if (_system.Proxy.RemoveMixedGroupMember(mixedGroup.Id, user.Number))
                                mixedGroup.DelUser(user.Id);
                        }
                        catch (Exception ex)
                        {
                            Log.Debug("BtnUserDelClick__MessageBox.Show ex BEFORE");
                            MessageBox.Show(String.Format("对应混编组：{0}信息同步失败!", mixedGroup.Name));
                            Log.Debug("BtnUserDelClick__MessageBox.Show ex AFTER"); 
                            return;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return;
            }

            //删除自定义用户组中的用户
            if (!_system.UserInfo.CustomizedUsersDel(user))
            {
                Log.Debug("BtnUserDelClick__MessageBox.Show BEFORE");
                MessageBox.Show("用户删除失败");
                Log.Debug("BtnUserDelClick__MessageBox.Show AFTER"); 
                return;
            }

            IsModified = true;

            Group group = _system.UserInfo.GroupGet(UserInfo.GroupCustomId, Group.Type.Customized);
            if (group != null && group.UserCount == 0 )
            {
                _system.UserInfo.GroupDel(group.Name);
                group = null;
            }

            if (group == null)
            {
                lstUsers.ItemsSource = null;
                _dataBinded = false;
            }
        }

	    private void UserCustomizeWindow_OnClosed(object sender, EventArgs e)
	    {
	        SaveMixedGroupRelated();
	        if (IsModified)
	            _system.CustomizedUserInfoSave();
	    }

        private void SaveMixedGroupRelated()
        {
            foreach (var mixedGroup in _system.UserInfo.MixedGroups)
            {
                string[] tempUsers = new string[mixedGroup.GroupUsers.Count];
                foreach (var user in mixedGroup.GroupUsers)
                {
                    tempUsers[mixedGroup.GroupUsers.IndexOf(user)] = user.Number;
                }

                try
                {
                    _system.Proxy.SaveMixedGroupMember(mixedGroup.Id, tempUsers);
                }
                catch (Exception)
                {

                }
            }
        }
	}
}