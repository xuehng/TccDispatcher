using System.ComponentModel;
using renstech.NET.SIPUA;

namespace renstech.NET.SupernovaDispatcher.Model
{
    public class HandsetInfo : INotifyPropertyChanged
    {
        private bool _isOnLine;
        private bool _isoffhook;
        private bool _isAnswered;
        private bool _isRinging;
        private string _callPary;

        public HandsetInfo(Handset.Handset handset, string name)
        {
            Id = handset.Id;
            IsOnLine = handset.IsOnLine;
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null) PropertyChanged(this, e);
        }

        public string PartyDisplayName 
        {
            get { return _callPary; }
            set { _callPary = value; OnPropertyChanged(new PropertyChangedEventArgs("PartyDisplayName")); } 
        }

        public string PartyDisplayNumber { get; set; }
        public string PartyNumber { get; set; }
        public sua_role_e Role { get; set; }
        public bool IsBusy { get; set; }

        public bool IsOnLine
        {
            get { return _isOnLine; }
            set { _isOnLine = value; OnPropertyChanged(new PropertyChangedEventArgs("IsOnLine")); }
        }

        public bool IsAnswered
        {
            get { return _isAnswered; }
            set { _isAnswered = value; OnPropertyChanged(new PropertyChangedEventArgs("IsAnswered")); }
        }

        public bool IsRinging
        {
            get { return _isRinging; }
            set { _isRinging = value; OnPropertyChanged(new PropertyChangedEventArgs("IsRinging")); }
        }

        public bool IsOffHook
        {
            get { return _isoffhook; }
            set { _isoffhook = value; OnPropertyChanged(new PropertyChangedEventArgs("IsOffHook")); }
        }

        public void ResetState()
        {
            PartyNumber = string.Empty;
            PartyDisplayName = string.Empty;
            PartyDisplayNumber = string.Empty;

            IsBusy = false;
            IsAnswered = false;
            IsRinging = false;
        }
    }
}
