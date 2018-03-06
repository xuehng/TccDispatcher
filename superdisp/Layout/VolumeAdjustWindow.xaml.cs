using System.Windows;
using CoreAudioApi;
using renstech.NET.SupernovaDispatcher.Model;

namespace renstech.NET.SupernovaDispatcher.Layout
{
	/// <summary>
	/// VolumeAdjustWindow.xaml 的交互逻辑
	/// </summary>
	public partial class VolumeAdjustWindow : Window
	{
        private const int step = 5;

        private MMDevice _devMainInput = null;
        private MMDevice _devMainOutput = null;
        private MMDevice _devLeftInput = null;
        private MMDevice _devLeftOutput = null;
        private MMDevice _devRightInput = null;
        private MMDevice _devRightOutput = null;

		public VolumeAdjustWindow()
		{
			this.InitializeComponent();

            InitializeDevices();

            InitializeControls();

            App app = App.Current as App;
            if (app.AppBkBrush != null)
                Background = app.AppBkBrush;
		}

        private void InitializeDevices()
        {
            MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
            MMDeviceCollection DevColl = DevEnum.EnumerateAudioEndPoints(EDataFlow.eAll, EDeviceState.DEVICE_STATE_ACTIVE);
            if (DevColl == null)
            {
                return;
            }

            App app = App.Current as App;
            GeneralSetting setting = app.GeneralSetting;

            _devMainInput = GetDevice(DevColl, setting.MainInput);
            _devMainOutput = GetDevice(DevColl, setting.MainOutput);
            _devRightInput = GetDevice(DevColl, setting.RightInput);
            _devRightOutput = GetDevice(DevColl, setting.RightOutput);
            _devLeftInput = GetDevice(DevColl, setting.LeftInput);
            _devLeftOutput = GetDevice(DevColl, setting.LeftOutput);
        }

        private MMDevice GetDevice(MMDeviceCollection collection, string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            for (int i = 0; i < collection.Count; i++)
            {
                MMDevice device = collection[i];

                if (string.Compare(device.Name, 0, name, 0, name.Length) == 0)
                    return device;
            }

            return null;
        }

        private void InitializeControls()
        {
            if (_devMainInput == null)
            {
                btnMainMicDecrease.IsEnabled = false;
                btnMainMicIncrease.IsEnabled = false;
                sliderMainMic.IsEnabled = false;
            }
            else
            {
                sliderMainMic.Minimum = 0;
                sliderMainMic.Maximum = 100;
                sliderMainMic.Value = (int)(_devMainInput.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
            }

            if (_devMainOutput == null)
            {
                btnMainSpeakerIncrease.IsEnabled = false;
                btnMainSpeakerDecrease.IsEnabled = false;
                sliderMainSpeaker.IsEnabled = false;
            }
            else
            {
                sliderMainSpeaker.Minimum = 0;
                sliderMainSpeaker.Maximum = 100;
                sliderMainSpeaker.Value = (int)(_devMainOutput.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
            }

            if (_devLeftInput == null)
            {
                btnLeftMicDecrease.IsEnabled = false;
                btnLeftMicIncrease.IsEnabled = false;
                sliderLeftMic.IsEnabled = false;
            }
            else
            {
                sliderLeftMic.Minimum = 0;
                sliderLeftMic.Maximum = 100;
                sliderLeftMic.Value = (int)(_devLeftInput.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
            }

            if (_devLeftOutput == null)
            {
                btnLeftSpeakerDecrease.IsEnabled = false;
                btnLeftSpeakerIncrease.IsEnabled = false;
                sliderLeftSpeaker.IsEnabled = false;
            }
            else
            {
                sliderLeftSpeaker.Minimum = 0;
                sliderLeftSpeaker.Maximum = 100;
                sliderLeftSpeaker.Value = (int)(_devLeftOutput.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
            }

            if (_devRightInput == null)
            {
                btnRightMicDecrease.IsEnabled = false;
                btnRightMicIncrease.IsEnabled = false;
                sliderRightMic.IsEnabled = false;
            }
            else
            {
                sliderRightMic.Minimum = 0;
                sliderRightMic.Maximum = 100;
                sliderRightMic.Value = (int)(_devRightInput.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
            }

            if (_devRightOutput == null)
            {
                btnRightSpeakerDecrease.IsEnabled = false;
                btnRightSpeakerIncrease.IsEnabled = false;
                sliderRightSpeaker.IsEnabled = false;
            }
            else
            {
                sliderRightSpeaker.Minimum = 0;
                sliderRightSpeaker.Maximum = 100;
                sliderRightSpeaker.Value = (int)(_devRightOutput.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
            }
        }

        private void sliderMainMic_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            _devMainInput.AudioEndpointVolume.MasterVolumeLevelScalar = ((float)sliderMainMic.Value / 100.0f);
        }

        private void sliderMainSpeaker_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            _devMainOutput.AudioEndpointVolume.MasterVolumeLevelScalar = ((float)sliderMainSpeaker.Value / 100.0f);
        }

        private void sliderLeftMic_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            _devLeftInput.AudioEndpointVolume.MasterVolumeLevelScalar = ((float)sliderLeftMic.Value / 100.0f);
        }

        private void sliderLeftSpeaker_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            _devLeftOutput.AudioEndpointVolume.MasterVolumeLevelScalar = ((float)sliderLeftSpeaker.Value / 100.0f);
        }

        private void sliderRightMic_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            _devRightInput.AudioEndpointVolume.MasterVolumeLevelScalar = ((float)sliderRightMic.Value / 100.0f);
        }

        private void sliderRightSpeaker_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            _devRightOutput.AudioEndpointVolume.MasterVolumeLevelScalar = ((float)sliderRightSpeaker.Value / 100.0f);
        }

        private void btnMainMicIncrease_Click(object sender, RoutedEventArgs e)
        {
            if (sliderMainMic.Value + step > sliderMainMic.Maximum)
            {
                sliderMainMic.Value = sliderMainMic.Maximum;
            }
            else
            {
                sliderMainMic.Value += 5;
            }
        }

        private void btnMainMicDecrease_Click(object sender, RoutedEventArgs e)
        {
            if (sliderMainMic.Value - step < sliderMainMic.Minimum)
            {
                sliderMainMic.Value = sliderMainMic.Minimum;
            }
            else
            {
                sliderMainMic.Value -= 5;
            }
        }

        private void btnMainSpeakerIncrease_Click(object sender, RoutedEventArgs e)
        {
            if (sliderMainSpeaker.Value + step > sliderMainSpeaker.Maximum)
            {
                sliderMainSpeaker.Value = sliderMainSpeaker.Maximum;
            }
            else
            {
                sliderMainSpeaker.Value += 5;
            }
        }

        private void btnMainSpeakerDecrease_Click(object sender, RoutedEventArgs e)
        {
            if (sliderMainSpeaker.Value - step < sliderMainSpeaker.Minimum)
            {
                sliderMainSpeaker.Value = sliderMainSpeaker.Minimum;
            }
            else
            {
                sliderMainSpeaker.Value -= 5;
            }
        }

        private void btnLeftMicIncrease_Click(object sender, RoutedEventArgs e)
        {
            if (sliderLeftMic.Value + step > sliderLeftMic.Maximum)
            {
                sliderLeftMic.Value = sliderLeftMic.Maximum;
            }
            else
            {
                sliderLeftMic.Value += 5;
            }
        }

        private void btnLeftMicDecrease_Click(object sender, RoutedEventArgs e)
        {
            if (sliderLeftMic.Value - step < sliderLeftMic.Minimum)
            {
                sliderLeftMic.Value = sliderLeftMic.Minimum;
            }
            else
            {
                sliderLeftMic.Value -= 5;
            }
        }

        private void btnLeftSpeakerIncrease_Click(object sender, RoutedEventArgs e)
        {
            if (sliderLeftSpeaker.Value + step > sliderLeftSpeaker.Maximum)
            {
                sliderLeftSpeaker.Value = sliderLeftSpeaker.Maximum;
            }
            else
            {
                sliderLeftSpeaker.Value += 5;
            }
        }

        private void btnLeftSpeakerDecrease_Click(object sender, RoutedEventArgs e)
        {
            if (sliderLeftSpeaker.Value - step < sliderLeftSpeaker.Minimum)
            {
                sliderLeftSpeaker.Value = sliderLeftSpeaker.Minimum;
            }
            else
            {
                sliderLeftSpeaker.Value -= 5;
            }
        }

        private void btnRightMicIncrease_Click(object sender, RoutedEventArgs e)
        {
            if (sliderRightMic.Value + step > sliderRightMic.Maximum)
            {
                sliderRightMic.Value = sliderRightMic.Maximum;
            }
            else
            {
                sliderRightMic.Value += 5;
            }
        }

        private void btnRightMicDecrease_Click(object sender, RoutedEventArgs e)
        {
            if (sliderRightMic.Value - step < sliderRightMic.Minimum)
            {
                sliderRightMic.Value = sliderRightMic.Minimum;
            }
            else
            {
                sliderRightMic.Value -= 5;
            }
        }

        private void btnRightSpeakerIncrease_Click(object sender, RoutedEventArgs e)
        {
            if (sliderRightSpeaker.Value + step > sliderRightSpeaker.Maximum)
            {
                sliderRightSpeaker.Value = sliderRightSpeaker.Maximum;
            }
            else
            {
                sliderRightSpeaker.Value += 5;
            }
        }

        private void btnRightSpeakerDecrease_Click(object sender, RoutedEventArgs e)
        {
            if (sliderRightSpeaker.Value - step < sliderRightSpeaker.Minimum)
            {
                sliderRightSpeaker.Value = sliderRightSpeaker.Minimum;
            }
            else
            {
                sliderRightSpeaker.Value -= 5;
            }
        }
	}
}