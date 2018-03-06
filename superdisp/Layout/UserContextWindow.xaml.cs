using System.Windows;
using System.Windows.Input;
using renstech.NET.SupernovaDispatcher.Model;

namespace renstech.NET.SupernovaDispatcher.Layout
{
    /// <summary>
    /// UserContextWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UserContextWindow : Window
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(UserContextWindow));
        readonly SpnvSubSystem _system;
        readonly User _user;

        public UserContextWindow(User user, SpnvSubSystem system)
        {
            InitializeComponent();

            App app = Application.Current as App;
            if (app != null && app.AppBkBrush != null)
                Background = app.AppBkBrush;

            _system = system;
            _user = user;

            DataContext = user;

            if (user.IsLocalUser)
            {
                btnNormalCall.IsEnabled = false;
                btnIntercom.IsEnabled = false;
                btnPickup.IsEnabled = false;
                btnThreeway.IsEnabled = false;
                btnIntercept.IsEnabled = false;
                btnEavesdrop.IsEnabled = false;
                btnJoinConf.IsEnabled = false;
                btnKickConf.IsEnabled = false;
                btnMessage.IsEnabled = false;
            }
        }

        private void btnNormalCall_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug("Protocal Stack Log: (UserContext)btnNormalCall_Click, Start");

            int callId = -1;
            _system.Channels.MakeCall(_system.AccountId, _user.Number, ref callId);

            Log.Debug(string.Format("Protocal Stack Log: (UserContext)btnNormalCall_Click, MakeCall:{0},{1},{2}", _system.AccountId, _user.Number, callId));

            Log.Debug("Protocal Stack Log: (UserContext)btnNormalCall_Click, End");

            DialogResult = true;
        }

        private void btnNormalCall_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
        }

        private void btnMessage_Click(object sender, RoutedEventArgs e)
        {
            MessageSendingWindow dialog = new MessageSendingWindow
                                              {Owner = this, DestName = _user.Name, DestNumber = _user.Number};

            if (dialog.ShowDialog() != true)
            {
                return;
            }
            App.SIPUA.SendMessage(_system.AccountId, _user.Number, dialog.Message);
        }

        private void btnIntercom_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug("Protocal Stack Log: (UserContext)btnIntercom_Click, Start");

            string prefix = _system.PrefixInfo.GetPrefix(DialPrefixType.DialIntercom);
            if (string.IsNullOrEmpty(prefix))
                return;

            string dest = string.Format("{0}{1}", prefix, _user.Number);

            int callId = -1;
            _system.Channels.MakeCall(_system.AccountId, dest, ref callId);

            Log.Debug(string.Format("Protocal Stack Log: (UserContext)btnIntercom_Click, MakeCall:{0},{1},{2}", _system.AccountId, dest, callId));

            Log.Debug("Protocal Stack Log: (UserContext)btnIntercom_Click, End");

            DialogResult = true;
        }

        private void btnPickup_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug("Protocal Stack Log: (UserContext)btnPickup_Click, Start");

            string prefix = _system.PrefixInfo.GetPrefix(DialPrefixType.DialPickup);
            if (string.IsNullOrEmpty(prefix))
                return;

            string dest = string.Format("{0}{1}", prefix, _user.Number);

            int callId = -1;
            _system.Channels.MakeCall(_system.AccountId, dest, ref callId);

            Log.Debug(string.Format("Protocal Stack Log: (UserContext)btnPickup_Click, MakeCall:{0},{1},{2}", _system.AccountId, dest, callId));

            Log.Debug("Protocal Stack Log: (UserContext)btnPickup_Click, End");

            DialogResult = true;
        }

        private void btnThreeway_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug("Protocal Stack Log: (UserContext)btnThreeway_Click, Start");

            string prefix = _system.PrefixInfo.GetPrefix(DialPrefixType.DialThreeway);
            if (string.IsNullOrEmpty(prefix))
                return;

            string dest = string.Format("{0}{1}", prefix, _user.Number);

            int callId = -1;
            _system.Channels.MakeCall(_system.AccountId, dest, ref callId);

            Log.Debug(string.Format("Protocal Stack Log: (UserContext)btnThreeway_Click, MakeCall:{0},{1},{2}", _system.AccountId, dest, callId));

            Log.Debug("Protocal Stack Log: (UserContext)btnThreeway_Click, End");

            DialogResult = true;
        }

        private void btnIntercept_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug("Protocal Stack Log: (UserContext)btnIntercept_Click, Start");

            string prefix = _system.PrefixInfo.GetPrefix(DialPrefixType.DialIntercept);
            if (string.IsNullOrEmpty(prefix))
                return;

            string dest = string.Format("{0}{1}", prefix, _user.Number);

            int callId = -1;
            _system.Channels.MakeCall(_system.AccountId, dest, ref callId);

            Log.Debug(string.Format("Protocal Stack Log: (UserContext)btnIntercept_Click, MakeCall:{0},{1},{2}", _system.AccountId, dest, callId));

            Log.Debug("Protocal Stack Log: (UserContext)btnIntercept_Click, End");

            DialogResult = true;
        }

        private void btnEavesdrop_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug("Protocal Stack Log: (UserContext)btnEavesdrop_Click, Start");

            string prefix = _system.PrefixInfo.GetPrefix(DialPrefixType.DialEavesdrop);
            if (string.IsNullOrEmpty(prefix))
                return;

            string dest = string.Format("{0}{1}", prefix, _user.Number);

            int callId = -1;
            _system.Channels.MakeCall(_system.AccountId, dest, ref callId);

            Log.Debug(string.Format("Protocal Stack Log: Current Function: (UserContext)btnEavesdrop_Click, MakeCall:{0},{1},{2}", _system.AccountId, dest, callId));

            Log.Debug("Protocal Stack Log: (UserContext)btnEavesdrop_Click, End");

            DialogResult = true;
        }

        private void btnJoinConf_Click(object sender, RoutedEventArgs e)
        {
            _system.Channels.AddConfMember(_user.Number);
            DialogResult = true;
        }

        private void btnKickConf_Click(object sender, RoutedEventArgs e)
        {
            _system.Channels.DelConMember(_user.Number);
            DialogResult = true;
        }
    }
}