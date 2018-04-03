using System;
using System.Collections.Generic;
using System.Linq;
using renstech.NET.SupernovaDispatcher.Utils;

namespace renstech.NET.SupernovaDispatcher.Model
{
    public class LoginUser
    {
        public string Name { get; set; }
        public string Password { get; set; }

        public LoginUser()
        {
        }

        public LoginUser(string name, string password)
        {
            Name = name;
            Password = password;
        }
    }

    public class GeneralSetting : SettingItem, ICloneable
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(GeneralSetting));

        private int _leftGPIO;
        private int _righGPIO;
        private string _modbusCOM;

        private string _mainInput;
        private string _mainOutput;
        private string _leftInput;
        private string _leftOutput;
        private string _righInput;
        private string _righOutput;

        private string _logLevel;
        private string _theme;
        private bool _isWindowMax;
        private bool _isAutoStart;

        private readonly List<LoginUser> _users = new List<LoginUser>();
        public GeneralSetting()
        {
            //_logLevel = "OFF";
            //将默认的设置改为debug
            _logLevel = "DEBUG";
            _theme = ThemeInfo.Officeblue;
        }

        public List<LoginUser> Users 
        { 
            get { return _users; } 
        }

        public int LeftGPIO
        {
            get { return _leftGPIO; }
            set { IsModified = true;
                _leftGPIO = value;
            }
        }

        public int RightGPIO 
        {
            get { return _righGPIO; }
            set {
                Log.Debug("________set______RightGPIO____________");
                IsModified = true;
                _righGPIO = value;
            }  
        }

        public string ModbusComNum
        {
            get { return _modbusCOM; }
            set{
                Log.Debug("________set______ModbusCOM____________");
                IsModified = true;
                _modbusCOM = value;
            }
        }

        public string MainInput
        {
            get { return _mainInput; }
            set { IsModified = true;
                _mainInput = value;
            }
        }
        
        public string MainOutput
        {
            get { return _mainOutput; }
            set { IsModified = true;
                _mainOutput = value;
            }
        }
        
        public string LeftInput
        {
            get { return _leftInput; }
            set { IsModified = true;
                _leftInput = value;
            }
        }
        
        public string LeftOutput
        {
            get { return _leftOutput; }
            set { IsModified = true;
                _leftOutput = value;
            } 
        }
        
        public string RightInput
        {
            get { return _righInput; }
            set { IsModified = true;
                _righInput = value;
            }
        }
        
        public string RightOutput
        {
            get { return _righOutput; }
            set { IsModified = true;
                _righOutput = value;
            }
        }
        
        public string LogLevel
        {
            get { return _logLevel; }
            set { IsModified = true;
                _logLevel = value;
            }
        }
        
        public bool MaxWindow
        {
            get { return _isWindowMax; }
            set 
            { 
                IsModified = true;
                _isWindowMax = value;
            }
        }
        
        public bool IsAutoRun
        {
            get { return _isAutoStart; }
            set {
                Log.Debug("________set______IsAutoRun____________");
                IsModified = true;
                _isAutoStart = value;
            }
        }
        
        public string Theme
        {
            get { return _theme; }
            set { IsModified = true;
                _theme = value;
            }
        }

        public object Clone()
        {
            var rVal = new GeneralSetting();

            rVal._leftGPIO = _leftGPIO;
            rVal._righGPIO = _righGPIO;
            rVal._modbusCOM = _modbusCOM;

            rVal._mainInput = _mainInput;
            rVal._mainOutput = _mainOutput;
            rVal._leftInput = _leftInput;
            rVal._leftOutput = _leftOutput;
            rVal._righInput = _righInput;
            rVal._righOutput = _righOutput;

            rVal._logLevel = _logLevel;
            rVal._theme = _theme;
            rVal._isWindowMax = _isWindowMax;
            rVal._isAutoStart = _isAutoStart;

            foreach (var user in _users)
                rVal._users.Add(user);

            return rVal;
        }

        public void Copy(GeneralSetting setting)
        {
            _leftGPIO = setting._leftGPIO;
            _righGPIO = setting._righGPIO;
            _modbusCOM = setting._modbusCOM;

            _mainInput = setting._mainInput;
            _mainOutput = setting._mainOutput;
            _leftInput = setting._leftInput;
            _leftOutput = setting._leftOutput;
            _righInput = setting._righInput;
            _righOutput = setting._righOutput;

            _logLevel = setting._logLevel;
            _theme = setting._theme;
            _isWindowMax = setting._isWindowMax;
            _isAutoStart = setting._isAutoStart;

            _users.Clear();

            foreach (var user in setting._users)
                _users.Add(user);
        }

        public LoginUser AddLoginUser(string name, string password)
        {
            if (GetLoginUser(name) != null)
                return null;

            var user = new LoginUser(name, Encrypt.TripleDESEncrypt(password));
            _users.Add(user);

            IsModified = true;
            return user;
        }

        public LoginUser GetLoginUser(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            try
            {
                return _users.FirstOrDefault(user => user.Name == name);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
        }

        public void DelLoginUser(string name)
        {
            LoginUser user = GetLoginUser(name);
            if (user == null)
                return;

            if (_users.Remove(user))
                IsModified = true;
        }

        public bool ValidateLoginUser(string username, string password)
        {
            string savedPassword = null;
            if (IsAdminUser(username))
            {
                savedPassword = RegistryInfo.GetAdminUserPassword() ?? GetDefaultAdminPassword();
            }
            else
            {
                LoginUser sysUser = GetLoginUser(username);
                if (sysUser != null)
                    savedPassword = sysUser.Password;
            }

            if (savedPassword == null)
                return false;

            return ValidateUserPassword(password, savedPassword);
        }

        public string GetLastLoginUser()
        {
            return RegistryInfo.GetLastLoginUser();
        }

        public static bool ValidateUserPassword(string plain, string encoded)
        {
            if (plain == null || encoded == null)
                return false;

            return (Encrypt.TripleDESEncrypt(plain)
                == encoded);
        }

        public static bool IsAdminUser(string username)
        {
            return (username == Properties.Settings.Default.AdminUserName) ;
        }

        public static string GetDefaultAdminPassword()
        {
            return (Encrypt.TripleDESEncrypt("admin"));
        }
    }
}