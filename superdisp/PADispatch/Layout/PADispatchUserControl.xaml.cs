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
using System.Windows.Navigation;
using System.Windows.Shapes;
using renstech.NET.SupernovaDispatcher.Interface;
using renstech.NET.SupernovaDispatcher.Model;

namespace renstech.NET.SupernovaDispatcher.PADispatch
{
    /// <summary>
    /// DavinciDispatchUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class PADispatchUserControl : UserControl, IDispatchPage
    {
        private PASubSystem _system = null;

        public PADispatchUserControl(PASubSystem system)
        {
            this.InitializeComponent();

            _system = system;
        }

        public Subsystem GetSubsystem()
        {
            return _system;
        }

        private void lbxZone_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        }

        private void lbxSection_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            PASection section = lbxSection.SelectedItem as PASection;
            if (section == null)
            {
                return;
            }

            lbxZone.ItemsSource = section.GetZoneLines(_system.Setting.LineZoneCount);
        }

        private void btnSection_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            PASection item = button.DataContext as PASection;

            if (lbxSection.SelectedItem != item)
            {
                lbxSection.SelectedItem = item;
            }
            else
            {

            }
        }

        public bool Initialize()
        {
            lbxSection.ItemsSource = _system.Stations;
            if (lbxSection.Items.Count != 0)
            {
                lbxSection.SelectedIndex = 0;
            }

            btnChannel.DataContext = _system.Channel;

            ZoneLineControl.ButtonHeight = _system.Setting.LineZoneHeight;
            ZoneLineControl.ColCount = _system.Setting.LineZoneCount;
            ZoneLineControl.ButtonStyle = (Style)this.Resources["ZoneButtonBaseStyle"];
            ZoneLineControl.ItemButtonClick = btnZoneButton_Click;
       
            return true;
        }

        public bool Terminate()
        {
            return true;
        }

        private void btnQuit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow dialog = new SettingWindow {Owner = Application.Current.MainWindow};
            dialog.ShowDialog();
        }

        private void btnZoneButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            
            PAZone zone = button.DataContext as PAZone;
            if (zone == null)
                return;

            ZoneContextWindow dialog = new ZoneContextWindow(_system, null, zone);
            dialog.Owner = App.Current.MainWindow;
            dialog.ShowDialog();
        }

        private void btnChannel_Click(object sender, RoutedEventArgs e)
        {
            _system.Channel.Hangup();
        }
    }
}