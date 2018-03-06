using System.Windows;
using renstech.NET.SupernovaDispatcher.Model;

namespace renstech.NET.SupernovaDispatcher.Layout
{
	/// <summary>
	/// AboutWindow.xaml �Ľ����߼�
	/// </summary>
	public partial class AboutWindow : Window
	{
		public AboutWindow()
		{
			this.InitializeComponent();

            App app = App.Current as App;
            if (app.AppBkBrush != null)
                Background = app.AppBkBrush;

            txtProduct.Content = LicenseInfo.AssemblyProduct;
            txtVersion.Content = LicenseInfo.AssemblyVersion;
            txtCopyRight.Content = LicenseInfo.AssemblyCopyright;
            //txtExpire.Content = string.Format("��Ч���ڣ�{0}��", app.Licenseinfo.ExpireDate.ToLongDateString());
            txtExpire.Content = string.Format("��Ч���ڣ�{0}��", "");

        }

        private void btnUpdateLicense_Click(object sender, RoutedEventArgs e)
        {
            LicenseWindow dialog = new LicenseWindow();
            dialog.Owner = this;
            dialog.ShowDialog();
        }
	}
}