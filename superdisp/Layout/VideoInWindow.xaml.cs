using System;
using System.Windows;
using System.Windows.Threading;
using renstech.NET.SIPUA;

namespace renstech.NET.SupernovaDispatcher.Layout
{
	/// <summary>
	/// VideoInWindow.xaml 的交互逻辑
	/// </summary>
	public partial class VideoInWindow : Window
	{
        private int _callId = -1;
        private bool _reposVidWin = false;

		public VideoInWindow(int callId)
		{
			this.InitializeComponent();

            _callId = callId;

            App.SIPUA.ShowVideoWindow(_callId, true);
            App.SIPUA.CallStateInfo += OnCallStateChanged;
        }

        private void ResizeVidWindow()
        {
            sua_vid_win_info info = new sua_vid_win_info();
            App.SIPUA.GetVidWindowInfo(_callId, ref info);

            Point locationFromWindow = vidPosBorder.TranslatePoint(new Point(0, 0), this);
            Point screenPt = vidPosBorder.PointToScreen(locationFromWindow);
            int parentTop = (int)screenPt.Y;
            int parentLeft = (int)screenPt.X;

            int left = (int)(parentLeft + (vidPosBorder.ActualWidth - info.width) / 2);
            int top = (int)(parentTop + (vidPosBorder.ActualHeight - info.height) / 2);

            App.SIPUA.SetVideoWindowPos(_callId, parentLeft - 8, parentTop - 8, (int)(vidPosBorder.ActualWidth-2), (int)(vidPosBorder.ActualHeight-2));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ResizeVidWindow();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            App.SIPUA.ShowVideoWindow(_callId, false);
            DialogResult = false;
        }

        public void OnCallStateChanged(object sender, CallStateArgs e)
        {
            if (e.CallId != _callId)
            {
                return;
            }

            if (e.STATE == sua_inv_state.PJSIP_INV_STATE_DISCONNECTED)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal,
                    new Action(
                        delegate()
                        {
                            this.Close();
                        }
                        ));
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_reposVidWin)
            {
                _reposVidWin = false;
            }
        }
	}
}