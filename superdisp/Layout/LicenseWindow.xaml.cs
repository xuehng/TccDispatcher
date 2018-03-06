using System.Windows;
using renstech.NET.SupernovaDispatcher.Control;

namespace renstech.NET.SupernovaDispatcher.Layout
{
	/// <summary>
	/// LicenseWindow.xaml 的交互逻辑
	/// </summary>
	public partial class LicenseWindow : Window
	{
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(LicenseWindow));

		public LicenseWindow()
		{
			InitializeComponent();			
		}

        private void btnSerial_Click(object sender, RoutedEventArgs e)
        {
            SerialWindow dialog = new SerialWindow {Owner = this};
            dialog.ShowDialog();
        }

        private void btnActivate_Click(object sender, RoutedEventArgs e)
        {
            App app = App.Current as App;
            if (app == null) return;

            if (string.IsNullOrEmpty(txtActivationCode.Text))
                return;
            try
            {
                if (!app.Licenseinfo.Parse(txtActivationCode.Text))
                    return;
            }
            catch (System.Exception ex)
            {
                Log.Debug("btnActivate_Click__MessageWindow.ShowDialog BEFORE");

                MessageWindow dialog = new MessageWindow(
                    Properties.Resources.IDS_ERROR_WINDOW_TITLE,
                    Properties.Resources.IDS_SOFTWARE_ACTIVATE_FAILURE,
                    MessageWindow.ButtonListType.ButtonOk,
                    MessageWindow.IconType.IconError) {Owner = this};
                dialog.ShowDialog();

                Log.Debug("btnActivate_Click__MessageWindow.ShowDialog AFTER");

                return;
            }

            if (!app.Licenseinfo.SaveLicense())
                return;
            
            Log.Debug("btnActivate_Click__MessageWindow.ShowDialog BEFORE");

            MessageWindow dialog2 = new MessageWindow(
                Properties.Resources.IDS_CONFIRM_WINDOW_TITLE,
                Properties.Resources.IDS_SOFTWARE_ACTIVATE,
                MessageWindow.ButtonListType.ButtonOk,
                MessageWindow.IconType.IconWarn) { Owner = this };
            dialog2.ShowDialog();

            Log.Debug("btnActivate_Click__MessageWindow.ShowDialog AFTER");

            DialogResult = true;
        }
	}
}