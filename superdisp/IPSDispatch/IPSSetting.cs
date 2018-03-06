using System;
using renstech.NET.SupernovaDispatcher.Model;

namespace renstech.NET.SupernovaDispatcher.IPSDispatch
{
    public class IPSSetting : SettingItem, ICloneable
    {
        private int _eventPort;
        private int _instructPort;
        private string _serverAddr;
        private bool _isLdapEnabled;
        private string _ldapAddr;
        private int _ldapPort;
        private string _ldapUser;
        private string _ldapPassword;

        public IPSSetting()
        {
            _instructPort = 7180;
            _eventPort = 5655;
            _ldapPort = 389;
            EventTag = "ips@pubsub.renstech.com";
        }

        public string ServerAddr
        {
            get { return _serverAddr; }
            set { IsModified = true;
                _serverAddr = value;
            }
        }

        public int InstructPort
        {
            get { return _instructPort; }
            set
            {
                IsModified = true;
                _instructPort = value;
            }
        }

        public int EventPort
        {
            get { return _eventPort; }
            set
            {
                IsModified = true;
                _eventPort = value;
            }
        }

        public string EventTag { get; set; }

        public bool IsLDAPEnabled
        {
            get { return _isLdapEnabled; }
            set { IsModified = true;
                _isLdapEnabled = value;
            }
        }

        public string LDAPServerAddr
        {
            get { return _ldapAddr; }
            set
            {
                IsModified = true;
                _ldapAddr = value;
            }
        }

        public int LDAPPort
        {
            get { return _ldapPort; }
            set { IsModified = true;
                _ldapPort = value;
            }
        }

        public string LDAPUser
        {
            get { return _ldapUser; }
            set { IsModified = true;
                _ldapUser = value;
            }
        }

        public string LDAPPassowrd
        {
            get { return _ldapPassword; }
            set { IsModified = true;
                _ldapPassword = value;
            }
        }

        #region ICloneable Members

        public object Clone()
        {
            var rVal = new IPSSetting();
            rVal._serverAddr = _serverAddr;
            rVal._instructPort = _instructPort;
            rVal._eventPort = _eventPort;
            rVal._isLdapEnabled = _isLdapEnabled;
            rVal._ldapAddr = _ldapAddr;
            rVal._ldapPort = _ldapPort;
            rVal._ldapUser = _ldapUser;
            rVal._ldapPassword = _ldapPassword;
            return rVal;
        }

        #endregion

        public void Copy(IPSSetting setting)
        {
            _serverAddr = setting.ServerAddr;
            _instructPort = setting.InstructPort;
            _eventPort = setting.EventPort;
            _isLdapEnabled = setting.IsLDAPEnabled;
            _ldapPort = setting.LDAPPort;
            _ldapAddr = setting.LDAPServerAddr;
            _ldapUser = setting.LDAPUser;
            _ldapPassword = setting.LDAPPassowrd;
        }
    }
}