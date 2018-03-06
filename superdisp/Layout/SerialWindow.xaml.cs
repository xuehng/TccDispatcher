using System.Windows;
using renstech.NET.SupernovaDispatcher.Model;

namespace renstech.NET.SupernovaDispatcher.Layout
{
	/// <summary>
	/// SerialWindow.xaml 的交互逻辑
	/// </summary>
	public partial class SerialWindow : Window
	{
		public SerialWindow()
		{
			InitializeComponent();

            txtSerial.Text = LicenseInfo.GenerateSerial();
		}

        private void btnClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSerial.Text))
                Clipboard.SetDataObject(txtSerial.Text);
        }
	}
}