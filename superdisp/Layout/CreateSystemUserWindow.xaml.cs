using System.Windows;
using renstech.NET.SupernovaDispatcher.Utils;

namespace renstech.NET.SupernovaDispatcher.Layout
{
	/// <summary>
	/// CreateSystemUserWindow.xaml 的交互逻辑
	/// </summary>
	public partial class CreateSystemUserWindow : Window
	{
		public CreateSystemUserWindow()
		{
			this.InitializeComponent();

            txtUserName.Focus();
		}

        public string UserName { get; set; }
        public string Password { get; set; }

        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            if ( !string.IsNullOrEmpty(txtUserName.Text) &&
                 !string.IsNullOrEmpty(pswFirst.Password) &&
                 pswFirst.Password == pswSecond.Password)
            {
                UserName = txtUserName.Text;
                Password = pswFirst.Password;
                DialogResult = true;
            }
        }

        private void BtnKeyboardClick(object sender, RoutedEventArgs e)
        {
            OnScreenKeyboard.StartOsk(this);
        }
	}
}