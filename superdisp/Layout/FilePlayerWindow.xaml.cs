using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using renstech.NET.SupernovaDispatcher.Model;

namespace renstech.NET.SupernovaDispatcher.Layout
{
    /// <summary>
    ///  WAVPlayWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FilePlayerWindow : Window
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(FilePlayerWindow));

        private DispatcherTimer _tickTimer = null;
        private IWavePlayer _waveOut = null;
        private WaveCallbackStrategy _waveCallbackStrategy = WaveCallbackStrategy.Event;
        private WaveStream _fileWaveStream = null; 
        private List<string> _outputdevices = new List<string>();
        private List<string> _outputdevicesname = new List<string>();

        public FilePlayerWindow(string file)
        {
            this.InitializeComponent();

            App app = App.Current as App;
            if (app.AppBkBrush != null)
                Background = app.AppBkBrush;

            InitializeOutputDevices();

            btnPause.IsEnabled = false;
            btnStop.IsEnabled = false;

            FileName = file;

            _tickTimer = new DispatcherTimer();
            _tickTimer.Tick += TimerHandler;
            _tickTimer.Interval = new TimeSpan(0, 0, 1);
        }

        public string FileName { get; private set; }

        private void InitializeOutputDevices()
        {
            _outputdevices.Clear();
            _outputdevicesname.Clear();

            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                var capabilities = WaveOut.GetCapabilities(i);
                _outputdevices.Add(capabilities.ProductName);
            }

            App app = App.Current as App;
            GeneralSetting setting = app.GeneralSetting;
            
            if (setting != null)
            {
                if (!string.IsNullOrEmpty(setting.MainOutput))
                    _outputdevicesname.Add(Properties.Resources.IDS_MAIN_SPEAKER);

                if (!string.IsNullOrEmpty(setting.LeftOutput))
                    _outputdevicesname.Add(Properties.Resources.IDS_HANDSET_LEFT);

                if (!string.IsNullOrEmpty(setting.RightOutput))
                    _outputdevicesname.Add(Properties.Resources.IDS_HANDSET_RIGHT);
            }
            
            if ( _outputdevicesname.Count == 0 )
            {
                comboxPlayDevices.IsEnabled = false;
            }
            else
            {
                comboxPlayDevices.ItemsSource = _outputdevicesname;
                comboxPlayDevices.SelectedItem = Properties.Resources.IDS_MAIN_SPEAKER;
            }
        }

        private static int CompareStringMinLen(string str1, string str2)
        {
            int len = str1.Length < str2.Length ? str1.Length : str2.Length;
            return string.Compare(str1, 0, str2, 0, len);
        }

        private int GetDeviceId(string name)
        {
            App app = App.Current as App;
            GeneralSetting setting = app.GeneralSetting;
            if (setting == null)
            {
                return -1;
            }

            string devicename = null;
            if (name == Properties.Resources.IDS_MAIN_SPEAKER)
            {
                devicename = setting.MainOutput;                
            }

            if (name == Properties.Resources.IDS_HANDSET_LEFT)
            {
                devicename = setting.LeftOutput;
            }

            if (name == Properties.Resources.IDS_HANDSET_RIGHT)
            {
                devicename = setting.RightOutput;
            }

            if (string.IsNullOrEmpty(devicename))
            {
                return -1;
            }

            foreach (string dev in _outputdevices)
            {
                if (CompareStringMinLen(dev, devicename) == 0)
                    return _outputdevices.IndexOf(dev);
            }
            return -1;
        }

        private void CreateWaveOut()
        {
            CloseWaveOut();

            int latency = 300;
            int deviceId = GetDeviceId(comboxPlayDevices.SelectedItem as string);

            if (_waveCallbackStrategy == WaveCallbackStrategy.Event)
            {
                var waveOut = new WaveOutEvent();
                waveOut.DeviceNumber = deviceId;
                waveOut.DesiredLatency = latency;
                _waveOut = waveOut;
            }
            else
            {
                WaveCallbackInfo callbackInfo = _waveCallbackStrategy == WaveCallbackStrategy.NewWindow ? WaveCallbackInfo.NewWindow() : WaveCallbackInfo.FunctionCallback();
                WaveOut outputDevice = new WaveOut(callbackInfo);
                outputDevice.DeviceNumber = deviceId;
                outputDevice.DesiredLatency = latency;
                _waveOut = outputDevice;
            }

            _waveOut.PlaybackStopped += OnPlaybackStoped;
        }

        private void CloseWaveOut()
        {
            if (_waveOut != null)
            {
                _waveOut.Stop();
            }

            if (_fileWaveStream != null)
            {
                _fileWaveStream.Dispose();
                _fileWaveStream = null;
            }

            if (_waveOut != null)
            {
                _waveOut.Dispose();
                _waveOut = null;
            }
        }

        private ISampleProvider CreateInputStream()
        {
            WaveStream readerStream = new WaveFileReader(FileName);
            if (readerStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm && readerStream.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
            {
                readerStream = WaveFormatConversionStream.CreatePcmStream(readerStream);
                readerStream = new BlockAlignReductionStream(readerStream);
            }
            _fileWaveStream = readerStream;

            var waveChannel = new SampleChannel(_fileWaveStream);
            waveChannel.PreVolumeMeter += OnPreVolumeMeter;

            var postVolumeMeter = new MeteringSampleProvider(waveChannel);
            postVolumeMeter.StreamVolume += OnPostVolumeMeter; 
            
            return postVolumeMeter;
        }

        void OnPreVolumeMeter(object sender, StreamVolumeEventArgs e)
        {

        }

        void OnPostVolumeMeter(object sender, StreamVolumeEventArgs e)
        {

        }

        void OnPlaybackStoped(object sender, StoppedEventArgs e)
        {
            _tickTimer.Stop();
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (_waveOut != null)
            {
                if (_waveOut.PlaybackState == PlaybackState.Playing)
                {
                    return;
                }
                else if (_waveOut.PlaybackState == PlaybackState.Paused)
                {
                    _waveOut.Play();

                    btnPlay.IsEnabled = false;
                    btnPause.IsEnabled = true;
                    btnStop.IsEnabled = true;
                    return;
                }
            }

            if (string.IsNullOrEmpty(FileName))
            {
                return;
            }

            try
            {
                CreateWaveOut();
            }
            catch (Exception driverCreateException)
            {
                Log.Debug("btnPlay_Click__MessageBox.Show driverCreateException BEFORE");
                MessageBox.Show(String.Format("{0}", driverCreateException.Message));
                Log.Debug("btnPlay_Click__MessageBox.Show driverCreateException AFTER");
                return;
            }

            ISampleProvider sampleProvider = null;
            try
            {
                sampleProvider = CreateInputStream();
            }
            catch (Exception createException)
            {
                Log.Debug("btnPlay_Click__MessageBox.Show createException BEFORE");
                MessageBox.Show(String.Format("{0}", createException.Message), "Error Loading File");
                Log.Debug("btnPlay_Click__MessageBox.Show createException AFTER");
                return;
            }

            sliderPosition.Maximum = (int)_fileWaveStream.TotalTime.TotalSeconds;
            sliderPosition.TickFrequency = sliderPosition.Maximum / 60;
            txtTime.Text = string.Format("00:00:00 / {0:00}:{1:00}:{2:00}",
                                    (int)_fileWaveStream.TotalTime.Hours,
                                    (int)_fileWaveStream.TotalTime.Minutes,
                                    (int)_fileWaveStream.TotalTime.Seconds);

            try
            {
                _waveOut.Init(new SampleToWaveProvider(sampleProvider));
            }
            catch (Exception initException)
            {
                Log.Debug("btnPlay_Click__MessageBox.Show initException BEFORE");
                MessageBox.Show(String.Format("{0}", initException.Message), "Error Initializing Output");
                Log.Debug("btnPlay_Click__MessageBox.Show initException AFTER"); 
                return;
            }

            _waveOut.Play();
            _tickTimer.Start();

            btnPlay.IsEnabled = false;
            btnPause.IsEnabled = true;
            btnStop.IsEnabled = true;
        }

        private void TimerHandler(object sender, EventArgs e)
        {
            if (_waveOut != null && _fileWaveStream != null)
            {
                TimeSpan currentTime = (_waveOut.PlaybackState == PlaybackState.Stopped) ? TimeSpan.Zero : _fileWaveStream.CurrentTime;
                if ((_fileWaveStream.TotalTime -_fileWaveStream.CurrentTime).TotalSeconds <= 0.5)
                {
                    btnStop_Click(sender, new RoutedEventArgs() );
                }
                else
                {
                    sliderPosition.Value = currentTime.TotalSeconds;
                    txtTime.Text = string.Format("{0:00}:{1:00}:{2:00} / {3:00}:{4:00}:{5:00}",
                                            (int)currentTime.Hours,
                                            (int)currentTime.Minutes,
                                            (int)currentTime.Seconds,
                                            (int)_fileWaveStream.TotalTime.Hours,
                                            (int)_fileWaveStream.TotalTime.Minutes,
                                            (int)_fileWaveStream.TotalTime.Seconds);

                }
            }
            else
            {
                sliderPosition.Value = 0;
            }
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            if (_waveOut != null && _waveOut.PlaybackState == PlaybackState.Playing)
            {
                _waveOut.Pause();

                btnPause.IsEnabled = false;
                btnPlay.IsEnabled = true;
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (_waveOut != null)
            {
                _waveOut.Stop();
            }

            btnPlay.IsEnabled = true;
            btnPause.IsEnabled = false;
            btnStop.IsEnabled = false;
            sliderPosition.Value = 0;  
        }

        private void SliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_waveOut == null || _fileWaveStream == null)
            {
                return;
            }

            _fileWaveStream.CurrentTime = TimeSpan.FromSeconds(sliderPosition.Value);

            txtTime.Text = string.Format("{0:00}:{1:00}:{2:00} / {3:00}:{4:00}:{5:00}",
                                            (int)_fileWaveStream.CurrentTime.TotalHours,
                                            (int)_fileWaveStream.CurrentTime.TotalMinutes,
                                            (int)_fileWaveStream.CurrentTime.TotalSeconds,
                                            (int)_fileWaveStream.TotalTime.TotalHours,
                                            (int)_fileWaveStream.TotalTime.TotalMinutes,
                                            (int)_fileWaveStream.TotalTime.TotalSeconds);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _tickTimer.Stop();
            CloseWaveOut();
        }
    }
}