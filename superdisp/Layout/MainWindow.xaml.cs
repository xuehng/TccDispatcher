#region

using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using log4net;
using renstech.NET.SupernovaDispatcher.Control;
using renstech.NET.SupernovaDispatcher.Interface;
using Application = System.Windows.Application;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using TabControl = System.Windows.Controls.TabControl;

#endregion

namespace renstech.NET.SupernovaDispatcher.Layout
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (MainWindow));

        private readonly List<IDispatchPage> _pagelist = new List<IDispatchPage>();

        public MainWindow()
        {
            InitializeComponent();

            var app = Application.Current as App;
            if (app != null && app.AppBkBrush != null)
                Background = app.AppBkBrush;
        }

        public void Initialize()
        {
            var app = Application.Current as App;
            foreach (var system in app.Systeminfo.Subsystems)
            {
                _pagelist.Add(system.GetDispatchPage());
            }

            foreach (var page in _pagelist)
            {
                var item = new TabItem
                {
                    Content = page,
                    Width = 200,
                    Height = 55,
                    Margin = new Thickness(0, 5, -2, -1),
                    FontSize = 16,
                    FontFamily = new FontFamily("Î¢ÈíÑÅºÚ"),
                    VerticalContentAlignment = VerticalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    HeaderTemplate = FindResource("SubSystemDataTemplate") as DataTemplate,
                    Header = page.GetSubsystem().Name
                };

                tabSubsystem.Items.Add(item);

                page.Initialize();
            }
            var rect = Screen.PrimaryScreen.Bounds;
            Height = rect.Height;
            Width = rect.Width;
        }

        private void tabSubsystem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tab = sender as TabControl;
            if (tab == null)
                return;

            var item = tab.SelectedItem as TabItem;
            if (item == null)
            {
                return;
            }

            var page = item.Content as IDispatchPage;
            if (page == null)
            {
                return;
            }

            var system = page.GetSubsystem();

            var account = App.SIPUA.GetAccount(system.AccountId);
            if (account == null)
            {
                accountPanel.Visibility = Visibility.Hidden;
            }
            else
            {
                if (!accountPanel.IsVisible)
                    accountPanel.Visibility = Visibility.Visible;

                accountPanel.DataContext = account;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (IsVisible == false)
            {
                return;
            }

            Log.Debug("Window_Closing__MessageWindow.ShowDialog BEFORE");

            var dialog = new MessageWindow(
                Properties.Resources.IDS_CONFIRM_WINDOW_TITLE,
                Properties.Resources.IDS_CONFIRM_QUIT,
                MessageWindow.ButtonListType.ButtonOkCancel) {Owner = Application.Current.MainWindow};
            if (dialog.ShowDialog() != true)
            {
                e.Cancel = true;
            }

            Log.Debug("Window_Closing__MessageWindow.ShowDialog AFTER");
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                var hwndTarget = hwndSource.CompositionTarget;
                if (hwndTarget != null)
                    hwndTarget.RenderMode = RenderMode.SoftwareOnly;    
            }
        }
    }
}