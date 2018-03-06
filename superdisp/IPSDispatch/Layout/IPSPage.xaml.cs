using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using renstech.NET.SupernovaDispatcher.Interface;
using renstech.NET.SupernovaDispatcher.Model;

namespace renstech.NET.SupernovaDispatcher.IPSDispatch.Layout
{
    /// <summary>
    /// Interaction logic for WiPosPage.xaml
    /// </summary>
    public partial class WiPosPage : UserControl, IDispatchPage
    {
        private readonly Dictionary<ulong, Polygon> _dictCamState;
        private readonly Dictionary<ulong, Image> _dictLocPin;
        private readonly BitmapImage _greenMaker;
        private readonly BitmapImage _highlightMaker;
        private readonly IPSSubsystem _system;
        private Map _currentMap;
        private ulong _highlightDevice;

        public WiPosPage(IPSSubsystem system)
        {
            InitializeComponent();

            _system = system;
            _dictLocPin = new Dictionary<ulong, Image>();
            _dictCamState = new Dictionary<ulong, Polygon>();

            _greenMaker = new BitmapImage();
            _greenMaker.BeginInit();
            _greenMaker.UriSource = new Uri(@"/SupernovaDispatcher;component/Resources/Map-Marker-Ball-Chartreuse.png",
                                            UriKind.Relative);
            _greenMaker.DecodePixelHeight = 48;
            _greenMaker.DecodePixelWidth = 48;
            _greenMaker.EndInit();

            _highlightMaker = new BitmapImage();
            _highlightMaker.BeginInit();
            _highlightMaker.UriSource = new Uri(@"/SupernovaDispatcher;component/Resources/Marker-Flag--Left-Pink.png",
                                                UriKind.Relative);
            _highlightMaker.DecodePixelHeight = 48;
            _highlightMaker.DecodePixelWidth = 48;
            _highlightMaker.EndInit();
        }

        #region IDispatchPage Members

        public Subsystem GetSubsystem()
        {
            return _system;
        }

        public bool Initialize()
        {
            mapsList.ItemsSource = _system.Maps;

            return true;
        }

        #endregion

        private bool DisplayMap(Map dispMap)
        {
            if (dispMap == null)
                return false;

            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(dispMap.FileName, UriKind.Relative);
            image.EndInit();

            mapCanvas.Height = image.PixelHeight;
            mapCanvas.Width = image.PixelWidth;

            imgMap.Source = image;

            DisplayMapDevices(dispMap.Id);

            lbxMapDevices.ItemsSource = dispMap.Devices;
            return true;
        }

        private void DisplayMapDevices(int mapId)
        {
            List<Device> devices = _system.GetMapDevices(mapId);
            if (devices == null)
                return;

            foreach (Device device in devices)
                UpdateDeviceLocation(device);
        }

        private Image GetDevicePin(ulong deviceId)
        {
            return _dictLocPin.ContainsKey(deviceId) ? _dictLocPin[deviceId] : null;
        }

        private void RemoveDevicePin(Device device)
        {
            Image pin = GetDevicePin(device.MacAddr);
            if (pin == null)
                return;

            mapCanvas.Children.Remove(pin);
            _dictLocPin.Remove(device.MacAddr);

            if (_dictCamState.ContainsKey(device.MacAddr))
            {
                mapCanvas.Children.Remove(_dictCamState[device.MacAddr]);
                _dictCamState.Remove(device.MacAddr);
            }
        }

        internal void UpdateDeviceLocation(Device device, bool force = false)
        {
            if (device == null || device.Location == null || _currentMap == null)
                return;

            Image pin = GetDevicePin(device.MacAddr);

            if ((_currentMap.Id != device.Location.MapId ||
                 device.Location.X == -1 ||
                 device.Location.Y == -1 || force)
                && (pin != null))
            {
                RemoveDevicePin(device);
                pin = null;

                if (!force)
                {
                    lbxMapDevices.Items.Refresh();
                    return;
                }
            }

            if (pin == null)
            {
                pin = new Image
                          {
                              Source = device.MacAddr == _highlightDevice ? _highlightMaker : _greenMaker,
                              SnapsToDevicePixels = true,
                              Stretch = Stretch.Uniform,
                              StretchDirection = StretchDirection.Both
                          };
                mapCanvas.Children.Add(pin);
                if (!force)
                    lbxMapDevices.Items.Refresh();

                if (!string.IsNullOrEmpty(device.Name))
                    pin.ToolTip = device.Name;

                pin.Cursor = Cursors.Hand;
                pin.MouseLeftButtonDown += OnClickPin;

                _dictLocPin.Add(device.MacAddr, pin);
            }

            int actX = device.Location.X - _greenMaker.DecodePixelHeight/2;
            int actY = device.Location.Y - _greenMaker.DecodePixelHeight;

            Canvas.SetLeft(pin, actX);
            Canvas.SetTop(pin, actY);
        }

        internal void UpdateCameraState(Device device)
        {
            if (device.Location.X == -1 ||
                device.Location.Y == -1)
                return;

            if (device.CameraSate == null)
                return;

            if (!_dictLocPin.ContainsKey(device.MacAddr))
                return;

            if (_dictCamState.ContainsKey(device.MacAddr))
            {
                mapCanvas.Children.Remove(_dictCamState[device.MacAddr]);
                _dictCamState.Remove(device.MacAddr);
            }

            var rotate = new RotateTransform(device.CameraSate.FaceAngle);
            rotate.CenterX = device.Location.X;
            rotate.CenterY = device.Location.Y;

            var cam = new Polygon();
            cam.Opacity = 0.4;
            cam.Stroke = Brushes.Black;
            cam.Fill = Brushes.LightSeaGreen;
            cam.StrokeThickness = 2;
            var point1 = new Point(device.Location.X, device.Location.Y);
            var point2 = new Point(device.Location.X - 40, device.Location.Y - 80);
            var point3 = new Point(device.Location.X + 40, device.Location.Y - 80);
            var myPointCollection = new PointCollection();
            myPointCollection.Add(point1);
            myPointCollection.Add(point2);
            myPointCollection.Add(point3);
            cam.Points = myPointCollection;
            cam.RenderTransform = rotate;
            mapCanvas.Children.Add(cam);

            _dictCamState.Add(device.MacAddr, cam);
        }

        private void UserControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            DisplayMap(_currentMap);
        }

        private void BtnSettingClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OptionsWindow(_system) {Owner = Application.Current.MainWindow};
            if (dialog.ShowDialog() == true)
            {
                App.ConfigManager.SaveSettings();
            }
        }

        private void BtnQuitClick(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void MapsListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var map = mapsList.SelectedItem as Map;
            if (map == null)
                return;

            _currentMap = map;
            DisplayMap(_currentMap);
        }

        public void UpdatePortrait(string num, string path)
        {
            Device device = _system.GetDevice(num);
            if (device != null)
                device.Portrait = path;
        }

        private void MapToggleButtonChecked(object sender, RoutedEventArgs e)
        {
        }

        private void MapToggleButtonUnchecked(object sender, RoutedEventArgs e)
        {
        }

        private void SetDeviceHighlight(Device device)
        {
            if (device == null)
                return;

            if (!_dictLocPin.ContainsKey(device.MacAddr))
                return;

            if (_highlightDevice != 0)
            {
                Device highlight = _system.GetDevice(_highlightDevice);
                SetDeviceUnhighlight(highlight);
            }

            device.IsHighlight = true;
            _highlightDevice = device.MacAddr;

            UpdateDeviceLocation(device, true);
        }

        private void SetDeviceUnhighlight(Device device)
        {
            if (device == null)
                return;

            if (!_dictLocPin.ContainsKey(device.MacAddr))
                return;

            device.IsHighlight = false;

            _highlightDevice = 0;

            UpdateDeviceLocation(device, true);
        }

        private void DeviceToggleButtonChecked(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            Debug.Assert(button != null);

            var device = button.DataContext as Device;
            Debug.Assert(device != null);

            SetDeviceHighlight(device);
        }

        private void DeviceToggleButtonUnchecked(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            Debug.Assert(button != null);

            var device = button.DataContext as Device;
            Debug.Assert(device != null);

            SetDeviceUnhighlight(device);
        }

        private void OnClickPin(object sender, RoutedEventArgs e)
        {
            var pin = sender as Image;
            if (pin == null)
                return;

            ulong deviceMac;
            foreach (var loc in _dictLocPin.Where(loc => loc.Value == pin))
            {
                deviceMac = loc.Key;
                break;
            }
        }
    }
}