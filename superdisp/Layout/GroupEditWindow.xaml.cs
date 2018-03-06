using System.Windows;
using renstech.NET.SupernovaDispatcher.Utils;

namespace renstech.NET.SupernovaDispatcher.Layout
{
	/// <summary>
	/// GroupCreationWindow.xaml 的交互逻辑
	/// </summary>
	public partial class GroupEditWindow : Window
	{
        public GroupEditWindow()
		{
			InitializeComponent();

            App app = Application.Current as App;
            if (app != null && app.AppBkBrush != null)
                Background = app.AppBkBrush;

            txtGroupName.DataContext = this;
            txtGroupName.Focus();
		}

        public string GroupName { get; set; }

        private void BtnOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void BtnKeyboardClick(object sender, RoutedEventArgs e)
        {
            OnScreenKeyboard.StartOsk(this);
        }
	}
}