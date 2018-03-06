using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using renstech.NET.SIPUA;
using renstech.NET.SupernovaDispatcher.Control;
using renstech.NET.SupernovaDispatcher.Interface;
using renstech.NET.SupernovaDispatcher.Layout;
using renstech.NET.SupernovaDispatcher.Model;
using renstech.NET.SupernovaDispatcher.Model.Handset;

namespace renstech.NET.SupernovaDispatcher
{
    /// <summary>
    /// SupernovaDispatchUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class SpnvPage : UserControl, IDispatchPage
    {
        private readonly SpnvSubSystem _subsystem;
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(SpnvPage));
        private User _highlightUser;
        private readonly DispatcherTimer _highlightTimer;

        public SpnvPage(SpnvSubSystem system)
        {
            this.InitializeComponent();

            _subsystem = system;

            _highlightTimer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 5)};
            _highlightTimer.Tick += highlight_stop_tick;
        }

        public bool Initialize()
        {
            Channel channel = _subsystem.Channels.AddChannel(btnChannel1);
            channel.Name = string.Format("{0}{1}", Properties.Resources.IDS_CHANNEL_NAME, channel.ChannelId);

            channel = _subsystem.Channels.AddChannel(btnChannel2);
            channel.Name = string.Format("{0}{1}", Properties.Resources.IDS_CHANNEL_NAME, channel.ChannelId);

            channel = _subsystem.Channels.AddChannel(btnChannel3);
            channel.Name = string.Format("{0}{1}", Properties.Resources.IDS_CHANNEL_NAME, channel.ChannelId);

            channel = _subsystem.Channels.AddChannel(btnChannel4);
            channel.Name = string.Format("{0}{1}", Properties.Resources.IDS_CHANNEL_NAME, channel.ChannelId);

            btnAutoAnswer.DataContext = _subsystem.Setting;
            btnAutoPhoneRedirect.DataContext = _subsystem.Setting;
            btnRecordFiles.DataContext = _subsystem.CallHisory;

            ShowPatternOne.Visibility = Visibility.Hidden;
            PatternTwoGrid.Visibility = Visibility.Visible;
            //btnSearch.Visibility = Visibility.Collapsed;
            searchGrid.Visibility = Visibility.Collapsed;

            PatternTwoList.ItemsSource = _subsystem.UserInfo.MixedGroups;
            lbxGroups.ItemsSource = _subsystem.UserInfo.Groups;
            tabctrlGroups.ItemsSource = _subsystem.UserInfo.Groups;

            if (App.HandsetMgr != null && App.HandsetMgr.HandsetLeft != null)
            {
                btnLeftHandset.DataContext = _subsystem.LeftHandsetInfo;
                App.HandsetMgr.HandsetLeft.OnHookStateChanged += OnHandsetHookStateChanged;
            }

            if (App.HandsetMgr != null && App.HandsetMgr.HandsetRight != null)
            {
                btnRightHandset.DataContext = _subsystem.RightHandsetInfo;
                App.HandsetMgr.HandsetRight.OnHookStateChanged += OnHandsetHookStateChanged;
            }

            Group.LineUserCount = _subsystem.Setting.UserButtonLineCount;
            UserLineControl.ButtonHeight = _subsystem.Setting.UserButtonLineHeight;
            UserLineControl.ColCount = _subsystem.Setting.UserButtonLineCount;
            UserLineControl.ItemButton_Click = btnUser_Click;

            UserLineControlMixedGroup.ButtonHeight = _subsystem.Setting.UserButtonLineHeight;
            UserLineControlMixedGroup.ItemButton_Click = btnMixedGroupUser_Click;
            UserLineControlMixedGroup.GroupButton_Click = btnMixedGroup_Click;

            return true;
        }

        public Subsystem GetSubsystem()
        {
            return _subsystem;
        }

        #region HelpPad_Below

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow setting = new SettingWindow(_subsystem) { Owner = Application.Current.MainWindow };

            Log.Debug("btnSetting_Click__setting.ShowDialog BEFORE");
            setting.ShowDialog();
            Log.Debug("btnSetting_Click__setting.ShowDialog AFTER");
        }

        private void btnVolumn_Click(object sender, RoutedEventArgs e)
        {
            VolumeAdjustWindow dialog = new VolumeAdjustWindow { };
            dialog.Owner = Window.GetWindow(this);

            Log.Debug("btnVolumn_Click__VolumeAdjustWindow.ShowDialog BEFORE");
            dialog.ShowDialog();
            Log.Debug("btnVolumn_Click__VolumeAdjustWindow.ShowDialog AFTER");
        }

        private void btnScreenSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (ShowPatternOne.Visibility == Visibility.Hidden && PatternTwoGrid.Visibility == Visibility.Visible)
            {
                ShowPatternOne.Visibility = Visibility.Visible;
                PatternTwoGrid.Visibility = Visibility.Hidden;
                //btnSearch.Visibility = Visibility.Visible;
                searchGrid.Visibility = Visibility.Visible;
            }
            else if (ShowPatternOne.Visibility == Visibility.Visible && PatternTwoGrid.Visibility == Visibility.Hidden)
            {
                ShowPatternOne.Visibility = Visibility.Hidden;
                PatternTwoGrid.Visibility = Visibility.Visible;
                //btnSearch.Visibility = Visibility.Collapsed;
                searchGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void btnDialPad_Click(object sender, RoutedEventArgs e)
        {
            DialPad dialPad = new DialPad(_subsystem) { Owner = Application.Current.MainWindow };

            Log.Debug("btnDialPad_Click__dialPad.ShowDialog BEFORE");
            dialPad.ShowDialog();
            Log.Debug("btnDialPad_Click__dialPad.ShowDialog AFTER");
        }

        private void btnRecordFilesClick(object sender, RoutedEventArgs e)
        {
            RecordFileListWindow dialog = new RecordFileListWindow(_subsystem) { };
            dialog.Owner = Window.GetWindow(this);

            Log.Debug("btnRecordFilesClick__dialPad.ShowDialog BEFORE"); 
            dialog.ShowDialog();
            Log.Debug("btnRecordFilesClick__dialPad.ShowDialog AFTER");
        }

        private void btnQuit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        #endregion

        #region DialPad

        private void btnDialPadClear_Click(object sender, RoutedEventArgs e)
        {
            txtDialString.Text = string.Empty;
        }

        private void btnDialPadNum_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug("Protocal Stack Log: (SpnvPage)btnDialPadNum_Click, Start");

            Button button = (Button)sender;
            txtDialString.Text += button.Content.ToString();

            Channel channel = _subsystem.Channels.GetActiveChannel();
            if (channel != null)
            {
                App.SIPUA.DialDTMF(channel.CallId, button.Content.ToString());

                Log.Debug(String.Format("Protocal Stack Log: (DialPad)btnDialPadNum_Click, DialDTMF:{0},{1}", channel.CallId, button.Content));
            }

            Log.Debug("Protocal Stack Log: (SpnvPage)btnDialPadNum_Click, End");
        }

        private void btnDialPadBackspace_Click(object sender, RoutedEventArgs e)
        {
            if (txtDialString.Text.Length <= 0)
                return;

            txtDialString.Text = txtDialString.Text.Length == 1 ? string.Empty : txtDialString.Text.Remove(txtDialString.Text.Length - 1);
        }

        private void btnDialPadDial_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug("Protocal Stack Log: (SpnvPage)btnDialPadDial_Click, Start");

            if (txtDialString.Text.Length == 0)
            {
                return;
            }

            int callId = -1;
            _subsystem.Channels.MakeCall(_subsystem.AccountId, txtDialString.Text, ref callId);

            Log.Debug(String.Format("Protocal Stack Log: (SpnvPage)btnDialPadDial_Click, Make Call:{0},{1},{2}", _subsystem.AccountId, txtDialString.Text, callId));

            Log.Debug("Protocal Stack Log: (SpnvPage)btnDialPadDial_Click, End");
        }

        #endregion

        #region HelpPad_Above

        private void btnAutoPhoneRedirect_Click(object sender, RoutedEventArgs e)
        {
            if (_subsystem.Setting.IsAutoPhoneRedirect)
            {//若已设置自动转接电话(处于Check状态)，点击后取消Checked状态
                _subsystem.Setting.IsAutoPhoneRedirect = false;
            }
            else
            {//若未设置自动转接电话(处于UnChecked状态)，点击后跳出对话框，要求输入转接号码
                XferWindow dialog = new XferWindow { Owner = Application.Current.MainWindow };

                Log.Debug("btnAutoPhoneRedirect_Click__XferWindow.ShowDialog BEFORE");

                if (dialog.ShowDialog() == true && dialog.txtDialString.Text.Length != 0)
                {
                    _subsystem.Setting.AutoPhoneRedirectNumber = dialog.txtDialString.Text;
                    _subsystem.Setting.IsAutoPhoneRedirect = true;
                    btnAutoPhoneRedirect.IsChecked = true;

                    _subsystem.Setting.IsAutoAnswer = false;
                }

                Log.Debug("btnAutoPhoneRedirect_Click__XferWindow.ShowDialog AFTER");
            }
        }

        private void btnAutoPhoneRedirect_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton button = sender as ToggleButton;
            if (button != null) button.IsChecked = _subsystem.Setting.IsAutoPhoneRedirect;
        }

        private void btnAutoAnswer_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton button = sender as ToggleButton;
            if (button == null)
                return;

            _subsystem.Setting.IsAutoAnswer = (button.IsChecked == true);

            if (_subsystem.Setting.IsAutoAnswer &&
                 _subsystem.Setting.IsAutoPhoneRedirect)
            {
                _subsystem.Setting.IsAutoPhoneRedirect = false;
                btnAutoPhoneRedirect.IsChecked = false;
            }
        }

        private void btnAutoAnswer_Unchecked(object sender, RoutedEventArgs e)
        {
            ToggleButton button = sender as ToggleButton;
            _subsystem.Setting.IsAutoAnswer = (button.IsChecked == true);
        }

        private void btnMute_Checked(object sender, RoutedEventArgs e)
        {
            App.SIPUA.MuteMicrophone(true);
        }

        private void btnMute_Unchecked(object sender, RoutedEventArgs e)
        {
            App.SIPUA.MuteMicrophone(false);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Group group = tabctrlGroups.SelectedItem as Group;
            if (group == null)
            {
                return;
            }

            Log.Debug("btnSearch_Click__SearchUserWindow.ShowDialog BEFORE");

            SearchUserWindow dialog = new SearchUserWindow(group) {Owner = Application.Current.MainWindow};
            if (dialog.ShowDialog() != true)
            {
                return;
            }

            Log.Debug("btnSearch_Click__SearchUserWindow.ShowDialog AFTER");

            if (dialog.User == null)
            {
                Log.Debug("btnSearch_Click__MessageWindow.ShowDialog BEFORE");

                MessageWindow dlg = new MessageWindow(
                    Properties.Resources.IDS_ERROR_WINDOW_TITLE,
                    Properties.Resources.IDS_SEARCH_NOT_FOUND,
                    MessageWindow.ButtonListType.ButtonOk,
                    MessageWindow.IconType.IconError) {Owner = Application.Current.MainWindow};
                dlg.ShowDialog();

                Log.Debug("btnSearch_Click__MessageWindow.ShowDialog AFTER");

                return;
            }

            SearchCurrentPage(dialog.User);
        }

        #endregion

        #region MixedGroup

        private void btnMixedGroupSlipRight_OnClick(object sender, RoutedEventArgs e)
        {
            ScrollViewer scroll = GetMixedGroupScrollViewer();
            if (scroll != null)
            {
                scroll.PageRight();
            }
        }

        private void btnMixedGroupSlipLeft_OnClick(object sender, RoutedEventArgs e)
        {
            ScrollViewer scroll = GetMixedGroupScrollViewer();
            if (scroll != null)
            {
                scroll.PageLeft();
            }
        }

        private void btnMixedGroupSlipDown_OnClick(object sender, RoutedEventArgs e)
        {
            List<ScrollViewer> scrolls = GetMixedGroupScrollViewers();
            foreach (var scroll in scrolls)
            {
                scroll.PageDown();
            }
        }

        private void btnMixedGroupSlipUp_OnClick(object sender, RoutedEventArgs e)
        {
            List<ScrollViewer> scrolls = GetMixedGroupScrollViewers();
            foreach (var scroll in scrolls)
            {
                scroll.PageUp();
            }
        }

        private void btnMixedGroup_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug("Protocal Stack Log: (SpnvPage)btnMixedGroup_Click, Start");

            Button button = sender as Button;
            if (button == null)
            {
                Log.Debug("Protocal Stack Log: (SpnvPage)btnMixedGroup_Click, button == null, False End");
                return;
            }

            Group mixGroup = (Group)button.DataContext;
            if (mixGroup == null)
            {
                Log.Debug("Protocal Stack Log: (SpnvPage)btnMixedGroup_Click, mixGroup == null, False End");
                return;
            }

            string prefix = _subsystem.PrefixInfo.GetPrefix(DialPrefixType.DialConference);
            if (string.IsNullOrEmpty(prefix))
            {
                Log.Debug("Protocal Stack Log: (SpnvPage)btnMixedGroup_Click, wrong prefix, False End");
                return;
            }
                
            string grpstr = _subsystem.PrefixInfo.GetGroupDialString(mixGroup);
            string mixconfnum = string.Format("{0}{1}", prefix, grpstr);

            //先判断主通道
            var channels = _subsystem.Channels.Channels;
            foreach (var channel in channels)
            {
                if (channel.IsGroupCall && channel.CallDestNum == mixconfnum)
                {
                    //挂断会议
                    App.SIPUA.Hangup(channel.CallId);
                    Log.Debug(String.Format("Protocal Stack Log: (SpnvPage)btnMixedGroup_Click, Conf In Main Channel, Hangup:{0} End", channel.CallId));
                    return;
                }
            }

            //判断左手柄
            var leftHandset = _subsystem.LeftHandset;
            if (leftHandset != null && leftHandset.RemoteNumber == mixconfnum)
            {
                leftHandset.Hangup();
                Log.Debug("Protocal Stack Log: (SpnvPage)btnMixedGroup_Click, Conf In leftHandset, Hangup, End");
                return;
            }

            //判断右手柄
            var rightHandset = _subsystem.RightHandset;
            if (rightHandset != null && rightHandset.RemoteNumber == mixconfnum)
            {
                rightHandset.Hangup();
                Log.Debug("Protocal Stack Log: (SpnvPage)btnMixedGroup_Click, Conf In rightHandset, Hangup, End");
                return;
            }

            //以上皆无，则发起会议
            int callId = -1;
            _subsystem.Channels.MakeCall(_subsystem.AccountId, mixconfnum, ref callId);

            Log.Debug(String.Format("Protocal Stack Log: (SpnvPage)btnMixedGroup_Click, Make Call:{0},{1},{2}", _subsystem.AccountId, mixconfnum, callId));

            Log.Debug("Protocal Stack Log: (SpnvPage)btnMixedGroup_Click, End");
        }

        private void btnMixedGroupUser_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug("Protocal Stack Log: (SpnvPage)btnMixedGroupUser_Click, Start");

            Button button = sender as Button;
            if (button == null)
                return;

            User user = button.DataContext as User;
            if (user == null)
                return;

            //先判定4个主通道
            var channels = _subsystem.Channels.Channels;
            foreach (var channel in channels)
            {
                if (channel.CallDestNum == user.Number)
                {
                    //channel.CallState不能单独作为判定依据

                    //用户来电==>挂起其他通话，接通当前通话
                    //只有IsIncomingCall为True时，IsStateBlinking才为True，所以此处单独使用IsStateBlinking作为判定条件
                    if (channel.IsStateBlinking)
                    {
                        _subsystem.Channels.HoldActiveCall();
                        App.SIPUA.Answer(channel.CallId);
                        Log.Debug(String.Format("Protocal Stack Log: (SpnvPage)btnMixedGroupUser_Click, Main Channel, User Incoming, Hold Active, Answer:{0}, End", channel.CallId));
                        return;
                    }
                    //通话中==>挂断当前通话
                    if (channel.IsStateConnected)
                    {
                        App.SIPUA.Hangup(channel.CallId);
                        Log.Debug(String.Format("Protocal Stack Log: (SpnvPage)btnMixedGroupUser_Click, Main Channel, User Connected, Hangup:{0}, End", channel.CallId));
                        return;
                    }
                    //在呼叫对方==>挂断当前通话
                    if (channel.CallState == sua_inv_state.PJSIP_INV_STATE_EARLY && channel.IsIncomingCall == false)
                    {
                        App.SIPUA.Hangup(channel.CallId);
                        Log.Debug(String.Format("Protocal Stack Log: (SpnvPage)btnMixedGroupUser_Click, Main Channel, User Connecting Not Connected, Hangup:{0}, End", channel.CallId));
                        return;
                    }
                }
            }
            //判定左手柄
            var leftHandset = _subsystem.LeftHandset;
            if (leftHandset != null && leftHandset.RemoteNumber == user.Number)
            {
                //用户来电会在通道中显示，不会在左右手柄
                //通话中==>挂断当前通话
                if (leftHandset.CallState == sua_inv_state.PJSIP_INV_STATE_CONFIRMED)
                {
                    leftHandset.Hangup();
                    Log.Debug("Protocal Stack Log: (SpnvPage)btnMixedGroupUser_Click, leftHandset, User Connected, Hangup, End");
                    return;
                }
                //在呼叫对方==>挂断当前通话
                if (leftHandset.CallState == sua_inv_state.PJSIP_INV_STATE_EARLY)
                {
                    leftHandset.Hangup();
                    Log.Debug("Protocal Stack Log: (SpnvPage)btnMixedGroupUser_Click, leftHandset, User Connecting Not Answered, Hangup, End");
                    return;
                }
            }
            //判断右手柄
            var rightHandset = _subsystem.RightHandset;
            if (rightHandset != null && rightHandset.RemoteNumber == user.Number)
            {
                //用户来电会在通道中显示，不会在左右手柄
                //通话中==>挂断当前通话
                if (rightHandset.CallState == sua_inv_state.PJSIP_INV_STATE_CONFIRMED)
                {
                    rightHandset.Hangup();
                    Log.Debug("Protocal Stack Log: (SpnvPage)btnMixedGroupUser_Click, rightHandset, User Connected, Hangup, End");
                    return;
                }
                //在呼叫对方==>挂断当前通话
                if (rightHandset.CallState == sua_inv_state.PJSIP_INV_STATE_EARLY)
                {
                    rightHandset.Hangup();
                    Log.Debug("Protocal Stack Log: (SpnvPage)btnMixedGroupUser_Click, rightHandset, User Connecting Not Answered, Hangup, End");
                    return;
                }
            }
            //发起呼叫
            int callId = -1;
            _subsystem.Channels.MakeCall(_subsystem.AccountId, user.Number, ref callId);

            Log.Debug(String.Format("Protocal Stack Log: (SpnvPage)btnMixedGroupUser_Click, Make Call:{0},{1},{2}", _subsystem.AccountId, user.Number, callId));

            Log.Debug("Protocal Stack Log: (SpnvPage)btnMixedGroupUser_Click, End");
        }

        #endregion

        #region Channels

        private void btnHandsetLeft_Click(object sender, RoutedEventArgs e)
        {
            App.HandsetMgr.HandsetLeft.IsOffHook = !App.HandsetMgr.HandsetLeft.IsOffHook;
        }

        private void btnHandsetRight_Click(object sender, RoutedEventArgs e)
        {
            App.HandsetMgr.HandsetRight.IsOffHook = !App.HandsetMgr.HandsetRight.IsOffHook;
        }

        private void btnChannel_Click(object sender, RoutedEventArgs e)
        {
            Button channelButton = sender as Button;
            if (channelButton == null)
            {
                return;
            }

            try
            {
                Channel channel = _subsystem.Channels.GetChannel(channelButton);

                ChannelContextWindow dialog = new ChannelContextWindow(_subsystem, channel) { Owner = Application.Current.MainWindow, DataContext = channel };

                Log.Debug("btnChannel_Click__ChannelContextWindow.ShowDialog BEFORE");
                dialog.ShowDialog();
                Log.Debug("btnChannel_Click__ChannelContextWindow.ShowDialog AFTER");
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        #endregion

        #region On...

        private void OnHandsetHookStateChanged(object sender, HandsetOffHookArgs args)
        {
            Log.Debug("_____SpnvPage.xaml.cs___OnHandsetHookStateChanged__start");
            Handset handset = sender as Handset;
            if (handset == null)
            {
                return;
            }

            if (_subsystem.LeftHandsetInfo.Id == args.Id)
            {
                _subsystem.LeftHandsetInfo.IsOffHook = args.IsOffhook;
            }
            else
            {
                _subsystem.RightHandsetInfo.IsOffHook = args.IsOffhook;
            }

            if (handset.IsOffHook)
            {
                Channel channel = _subsystem.Channels.GetInboundRingingChannel();
                if (channel == null)
                {
                    return;
                }
                channel.IsRediect2Handset = true;

                this.Dispatcher.Invoke(DispatcherPriority.Normal,
                        new Action(
                            () => handset.Answer(_subsystem.AccountId, channel.CallId, _subsystem.Setting.AccountUser)
                            ));
            }
            else
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal,
                        new Action(
                            () => handset.Hangup()
                            ));
            }
            Log.Debug("_____SpnvPage.xaml.cs___OnHandsetHookStateChanged__end");
        }

        #endregion

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null)
                return;

            User user = button.DataContext as User;
            if (user == null)
                return;
            
            UserContextWindow dialog = new UserContextWindow(user, _subsystem) {Owner = Application.Current.MainWindow};

            Log.Debug("btnUser_Click__UserContextWindow.ShowDialog BEFORE");
            dialog.ShowDialog();
            Log.Debug("btnUser_Click__UserContextWindow.ShowDialog AFTER");
        }

        private void btnGroup_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null)
                return;

            Group item = (Group)button.DataContext;
            if (item == null)
                return;

            if (lbxGroups.SelectedItem != item)
            {
                lbxGroups.SelectedItem = item;
                tabctrlGroups.SelectedItem = item;
            }
            else
            {
                GroupContextWindow dialog = new GroupContextWindow(item, _subsystem) { Owner = Application.Current.MainWindow };

                Log.Debug("btnGroup_Click__GroupContextWindow.ShowDialog BEFORE");
                dialog.ShowDialog();
                Log.Debug("btnGroup_Click__GroupContextWindow.ShowDialog AFTER");
            }
        }
        
        private ListBox GetCurrentPageUserList()
        {
            try
            {
                ContentPresenter cp = tabctrlGroups.Template.FindName("PART_SelectedContentHost", tabctrlGroups) as ContentPresenter;
                if (cp == null)
                {
                    return null;
                }

                return tabctrlGroups.ContentTemplate.FindName("lbxUsers", cp) as ListBox;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private ScrollViewer GetUserListScoller()
        {
            try
            {
                ContentPresenter cp = tabctrlGroups.Template.FindName("PART_SelectedContentHost", tabctrlGroups) as ContentPresenter;
                if (cp == null)
                {
                    return null;
                }

                ListBox listbox = tabctrlGroups.ContentTemplate.FindName("lbxUsers", cp) as ListBox;
                if (listbox == null)
                {
                    return null;
                }

                Border border = VisualTreeHelper.GetChild(listbox, 0) as Border;
                if (border == null)
                {
                    return null;
                }

                ScrollViewer scroll = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
                return scroll;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private ScrollViewer GetMixedGroupScrollViewer()
        {
            try
            {
                Border border = VisualTreeHelper.GetChild(PatternTwoList, 0) as Border;
                if (border == null)
                {
                    return null;
                }

                ScrollViewer scroll = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
                return scroll;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<ScrollViewer> GetMixedGroupScrollViewers()
        {
            List<ScrollViewer> scrollViewers = new List<ScrollViewer>();
            try
            {
                var items = PatternTwoList.Items;
                foreach (var item in items)
                {
                    var grid = PatternTwoList.ItemContainerGenerator.ContainerFromItem(item);
                    if (grid != null)
                    {
                        ScrollViewer scroll = WpfTreeHelper.FindChild<ScrollViewer>(grid);
                        if (scroll == null)
                        {
                            continue;
                        } 
                        scrollViewers.Add(scroll);
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return scrollViewers;
        }

        private void btnUserListPrevPage_Click(object sender, RoutedEventArgs e)
        {
            ScrollViewer scroll = GetUserListScoller();
            if (scroll != null)
                scroll.PageUp();
        }

        private void btnUserListNextPage_Click(object sender, RoutedEventArgs e)
        {
            ScrollViewer scroll = GetUserListScoller();
            if (scroll != null)
                scroll.PageDown();
        }
        
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            lbxGroups.SelectedIndex = 0;
            tabctrlGroups.SelectedIndex = 0;
        }

        private void lbxGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Group group = lbxGroups.SelectedItem as Group;
            if (group == null)
            {
                return;
            }

            tabctrlGroups.SelectedItem = lbxGroups.SelectedItem;
        }
        
        private static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        private void SearchCurrentPage(User user)
        {
            ListBox listbox = GetCurrentPageUserList();
            if (listbox == null)
            {
                return;
            }

            List<List<object>> lines = listbox.ItemsSource as List<List<object>>;
            if (lines == null)
            {
                return;
            }

            foreach (List<object> line in lines)
            {
                int index = line.IndexOf(user);
                if (index == -1)
                    continue;

                listbox.SelectedItem = line;
                listbox.ScrollIntoView(line);
                break;                
            }
                
            _highlightUser = user;
            _highlightUser.IsHighLight = true;
            _highlightTimer.Start();
        }
        
        private void highlight_stop_tick(object sender, EventArgs e)
        {
            _highlightTimer.Stop();

            if (_highlightUser == null)
                return;

            _highlightUser.IsHighLight = false;
            _highlightUser = null;
        }
    }
}