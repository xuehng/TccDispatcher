using System;
using System.Windows;

namespace renstech.NET.SupernovaDispatcher.IPSDispatch.Layout
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        private readonly IPSSetting _setting;
        private readonly IPSSubsystem _system;

        public OptionsWindow(IPSSubsystem system)
        {
            InitializeComponent();

            _system = system;

            _setting = _system.Setting.Clone() as IPSSetting;
            if (_setting != null)
            {
                _setting.IsModified = false;
                DataContext = _setting;
            }

            var app = Application.Current as App;
            if (app != null && app.AppBkBrush != null)
                Background = app.AppBkBrush;
                
            if (_setting != null)
                EnableLDAPControls(_setting.IsLDAPEnabled);
        }

        private void BtnOkClick(object sender, RoutedEventArgs e)
        {
            if (_setting.IsModified)
            {
                _system.Setting.Copy(_setting);
                _system.Setting.IsModified = true;
            }

            DialogResult = true;
        }

        private void ChkEnableLdapChecked(object sender, RoutedEventArgs e)
        {
            EnableLDAPControls(true);
        }

        private void ChkEnableLdapUnchecked(object sender, RoutedEventArgs e)
        {
            EnableLDAPControls(false);
        }

        private void EnableLDAPControls(bool enable)
        {
            txtLDAPAccount.IsEnabled = enable;
            txtLDAPPassword.IsEnabled = enable;
            txtLDAPAddr.IsEnabled = enable;
            txtLDAPPort.IsEnabled = enable;            
        }
    }
}