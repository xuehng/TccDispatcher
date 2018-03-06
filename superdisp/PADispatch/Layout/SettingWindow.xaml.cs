using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace renstech.NET.SupernovaDispatcher.PADispatch
{
	/// <summary>
	/// SettingWindow.xaml 的交互逻辑
	/// </summary>
	public partial class SettingWindow : Window
	{
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(SettingWindow));

		public SettingWindow()
		{
			this.InitializeComponent();

            App app = App.Current as App;
            if (app.AppBkBrush != null)
                Background = app.AppBkBrush;
        }

        private void btnOptions_Click(object sender, RoutedEventArgs e)
        {
            OptionsWindow dialog = new OptionsWindow();
            dialog.Owner = this;

            Log.Debug("btnOptions_Click__OptionsWindow.ShowDialog BEFORE");

            if (dialog.ShowDialog() == true)
            {
                App.ConfigManager.SaveSettings();
            }

            Log.Debug("btnOptions_Click__OptionsWindow.ShowDialog AFTER");
        }
	}
}