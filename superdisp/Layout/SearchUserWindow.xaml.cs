using System.Windows;
using System.Windows.Controls;
using renstech.NET.SupernovaDispatcher.Model;
using renstech.NET.SupernovaDispatcher.Utils;

namespace renstech.NET.SupernovaDispatcher.Layout
{
	/// <summary>
	/// SearchWindow.xaml 的交互逻辑
	/// </summary>
	public partial class SearchUserWindow : Window
	{
        private Group _group;

		public SearchUserWindow(Group group)
		{
			InitializeComponent();

            App app = App.Current as App;
            if (app != null && app.AppBkBrush != null)
                Background = app.AppBkBrush;

            _group = group;

            txtName.DataContext = this;
            txtExtension.DataContext = this;

            btnSearchCurrent.IsEnabled = false;
        }

        public string UserName { get; private set; }
        public string UserNumber { get; private set; }
        public User User { get; private set; }

        private void TxtNameTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtName.Text) || !string.IsNullOrEmpty(txtExtension.Text))
            {
                btnSearchCurrent.IsEnabled = true;
            }
        }

        private void TxtExtensionTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtName.Text) || !string.IsNullOrEmpty(txtExtension.Text))
            {
                btnSearchCurrent.IsEnabled = true;
            }
        }

        private User GetUser()
        {
            if (_group == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(UserName))
            {
                User user = _group.GetUserByName(UserName);
                if (user != null)
                    return user;
            }

            if (!string.IsNullOrEmpty(UserNumber))
            {
                User user = _group.GetUserByNumber(UserNumber);
                if (user != null)
                    return user;
            }

            return null;
        }

        private void BtnSearchCurrentClick(object sender, RoutedEventArgs e)
        {
            UserName = txtName.Text;
            UserNumber = txtExtension.Text;
            User = GetUser();
            DialogResult = true;
        }

        private void BtnKeyboardClick(object sender, RoutedEventArgs e)
        {
            OnScreenKeyboard.StartOsk(this);
        }
	}
}