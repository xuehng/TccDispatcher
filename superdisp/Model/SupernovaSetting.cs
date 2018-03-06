using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace renstech.NET.SupernovaDispatcher.Model
{
    public class SupernovaSetting : SettingItem, INotifyPropertyChanged, ICloneable
    {
        private string _accUser;
        private string _accPassword;
        private string _accDomain;
        private string _accProxy;
        private bool _isRegDomain;
        private int _regInterval;

        private string _nidsAddr;
        private int _nidsIntructPort;
        private int _nidsEventPort;
        private int _nidsEchoPort;
        private bool _isFailoverDetect;
        private string _presenceTag;
        private string _dialogTag;
        private string _heartbeatTag;

        private string _ringFile;
        private string _historyFile;
        private string _custUserFile;
        private string _mixedGroupsFile;
        
        private string _recordingFileDir;
        private int _recordingDelDate;

        private int _userbtnHeight;
        private int _userbtnLineCnt;

        private bool _isAutoAnswer;
        private bool _isAutoPhoneRedirect;
        private string _autoPhoneRedirectNum;

        private string _ipAddress;
        private int _xmlrpcPort;
        private string _tccXmlrpcServerAddr;
        private int _tccXmlrpcServerPort;

        private string _outCallPrefix;

        public SupernovaSetting()
        {
            IsModified = false;

            _isRegDomain = true;
            _regInterval = 3600;
            _isFailoverDetect = true;
            _nidsIntructPort = 7085;
            _nidsEventPort = 5556;
            _nidsEchoPort = 5557;
            _presenceTag = "presence@pubsub.renstech.com";
            _dialogTag = "dialog@pubsub.renstech.com";
            _heartbeatTag = "heartbeat@pubsub.renstech.com";

            _ringFile = "telephone-ring-4";
            _historyFile = "callhistory.lst";
            _custUserFile = "custom_users.lst";
            _mixedGroupsFile = "custom_groups.lst";
            
            _recordingFileDir = Environment.CurrentDirectory + "\\recordings";
            _recordingDelDate = 7;
            IsRecordingFileDirChanged = false;

            _userbtnHeight = 80;
            _userbtnLineCnt = 7;

            _outCallPrefix = "9";
        }

        public string AccountUser
        {
            get { return _accUser; }
            set { IsModified = true;
                _accUser = value;
            }
        }

        public string AccountPassword 
        {
            get { return _accPassword; }
            set { IsModified = true;
                _accPassword = value;
            }
        }

        public string AccountDomain
        {
            get { return _accDomain; }
            set { IsModified = true;
                _accDomain = value;
            }
        }

        public string AccountProxy
        {
            get { return _accProxy; }
            set { IsModified = true;
                _accProxy = value;
            }
        }

        public bool IsDomainReg
        {
            get { return _isRegDomain; }
            set { IsModified = true;
                _isRegDomain = value;
            }
        }

        [XmlIgnore]
        public bool IsProxyReg
        {
            get { return !_isRegDomain; }
            set { IsModified = true;
                _isRegDomain = !value;
            }
        }

        public int RegExpire
        {
            get { return _regInterval; }
            set { IsModified = true;
                _regInterval = value;
            }
        }

        public bool IsFailOverDetect
        {
            get { return _isFailoverDetect; }
            set { IsModified = true;
                _isFailoverDetect = value;
            }
        }

        public string NidsAddr
        {
            get { return _nidsAddr; }
            set { IsModified = true;
                _nidsAddr = value;
            }
        }

        public int NidsInstructPort
        {
            get { return _nidsIntructPort; }
            set { IsModified = true;
                _nidsIntructPort = value;
            }
        }

        public int NidsEventPort
        {
            get { return _nidsEventPort; }
            set { IsModified = true;
                _nidsEventPort = value;
            } 
        }

        public int NidsEchoPort
        {
            get { return _nidsEchoPort; }
            set { IsModified = true;
                _nidsEchoPort = value;
            }
        }

        public string PresenceTag
        {
            get { return _presenceTag; }
            set { IsModified = true;
                _presenceTag = value;
            }
        }

        public string DialogTag
        {
            get { return _dialogTag; }
            set { IsModified = true;
                _dialogTag = value;
            }
        }

        public string HeartbeatTag
        {
            get { return _heartbeatTag; }
            set
            {
                IsModified = true;
                _heartbeatTag = value;
            }
        }

        public string RingFileName
        {
            get { return _ringFile; }
            set { IsModified = true; 
                _ringFile = value; }
        }

        public string CallHistoryFile
        {
            get { return _historyFile; }
            set { IsModified = true;
                _historyFile = value;
            }
        }

        public string CustomUsersFile
        {
            get { return _custUserFile; }
            set
            { IsModified = true; 
              _custUserFile = value; }
        }

        public string MixedGroupsFile
        {
            get { return _mixedGroupsFile; }
            set
            {
                IsModified = true;
                _mixedGroupsFile = value; }
        }

        public string RecordingFileDir
        {
            get
            {
                if (!System.IO.Directory.Exists(_recordingFileDir))
                {
                    _recordingFileDir = Environment.CurrentDirectory + "\\recordings";
                    if (!System.IO.Directory.Exists(_recordingFileDir))
                        System.IO.Directory.CreateDirectory(_recordingFileDir);
                }
                return _recordingFileDir;
            }
            set
            {
                if (_recordingFileDir != value)
                {
                    IsModified = true;
                    _recordingFileDir = value;
                    IsRecordingFileDirChanged = true;
                    OnPropertyChanged(new PropertyChangedEventArgs("RecordingFileDir"));
                    OnPropertyChanged(new PropertyChangedEventArgs("IsRecordingFileDirChanged"));
                }
            }
        }

        public int RecordingDelDate
        {
            get { return _recordingDelDate; }
            set
            {
                IsModified = true;
                _recordingDelDate = value;
                OnPropertyChanged(new PropertyChangedEventArgs("RecordingDelDate"));
            }
        }

        [XmlIgnore]
        public bool IsRecordingFileDirChanged { get; set; }

        public int UserButtonLineHeight
        {
            get { return _userbtnHeight; }
            set { IsModified = true;
                _userbtnHeight = value;
            }
        }

        public int UserButtonLineCount
        {
            get { return _userbtnLineCnt; }
            set { IsModified = true;
                _userbtnLineCnt = value;
            }
        }

        public string IpAddress
        {
            get { return _ipAddress; }
            set { IsModified = true;
                _ipAddress = value;
            }
        }

        public int XmlrpcPort
        {
            get { return _xmlrpcPort; }
            set { IsModified = true;
                _xmlrpcPort = value;
            }
        }

        public string TccXmlrpcServerAddr
        {
            get { return _tccXmlrpcServerAddr; }
            set { IsModified = true;
                _tccXmlrpcServerAddr = value;
            }
        }

        public int TccXmlrpcServerPort
        {
            get { return _tccXmlrpcServerPort; }
            set { IsModified = true;
                _tccXmlrpcServerPort = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
        
        [XmlIgnore]
        public bool IsAutoAnswer
        {
            get { return _isAutoAnswer; }
            set
            {
                _isAutoAnswer = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsAutoAnswer"));
            }
        }

        [XmlIgnore]
        public bool IsAutoPhoneRedirect
        {
            get { return _isAutoPhoneRedirect; }
            set
            {
                _isAutoPhoneRedirect = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsAutoPhoneRedirect"));
            }
        }

        [XmlIgnore]
        public string AutoPhoneRedirectNumber
        {
            get { return _autoPhoneRedirectNum; }
            set
            {
                _autoPhoneRedirectNum = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AutoPhoneRedirectNumber"));
            }
        }

        public string GetRingFilePath()
        {
            if (string.IsNullOrEmpty(RingFileName))
                return null;

            string file = string.Format("{0}.wav", RingFileName);
            string dir = System.IO.Path.Combine(Environment.CurrentDirectory,
                Properties.Settings.Default.RingingFileDir);
            return System.IO.Path.Combine(dir, file);
        }

        public string OutCallPrefix
        {
            get { return _outCallPrefix; }
            set { _outCallPrefix = value; }
        }

        public object Clone()
        {
            var rVal = new SupernovaSetting();

            rVal._accUser = _accUser;
            rVal._accPassword = _accPassword;
            rVal._accDomain = _accDomain;
            rVal._accProxy = _accProxy;
            rVal._isRegDomain = _isRegDomain;
            rVal._regInterval = _regInterval;

            rVal._nidsAddr = _nidsAddr;
            rVal._nidsIntructPort = _nidsIntructPort;
            rVal._nidsEventPort = _nidsEventPort;
            rVal._nidsEchoPort = _nidsEchoPort;
            rVal._isFailoverDetect = _isFailoverDetect;
            rVal._presenceTag = _presenceTag;
            rVal._dialogTag = _dialogTag;
            rVal._heartbeatTag = _heartbeatTag;

            rVal._ringFile = _ringFile;
            rVal._historyFile = _historyFile;
            rVal._custUserFile = _custUserFile;

            rVal._userbtnHeight = _userbtnHeight;
            rVal._userbtnLineCnt = _userbtnLineCnt;

            rVal._ipAddress = _ipAddress;
            rVal._xmlrpcPort = _xmlrpcPort;
            rVal._tccXmlrpcServerAddr = _tccXmlrpcServerAddr;
            rVal._tccXmlrpcServerPort = _tccXmlrpcServerPort;
            
            rVal._recordingFileDir = _recordingFileDir;
            rVal._recordingDelDate = _recordingDelDate;

            return rVal;
        }

        public void Copy(SupernovaSetting setting)
        {
            _accUser = setting._accUser;
            _accPassword = setting._accPassword;
            _accDomain = setting._accDomain;
            _accProxy = setting._accProxy;
            _isRegDomain = setting._isRegDomain;
            _regInterval = setting._regInterval;

            _nidsAddr = setting._nidsAddr;
            _nidsIntructPort = setting._nidsIntructPort;
            _nidsEventPort = setting._nidsEventPort;
            _nidsEchoPort = setting._nidsEchoPort;
            _isFailoverDetect = setting._isFailoverDetect;
            _presenceTag = setting._presenceTag;
            _dialogTag = setting._dialogTag;
            _heartbeatTag = setting._heartbeatTag;

            _ringFile = setting._ringFile;
            _historyFile = setting._historyFile;
            _custUserFile = setting._custUserFile;

            _userbtnHeight = setting._userbtnHeight;
            _userbtnLineCnt = setting._userbtnLineCnt;

            _ipAddress = setting._ipAddress;
            _xmlrpcPort = setting._xmlrpcPort;
            _tccXmlrpcServerAddr = setting._tccXmlrpcServerAddr;
            _tccXmlrpcServerPort = setting._tccXmlrpcServerPort;
            
            _recordingFileDir = setting._recordingFileDir;
            _recordingDelDate = setting._recordingDelDate;
        }
    }
}
