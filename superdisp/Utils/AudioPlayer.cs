using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Windows;

namespace renstech.NET.SupernovaDispatcher.Utils
{
    class AudioPlayer
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(AudioPlayer));

        private IWavePlayer _waveOut = null;
        private WaveStream _fileWaveStream = null;

        public string FilePath { get; set; }
        public string DeviceName { get; set; }

        public bool IsPlaying 
        { 
            get
            {
                if (_waveOut != null)
                    return (_waveOut.PlaybackState == PlaybackState.Playing);
                return false;
            } 
        }

        public bool IsPaused
        {
            get
            {
                if (_waveOut != null)
                    return (_waveOut.PlaybackState == PlaybackState.Paused);
                return false;
            } 
        }

        private int GetDeviceId(string devname)
        {
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                var capabilities = WaveOut.GetCapabilities(i);
                if (string.Compare(capabilities.ProductName, 0, devname, 0, devname.Length) == 0 )
                {
                    return i;
                }
            }
            return -1;
        }

        public bool Play()
        {
            if (IsPlaying)
            {
                return false;
            }

            if (IsPaused)
            {
                _waveOut.Play();
                return true;
            }

            if (string.IsNullOrEmpty(FilePath) || string.IsNullOrEmpty(DeviceName) )
            {
                return false;
            }

            int devId = GetDeviceId(DeviceName);
            if (devId == -1)
            {
                return false;
            }

            try
            {
                CreateWaveOut(devId);
            }
            catch (Exception driverCreateException)
            {
                Log.Debug("Play__MessageBox.Show driverCreateException BEFORE");
                MessageBox.Show(String.Format("{0}", driverCreateException.Message));
                Log.Debug("Play__MessageBox.Show driverCreateException AFTER"); 
                return false;
            }

            ISampleProvider sampleProvider = null;
            try
            {
                sampleProvider = CreateInputStream();
            }
            catch (Exception createException)
            {
                Log.Debug("Play__MessageBox.Show createException BEFORE");
                MessageBox.Show(String.Format("{0}", createException.Message), "Error Loading File");
                Log.Debug("Play__MessageBox.Show createException AFTER"); 
                return false;
            }

            try
            {
                _waveOut.Init(new SampleToWaveProvider(sampleProvider));
            }
            catch (Exception initException)
            {
                Log.Debug("Play__MessageBox.Show initException BEFORE");
                MessageBox.Show(String.Format("{0}", initException.Message), "Error Initializing Output");
                Log.Debug("Play__MessageBox.Show initException AFTER"); 
                return false;
            }

            _waveOut.Play();
            return true;
        }

        public void Stop()
        {
            if (_waveOut != null)
            {
                _waveOut.Stop();
            }
        }

        public void CreateWaveOut(int devId)
        {
            if (_waveOut != null)
            {
                CloseWaveOut();
            }

            var waveOut = new WaveOutEvent();
            waveOut.DeviceNumber = devId;
            waveOut.DesiredLatency = 300;
            _waveOut = waveOut;
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
            WaveStream readerStream = new WaveFileReader(FilePath);
            if (readerStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm && readerStream.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
            {
                readerStream = WaveFormatConversionStream.CreatePcmStream(readerStream);
                readerStream = new BlockAlignReductionStream(readerStream);
            }
            _fileWaveStream = readerStream;

            var waveChannel = new SampleChannel(_fileWaveStream);
            var postVolumeMeter = new MeteringSampleProvider(waveChannel);

            return postVolumeMeter;
        }
    }
}
