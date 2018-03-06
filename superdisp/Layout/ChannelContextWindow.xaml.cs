using System;
using System.Windows;
using System.Windows.Interop;
using renstech.NET.SIPUA;
using renstech.NET.SupernovaDispatcher.Model;
using renstech.NET.SupernovaDispatcher.Model.Handset;

namespace renstech.NET.SupernovaDispatcher.Layout
{
    /// <summary>
    /// ChannelContextWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ChannelContextWindow : Window
    {
        private SpnvSubSystem _sysytem = null;
        private Channel _channel = null;
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ChannelContextWindow));

        public ChannelContextWindow(SpnvSubSystem system, Channel channel)
        {
            this.InitializeComponent();

            App app = App.Current as App;
            if (app.AppBkBrush != null)
                Background = app.AppBkBrush;

            _sysytem = system;
            _channel = channel;

            if (channel.IsChannelBusy)
            {
                this.txtChannelName.Text = (channel.CallPartyDisplayName != null && channel.CallPartyDisplayName.Length != 0) ? channel.CallPartyDisplayName : channel.Name;
                this.txtChannelState.Text = channel.CallPartyDisplayNumber;
            }
            else
            {
                this.txtChannelName.Text = channel.Name;
            }

            DialPrefixType type = _sysytem.PrefixInfo.GetPrefixType(channel.CallDestNum);
            if (type != DialPrefixType.DialNoprefix)
            {
                btnRedirectToHandset.IsEnabled = false;
            }
        }

        private void btnCallAnswer_Click(object sender, RoutedEventArgs e)
        {
            _sysytem.Channels.HoldActiveCall();

            App.SIPUA.Answer(_channel.CallId);
            
            DialogResult = true;
        }

        private void btnHold_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug("Protocal Stack Log: (ChannelContextWindow)btnHold_Click, Start");

            if (_channel.IsStateHold)
            {
                _sysytem.Channels.HoldActiveCall();

                App.SIPUA.Unhold(_channel.CallId);

                Log.Debug(String.Format("Protocal Stack Log: (ChannelContextWindow)btnHangup_Click, Unhold:{0}", _channel.CallId));
            }
            else
            {
                App.SIPUA.Hold(_channel.CallId);

                Log.Debug(String.Format("Protocal Stack Log: (ChannelContextWindow)btnHangup_Click, Hold:{0}", _channel.CallId));
            }

            Log.Debug("Protocal Stack Log: (ChannelContextWindow)btnHold_Click, End");

            DialogResult = true;
        }

        private void btnHangup_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug("Protocal Stack Log: (ChannelContextWindow)btnHangup_Click, Start");

            App.SIPUA.Hangup(_channel.CallId);

            Log.Debug(String.Format("Protocal Stack Log: (ChannelContextWindow)btnHangup_Click, Hangup:{0}", _channel.CallId));

            Log.Debug("Protocal Stack Log: (ChannelContextWindow)btnHangup_Click, End");

            DialogResult = true;
        }

        private void btnRedirect_Click(object sender, RoutedEventArgs e)
        {
            XferWindow dialog = new XferWindow();
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                App.SIPUA.Xfer(_sysytem.AccountId, _channel.CallId, dialog.txtDialString.Text);
                DialogResult = true;
            }
        }

        private void btnMessage_Click(object sender, RoutedEventArgs e)
        {
            MessageSendingWindow dialog = new MessageSendingWindow();
            dialog.Owner = this;
            dialog.DestName = _channel.CallPartyDisplayName;
            dialog.DestNumber = _channel.CallPartyDisplayNumber;

            if (dialog.ShowDialog() == true)
            {
                App.SIPUA.SendMessage(_channel.CallId, dialog.Message);
                DialogResult = true;
            }
        }

        private void btnShowVideoWin_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;

            VideoInWindow dialog = new VideoInWindow(_channel.CallId);
            dialog.Owner = App.Current.MainWindow;
            dialog.ShowDialog();
            return;

            sua_vid_win_info info = new sua_vid_win_info();
            App.SIPUA.GetVidWindowInfo(_channel.CallId, ref info);

            if (info.show == 1)
            {
                App.SIPUA.ShowVideoWindow(_channel.CallId, false);
            }
            else
            {
                App.SIPUA.ShowVideoWindow(_channel.CallId, true);

                MainWindow main = App.Current.MainWindow as MainWindow;
                if (main != null)
                {
                    int top = ((int)main.Top) + ((int)main.Height)/ 2 - info.height / 2;
                    int left = ((int)main.Left) + ((int)main.Width)/ 2 - info.width / 2;

                    HwndSource source = (HwndSource)HwndSource.FromVisual(main);
                    IntPtr hWnd = source.Handle;
                    //App.SIPUA.SetVideoWindowPos(_channel.CallId, hWnd, left, top);
                }
            }
        }

        private void btnRedirectToHandset_Click(object sender, RoutedEventArgs e)
        {   
            Handset handset = _sysytem.GetIdleHandset();
            if (handset == null)
            {
                return;
            }

            handset.TransferAnswer(_sysytem.AccountId, _channel.CallId, _sysytem.Setting.AccountUser);
        }
    }
}