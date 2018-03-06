using System.ComponentModel;

namespace renstech.NET.PJSIP
{
    public class Account : INotifyPropertyChanged
    {
        public static readonly int ACCOUNT_NULL = -1;
        private bool _isRegistered;

        public Account()
        {
            Id = ACCOUNT_NULL;

            IsDomainRegistration = true;
            IsRegistrationEnabled = true;
            IsAutoAnswer = false;
            IsAutoShowInboundVideo = false;
            RegistrationExpire = 3600;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        public int Id { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        public string Proxy { get; set; }
        public bool IsRegistrationEnabled { get; set; }
        public bool IsDomainRegistration { get; set; }
        public uint RegistrationExpire { get; set; }
        public bool IsAutoAnswer { get; set; }
        public bool IsAutoShowInboundVideo { get; set; }

        public bool IsRegistered
        {
            get { return _isRegistered; }
            set { _isRegistered = value; OnPropertyChanged(new PropertyChangedEventArgs("IsRegistered")); }
        }
    }
}
