using System.ComponentModel;

namespace renstech.NET.SupernovaDispatcher.Model
{
    public class UserDialogInfo : INotifyPropertyChanged
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(UserDialogInfo));

        private bool _isAnswered;
        private bool _isInCall;
        private bool _isInbound;
        private bool _isRinging;
        private bool _isAccountRelated;

        private string _partyDisplayName;
        private string _partyDisplayNumber;

        public string PartyName { get; set; }
        public string PartyNumber { get; set; }

        public string PartyDisplayName
        {
            get { return _partyDisplayName; }
            set
            {
                if (_partyDisplayName != value)
                {
                    _partyDisplayName = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("PartyDisplayName"));    
                }
            }
        }

        public string PartyDisplayNumber
        {
            get { return _partyDisplayNumber; }
            set
            {
                if (_partyDisplayNumber != value)
                {
                    _partyDisplayNumber = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("PartyDisplayNumber"));
                }
            }
        }

        public bool IsAccountRelated
        {
            get { return _isAccountRelated; }
            set
            {
                if (_isAccountRelated != value)
                {
                    _isAccountRelated = value;

                    OnPropertyChanged(new PropertyChangedEventArgs("IsAccountRelated"));
                    //OnPropertyChanged(new PropertyChangedEventArgs("IsInCallWithAccount"));
                    //OnPropertyChanged(new PropertyChangedEventArgs("IsAnsweredWithAccount"));
                    //OnPropertyChanged(new PropertyChangedEventArgs("IsRingingWithAccount"));
                }
            }
        }

        public bool IsInCall
        {
            get { return _isInCall; }
            set
            {
                if (_isInCall != value)
                {
                    _isInCall = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsIncall"));
                    OnPropertyChanged(new PropertyChangedEventArgs("IsInCallWithAccount"));
                    Log.Debug(string.Format("Model: UserDialogInfo, PartyDisplayName:{0}, IsInCall:{1},{2}", PartyDisplayName, value, value ? "" : "background to normal"));
                }
            }
        }

        public bool IsInbound
        {
            get { return _isInbound; }
            set
            {
                if (_isInbound != value)
                {
                    _isInbound = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsInbound"));
                }
            }
        }

        public bool IsRinging
        {
            get { return _isRinging; }
            set
            {
                if (_isRinging != value)
                {
                    _isRinging = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsRinging"));
                    OnPropertyChanged(new PropertyChangedEventArgs("IsRingingWithAccount"));
                    Log.Debug(string.Format("Model: UserDialogInfo, PartyDisplayName:{0}, IsRinging:{1}, {2}", PartyDisplayName, value, value?"start blinking":"stop blinking"));
                }
            }
        }

        public bool IsAnswered
        {
            get { return _isAnswered; }
            set
            {
                if (_isAnswered != value)
                {
                    _isAnswered = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsAnswered"));
                    OnPropertyChanged(new PropertyChangedEventArgs("IsAnsweredWithAccount"));
                    Log.Debug(string.Format("Model: UserDialogInfo, PartyDisplayName:{0}, IsAnswered:{1}, {2}", PartyDisplayName, value, value ? "background to red" : ""));
                }
            }
        }

        public bool IsAnsweredWithAccount
        {
            get { return IsAnswered && IsAccountRelated; }
        }

        public bool IsRingingWithAccount
        {
            get { return IsRinging && IsAccountRelated; }
        }

        public bool IsInCallWithAccount
        {
            get { return IsInCall && IsAccountRelated; }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        public void Reset()
        {
            //考虑到触发器的触发，需要按顺序进行重置
            IsInbound = false;
            IsAccountRelated = false;
            IsRinging = false;
            IsAnswered = false; 
            IsInCall = false;

            PartyName = string.Empty;
            PartyNumber = string.Empty;
            PartyDisplayName = string.Empty;
            PartyDisplayNumber = string.Empty;
        }
    }
}