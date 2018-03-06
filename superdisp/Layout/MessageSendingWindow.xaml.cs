using System.Windows;
using renstech.NET.SupernovaDispatcher.Utils;

namespace renstech.NET.SupernovaDispatcher.Layout
{
    /// <summary>
    /// MessageReplyWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MessageSendingWindow : Window
    {
        public MessageSendingWindow()
        {
            this.InitializeComponent();

            App app = Application.Current as App;
            if (app != null && app.AppBkBrush != null)
                Background = app.AppBkBrush;

            DestName = "";
            DestNumber = "";
        }

        public string DestName { get; set; }
        public string DestNumber { get; set; }
        public string Message { get; private set; }

        private void BtnOkClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DestNumber) || string.IsNullOrEmpty(txtMsgContent.Text) )
                return;

            Message = txtMsgContent.Text;

            DialogResult = true;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            txtReceiverName.Text = string.Format(Properties.Resources.IDS_MESSAGE_DEST_FORMAT,
                            DestName, DestNumber);
            txtMsgContent.Focus();
        }

        private void BtnKeyboardClick(object sender, RoutedEventArgs e)
        {
            OnScreenKeyboard.StartOsk(this);
        }
    }
}