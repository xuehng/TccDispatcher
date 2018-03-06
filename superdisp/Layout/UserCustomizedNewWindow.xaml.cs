using System.Windows;
using renstech.NET.SupernovaDispatcher.Utils;

namespace renstech.NET.SupernovaDispatcher.Layout
{
	/// <summary>
	/// UserCustomizedNewWindow.xaml 的交互逻辑
	/// </summary>
	public partial class UserCustomizedNewWindow : Window
	{
		public UserCustomizedNewWindow()
		{
			InitializeComponent();

            App app = Application.Current as App;
            if (app != null && app.AppBkBrush != null)
                Background = app.AppBkBrush;

		    DataContext = this;
		}

        public string UserName { get; set; }
        public string ExtensionPrevious { get; set; }
        public string ExtensionCurrent { get; set; }
        public bool IsExtensionChanged { get; set; }

	    private void BtnOkClick(object sender, RoutedEventArgs e)
        {
            //任何为空，则修改无效，返回false
            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(ExtensionCurrent))
            {
                DialogResult = false;
                return;
            }

            //如果修改了号码，则设置标记
	        if (ExtensionPrevious != ExtensionCurrent)
	            IsExtensionChanged = true;

            DialogResult = true;
        }

        private void BtnKeyboardClick(object sender, RoutedEventArgs e)
        {
            OnScreenKeyboard.StartOsk(this);
        }
	}
}