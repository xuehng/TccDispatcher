using System.Windows;
using renstech.NET.SupernovaDispatcher.Model;
using renstech.NET.SupernovaDispatcher.Utils;
using MessageWindow = renstech.NET.SupernovaDispatcher.Control.MessageWindow;

namespace renstech.NET.SupernovaDispatcher.Layout
{
    /// <summary>
    /// MessageInboxWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MessageInboxWindow : Window
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(MessageInboxWindow));

        private SpnvSubSystem _subsystem = null;

        public MessageInboxWindow(SpnvSubSystem system)
        {
            this.InitializeComponent();

            App app = App.Current as App;
            if (app.AppBkBrush != null)
                Background = app.AppBkBrush;

            _subsystem = system;
            lbxMessage.ItemsSource = system.TextMessages.Messages;
        }

        private void btnDeleteMsg_Click(object sender, RoutedEventArgs e)
        {
            if (lbxMessage.SelectedIndex == -1)
            {
                return;
            }

            Message msg = lbxMessage.SelectedItem as Message;
            if (msg == null)
            {
                return;
            }

            _subsystem.TextMessages.DeleteMessage(msg);
        }

        private void btnDeleteAllMsg_Click(object sender, RoutedEventArgs e)
        {
            if (lbxMessage.Items.Count == 0)
            {
                return;
            }

            Log.Debug("btnDeleteAllMsg_Click__MessageWindow.ShowDialog BEFORE");

            MessageWindow dialog = new MessageWindow(
                Properties.Resources.IDS_CONFIRM_WINDOW_TITLE,
                Properties.Resources.IDS_MESSAGE_DELETE_ALL,
                MessageWindow.ButtonListType.ButtonOkCancel) {Owner = this};
            if (dialog.ShowDialog() == true)
            {
                _subsystem.TextMessages.DeleteAll();
            }

            Log.Debug("btnDeleteAllMsg_Click__MessageWindow.ShowDialog AFTER");
        }

        private void btnReply_Click(object sender, RoutedEventArgs e)
        {
            if (lbxMessage.SelectedIndex == -1)
            {
                return;
            }

            Message msg = lbxMessage.SelectedItem as Message;
            if (msg == null)
            {
                return;
            }

            MessageSendingWindow dialog = new MessageSendingWindow {DestNumber = msg.From, Owner = this};
            if (dialog.ShowDialog() != true)
            {
                return;
            }
            App.SIPUA.SendMessage(_subsystem.AccountId, msg.From, dialog.Message);
        }
    }
}