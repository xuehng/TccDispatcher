using System;
using System.Windows;
using System.Windows.Controls;
using renstech.NET.SupernovaDispatcher.Model;

namespace renstech.NET.SupernovaDispatcher.Layout
{
    /// <summary>
    /// DialPad.xaml 的交互逻辑
    /// </summary>
    public partial class DialPad : Window
    {
        private readonly SpnvSubSystem _subsystem;
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(DialPad));

        public DialPad(SpnvSubSystem system)
        {
            InitializeComponent();

            _subsystem = system;
        }

        private void btnNum_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug("Protocal Stack Log: (DialPad)btnNum_Click, Start");

            Button button = (Button)sender;
            txtDialString.Text += button.Content.ToString();

            Channel channel = _subsystem.Channels.GetActiveChannel();
            if (channel != null)
            {
                App.SIPUA.DialDTMF(channel.CallId, button.Content.ToString());

                Log.Debug(String.Format("Protocal Stack Log: (DialPad)btnNum_Click, DialDTMF:{0},{1}", channel.CallId, button.Content));
            }

            Log.Debug("Protocal Stack Log: (DialPad)btnNum_Click, End");
        }

        private void btnBackspace_Click(object sender, RoutedEventArgs e)
        {
            if (txtDialString.Text.Length <= 0)
                return;

            txtDialString.Text = txtDialString.Text.Length == 1 ? string.Empty : txtDialString.Text.Remove(txtDialString.Text.Length - 1);

        }

        private void btnDial_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug("Protocal Stack Log: (DialPad)btnDial_Click, Start");

            if (txtDialString.Text.Length == 0)
            {
                return;
            }

            int callId = -1;
            _subsystem.Channels.MakeCall(_subsystem.AccountId, txtDialString.Text, ref callId);

            Log.Debug(String.Format("Protocal Stack Log: (DialPad)btnDial_Click, Make Call:{0},{1},{2}", _subsystem.AccountId, txtDialString.Text, callId));

            Log.Debug("Protocal Stack Log: (DialPad)btnDial_Click, Start");
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtDialString.Text = string.Empty;
        }
    }
}
