using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace renstech.NET.SupernovaDispatcher.IPSDispatch
{
    internal class Device : INotifyPropertyChanged
    {
        private string _portraitPath;

        public Device(ulong mac, string name, string number)
        {
            MacAddr = mac;
            Name = name;
            Number = number;

            Location = new Location();
        }

        public ulong MacAddr { get; private set; }
        public string Name { get; private set; }
        public string Number { get; private set; }

        public Location Location { get; set; }
        public CameraState CameraSate { get; set; }
        public bool IsDetected { get; set; }

        public bool IsHighlight { get; set; }
        public static string DefaultPortrait { get; set; }

        public string Portrait
        {
            get { return _portraitPath; }
            set
            {
                _portraitPath = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Portrait"));
                OnPropertyChanged(new PropertyChangedEventArgs("PortraitImage"));
            }
        }

        public BitmapSource PortraitImage
        {
            get
            {
                string path = _portraitPath;
                if (string.IsNullOrEmpty(path))
                    path = DefaultPortrait;

                if (string.IsNullOrEmpty(path))
                    return null;

                try
                {
	                BitmapImage image = new BitmapImage();
	                image.BeginInit();
	                image.UriSource = new Uri(path);
	                image.EndInit();
	                return image;
                }
                catch (System.Exception ex)
                {
                    return null;
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
    }
}