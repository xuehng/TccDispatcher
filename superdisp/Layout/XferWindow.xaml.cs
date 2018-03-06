using System.Windows;
using System.Windows.Controls;

namespace renstech.NET.SupernovaDispatcher.Layout
{
    /// <summary>
    /// XferWindow.xaml 的交互逻辑
    /// </summary>
    public partial class XferWindow : Window
    {
        public XferWindow()
        {
            this.InitializeComponent();

            App app = App.Current as App;
            if (app.AppBkBrush != null)
                Background = app.AppBkBrush;
        }

        private void btnNum_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            txtDialString.Text += button.Content.ToString();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtDialString.Text = "";
        }

        private void btnBackspace_Click(object sender, RoutedEventArgs e)
        {
            if (txtDialString.Text.Length <= 0)
                return;

            if (txtDialString.Text.Length == 1)
                txtDialString.Text = "";
            else
                txtDialString.Text = txtDialString.Text.Remove(txtDialString.Text.Length - 1);
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}