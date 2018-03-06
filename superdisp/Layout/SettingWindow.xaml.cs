using System.Windows;
using renstech.NET.SupernovaDispatcher.Model;

namespace renstech.NET.SupernovaDispatcher.Layout
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : Window
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(SettingWindow));

        readonly SpnvSubSystem _system;

        public SettingWindow(SpnvSubSystem system)
        {
            InitializeComponent();

            App app = Application.Current as App;
            if (app != null && app.AppBkBrush != null)
                Background = app.AppBkBrush;

            _system = system;

            btnStatus.Visibility = Visibility.Hidden;
            btnCallHistory.DataContext = _system.CallHisory;
        }

        private void BtnMessageClick(object sender, RoutedEventArgs e)
        {
            MessageInboxWindow dialog = new MessageInboxWindow(_system) { Owner = Application.Current.MainWindow };

            Log.Debug("BtnMessageClick__MessageInboxWindow.ShowDialog BEFORE");
            dialog.ShowDialog();
            Log.Debug("BtnMessageClick__MessageInboxWindow.ShowDialog AFTER");

            _system.TextMessages.UnreadMsg = 0;
        }

        private void BtnOptionsClick(object sender, RoutedEventArgs e)
        {
            OptionsWindow dialog = new OptionsWindow(_system) {Owner = this};

            Log.Debug("BtnOptionsClick__OptionsWindow.ShowDialog BEFORE");

            if (dialog.ShowDialog() == true)
            {
                App.ConfigManager.SaveSettings();
            }

            Log.Debug("BtnOptionsClick__OptionsWindow.ShowDialog AFTER");
        }

        private void BtnGroupSettingClick(object sender, RoutedEventArgs e)
        {
            GroupSettingWindow dialog = new GroupSettingWindow(_system) {Owner = this};

            Log.Debug("BtnGroupSettingClick__GroupSettingWindow.ShowDialog BEFORE");
            dialog.ShowDialog();
            Log.Debug("BtnGroupSettingClick__GroupSettingWindow.ShowDialog AFTER");
        }

        private void BtnCallHistoryClick(object sender, RoutedEventArgs e)
        {
            CallHistoryWindow dialog = new CallHistoryWindow(_system) { Owner = Application.Current.MainWindow };
            Log.Debug("BtnCallHistoryClick__CallHistoryWindow.ShowDialog BEFORE");
            dialog.ShowDialog();
            Log.Debug("BtnCallHistoryClick__CallHistoryWindow.ShowDialog AFTER");
        }

        private void BtnAboutClick(object sender, RoutedEventArgs e)
        {
            AboutWindow dialog = new AboutWindow {Owner = this};

            Log.Debug("BtnAboutClick__AboutWindow.ShowDialog BEFORE"); 
            dialog.ShowDialog();
            Log.Debug("BtnAboutClick__AboutWindow.ShowDialog AFTER"); 
        }

        private void BtnUserCustomizedClick(object sender, RoutedEventArgs e)
        {
            var dialog = new UserCustomizeWindow(_system){Owner = this};

            Log.Debug("BtnUserCustomizedClick__UserCustomizeWindow.ShowDialog BEFORE"); 
            dialog.ShowDialog();
            Log.Debug("BtnUserCustomizedClick__UserCustomizeWindow.ShowDialog AFTER"); 
        }
    }
}