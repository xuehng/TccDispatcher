using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using renstech.NET.SupernovaDispatcher.Control;
using renstech.NET.SupernovaDispatcher.Model;
using renstech.NET.SupernovaDispatcher.Utils;

namespace renstech.NET.SupernovaDispatcher.Layout
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(LicenseWindow));

        private readonly App _app;
        private readonly GeneralSetting _setting;
        
        public LoginWindow()
        {
            InitializeComponent();

            _app = Application.Current as App;
            if (_app != null) 
                _setting = _app.GeneralSetting;

            string username = _setting.GetLastLoginUser();
            if (_setting.GetLoginUser(username) != null)
            {
                txtUserName.Text = username;
            }

            btnOK.IsEnabled = false;
        }

        private void BtnOkClick(object sender, RoutedEventArgs e)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            if (!_setting.ValidateLoginUser(txtUserName.Text, txtPassword.Password))
            {
                Log.Debug("BtnOkClick__MessageWindow.ShowDialog BEFORE");

                MessageWindow msgbox = new MessageWindow(
                    Properties.Resources.IDS_ERROR_WINDOW_TITLE,
                    Properties.Resources.IDS_LOGIN_ERR,
                    MessageWindow.ButtonListType.ButtonOk,
                    MessageWindow.IconType.IconError) {Owner = this};
                msgbox.ShowDialog();

                Log.Debug("BtnOkClick__MessageWindow.ShowDialog AFTER");

                txtPassword.Password = string.Empty;
                return;
            }

            if (GeneralSetting.IsAdminUser(txtUserName.Text))
                _app.LoginRole.Charater = LoginUserRole.Role.Admin;

            Log.Info(string.Format("TimeSpendCounter__LoginWindow.ValidateLoginUser__{0}", stopWatch.ElapsedMilliseconds));
            stopWatch.Restart();

            _app.LoginUser = txtUserName.Text;            
            RegistryInfo.SaveLastLoginUser(txtUserName.Text);

            stopWatch.Stop();
            Log.Info(string.Format("TimeSpendCounter__LoginWindow.SaveLastLoginUser__{0}", stopWatch.ElapsedMilliseconds));

            StartInitialize(false);
        }

        private void StartInitialize(bool skipFailed)
        {
            btnOK.IsEnabled = false;
            prgressbar.Visibility = Visibility.Visible;

            InitDelegate d = Init;
            d.BeginInvoke(skipFailed, null, null);
        }

        private delegate void InitDelegate(bool skipFailed);
        private void Init(bool skipFailed)
        {
            string subsystemName = null;
            string errMsg = null;

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            foreach (Subsystem system in _app.Systeminfo.Subsystems)
            {
                if (system.InitResult == InitResult.InitOk || 
                    (system.InitResult == InitResult.InitFail && skipFailed))
                {
                    continue;
                }

                if (!system.Initialize(ref errMsg))
                {
                    subsystemName = system.Name;
                    break;
                }
            }

            Dispatcher.Invoke(DispatcherPriority.Normal,
                new Action(
                    delegate {
                        prgressbar.Visibility = Visibility.Hidden;
                    }
                    ));


            stopWatch.Stop();
            Log.Info(string.Format("TimeSpendCounter__LoginWindow.System.Init__{0}", stopWatch.ElapsedMilliseconds));

            Dispatcher.BeginInvoke(new InitCompletedDelegate(InitCompleted), DispatcherPriority.Normal,
                new object[] { subsystemName, errMsg });
        }

        private delegate void InitCompletedDelegate(string subsystemName, string err);

        private void InitCompleted(string subsystemName, string err)
        {
            if (!string.IsNullOrEmpty(subsystemName))
            {
                btnOK.IsEnabled = true;

                string sysErr = string.Format(Properties.Resources.IDS_SUBSYSTEM_INIT_FAIL_FORMAT,
                    subsystemName);
                if (err.Contains("NIDS服务器"))
                {
                    sysErr = "指令与事件通知服务器连接失败，请检查系统设置或网络连接！";
                }

                Log.Debug("InitCompleted__MessageWindow.ShowDialog BEFORE");

                MessageWindow msgbox = new MessageWindow(
                    Properties.Resources.IDS_ERROR_WINDOW_TITLE,
                    sysErr,
                    MessageWindow.ButtonListType.ButtonContCancel,
                    MessageWindow.IconType.IconError) {Owner = this};
                if (msgbox.ShowDialog() != true )
                {
                    return;
                }

                Log.Debug("InitCompleted__MessageWindow.ShowDialog AFTER");

                if (msgbox.WindowResult == MessageWindow.Result.ResultCont)
                {//将此处的逻辑，改为跳出设置窗口
                    StartInitialize(true);
                }
            }
            else
            {
                _app.InitializeSipAccounts();

                DialogResult = true;
                Close();
            }
        }

        private void LayoutRootLoaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtUserName.Text))
                txtUserName.Focus();
            else
                txtPassword.Focus();
        }

        private void TxtChanged(object sender, RoutedEventArgs e)
        {
            if (txtPassword.Password.Length > 0 && txtUserName.Text.Length > 0)
                btnOK.IsEnabled = true;
            else
                btnOK.IsEnabled = false;
        }

        private void BtnKeyboardClick(object sender, RoutedEventArgs e)
        {
            OnScreenKeyboard.StartOsk(this);
        }
    }
}