using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Threading;
using renstech.NET.PJSIP;
using renstech.NET.SIPUA;
using renstech.NET.SupernovaDispatcher.Model;
using renstech.NET.SupernovaDispatcher.Model.Handset;
using LoginWindow = renstech.NET.SupernovaDispatcher.Layout.LoginWindow;

namespace renstech.NET.SupernovaDispatcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(App));
        private Mutex _myMutex;
        private GeneralSetting _generalSetting;
        
        private SupernovaSetting _spnvSetting;
        private SpnvSubSystem _spnvSubSystem;
        private RecordingFiles _recordingFiles;
        private System.Timers.Timer _delRecordingsTimer;

        public static ConfigManager ConfigManager { get; private set; }
        public static PJSIP.SIPUA SIPUA { get; private set; }
        public static HandsetManager HandsetMgr { get; private set; }
        public static LogInfo LogInfo { get; private set; }
        public LicenseInfo Licenseinfo { get; private set; }
        public SubsystemManager Systeminfo { get; private set; }
        public string LoginUser { get; set; }
        public System.Windows.Media.Brush AppBkBrush { get; set; }
        public LoginUserRole LoginRole { get; set; }

        public App()
        {
            AppBkBrush = Xceed.Wpf.Themes.Office2007.Office2007BlueResources.ApplicationBackgroundBrush;

            Systeminfo = new SubsystemManager();
            LoginRole = new LoginUserRole();

            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(path);
            
            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs ex)
            {
                if (ex == null)
                    return;
                MessageBox.Show(ex.ExceptionObject.ToString());
                Environment.Exit(1);
            };
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        private bool EnsureSingleInstance()
        {
            bool createdNew;
            try
            {
                _myMutex = new Mutex(true, "Superdisp2.0", out createdNew);
            }
            catch (Exception)
            {
                return true;
            }

            if (createdNew)
            {
                return true;
            }
            
            var current = Process.GetCurrentProcess();
            foreach (var process in Process.GetProcessesByName(current.ProcessName).Where(process => process.Id != current.Id))
            {
                SetForegroundWindow(process.MainWindowHandle);
                break;
            }
            return false;
        }

        protected override void  OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;

            if (!EnsureSingleInstance())
            {
                Shutdown();
                return;
            }

            LoadSettings();

            InitializeModules();

            Layout.MainWindow main = new Layout.MainWindow();
            SetTheme(_generalSetting.Theme);

            /*
            if (!InitializeLicense())
            {
                Shutdown();
                return;
            }*/

            Systeminfo.HandsetMgr = HandsetMgr;
            Systeminfo.SIPUA = SIPUA;
            Systeminfo.ConfigMgr = ConfigManager;
            Systeminfo.Initialize(1);
            
            LoginWindow login = new LoginWindow();
            if (login.ShowDialog() != true)
            {
                Shutdown();
                return;
            }
            
            main.Initialize();

            if (_generalSetting.MaxWindow)
            {
                main.WindowState = WindowState.Maximized;
                main.WindowStyle = WindowStyle.None;
            }
            main.Show();

            //开启定时删除录音文件的时钟
            _spnvSubSystem = (SpnvSubSystem) Systeminfo.GetSpnvSubSystem();
            if (_spnvSubSystem != null)
            {
                _recordingFiles = new RecordingFiles(_spnvSubSystem);
                _delRecordingsTimer = new System.Timers.Timer(10 * 60 * 1000);
                _delRecordingsTimer.Elapsed += DelRecordingsAtTwelve;
                _delRecordingsTimer.Start();
            }
        }

        private static void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
        }

        private static void HandleException(Exception ex)
        {
            if (ex == null)
                return;

            //ExceptionPolicy.HandleException(ex, "Default Policy");
            //MessageBox.Show(ems.Properties.Resources.UnhandledException);

            MessageBox.Show(ex.ToString());
            Environment.Exit(1);
        }

        private void DelRecordingsAtTwelve(object sender, EventArgs e)
        {
            Log.Debug("Func: DelRecordingsAtTwelve. Enter");

            var hour = DateTime.Now.Hour;
            var mins = DateTime.Now.Minute;
            //设置凌晨的3:00-3:10进行删除
            if (hour >= 3 && hour < 4 && mins < 10 && mins >= 0)
            {
                Log.Debug("Func: DelRecordingsAtTwelve. del action, begin");
                _recordingFiles.DelPreviousRecordings();
                Log.Debug("Func: DelRecordingsAtTwelve. del action, end");
            }
            Log.Debug("Func: DelRecordingsAtTwelve. Leave");
        }
        
        private bool InitializeLicense()
        {
            Licenseinfo = new LicenseInfo();
            Licenseinfo.Initialize();

            if (Licenseinfo.IsLicensed())
                return true;

            var dialog = new Layout.LicenseWindow();
            return dialog.ShowDialog() == true;            
        }

        private void LoadSettings()
        {
            ConfigManager = ConfigManager.NewInstance("dispathcer.cfg");

            _generalSetting = ConfigManager.GetSetting(typeof(GeneralSetting), true) as GeneralSetting;
            _spnvSetting = ConfigManager.GetSetting(typeof (SupernovaSetting), true) as SupernovaSetting;
        }

        public GeneralSetting GeneralSetting { get { return _generalSetting; } }

        private void InitializeModules()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            LogInfo = new LogInfo();
            LogInfo.Initialize();
            LogInfo.SetLogLevel(_generalSetting.LogLevel);

            SIPUA = new PJSIP.SIPUA();
            SIPUA.CaptureDevice = _generalSetting.MainInput;
            SIPUA.PlaybakDevice = _generalSetting.MainOutput;
            SIPUA.LogInfo += OnLog;
            SIPUA.Start();

            HandsetMgr = new HandsetManager();
            HandsetMgr.LeftGpio = _generalSetting.LeftGPIO;
            HandsetMgr.RightGpio = _generalSetting.RightGPIO;
            HandsetMgr.LeftCaptureDevice = _generalSetting.LeftInput;
            HandsetMgr.LeftPlaybackDevice = _generalSetting.LeftOutput;
            HandsetMgr.RightCaptureDevice = _generalSetting.RightInput;
            HandsetMgr.RightPlaybackDevice = _generalSetting.RightOutput;
            HandsetMgr.Initialize("LitePhone.exe",_spnvSetting);

            stopWatch.Stop();
            Log.Info(string.Format("TimeSpendCounter__InitializeModules__{0}", stopWatch.ElapsedMilliseconds));
        }

        private static void OnLog(object sender, LogEventArgs args)
        {
            Log.Debug(args.Data);
        }

        public void InitializeSipAccounts()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            foreach (var system in Systeminfo.Subsystems)
            {
                Account account = system.GetSIPAccount();
                if (account == null)
                    continue;

                SIPUA.AddAccount(account);
                system.AccountId = account.Id;

                string proxy = "";
                if (!account.IsDomainRegistration)
                    proxy = account.Proxy;

                if (HandsetMgr.HandsetLeft != null)
                {
                    HandsetMgr.HandsetLeft.AddAccount(account.User,
                        account.Password, account.Domain,
                        proxy, false);
                }

                if (HandsetMgr.HandsetRight != null)
                {
                    HandsetMgr.HandsetRight.AddAccount(account.User,
                        account.Password, account.Domain,
                        proxy, false);
                }
            }

            stopWatch.Stop();
            Log.Info(string.Format("TimeSpendCounter__InitializeSipAccounts__{0}", stopWatch.ElapsedMilliseconds));
        }

        public bool SetTheme(string name)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            System.Windows.Media.Brush backgroud = null;
            ResourceDictionary rd = ThemeInfo.GetResourceDictionary(name, ref backgroud);
            if (rd == null)
            {
                return false;
            }
            
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(rd);

            ResourceDictionary res = new ResourceDictionary
                                         {Source = new Uri(@"Dictionary\ExpressionLight.xaml", UriKind.Relative)};
            Resources.MergedDictionaries.Add(res);

            res = new ResourceDictionary { Source = new Uri(@"Dictionary\DispatcherResourceDictionary.xaml", UriKind.Relative) };
            Resources.MergedDictionaries.Add(res);

            AppBkBrush = backgroud;
            Current.MainWindow.Background = AppBkBrush;

            stopWatch.Stop();
            Log.Info(string.Format("TimeSpendCounter__SetTheme__{0}", stopWatch.ElapsedMilliseconds));

            return true;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (SIPUA != null)
                SIPUA.Stop();

            if (HandsetMgr != null)
                HandsetMgr.UnInitialize();

            foreach (Subsystem system in Systeminfo.Subsystems.Where(system => system.InitResult != InitResult.InitNone))
            {
                system.Uninitialize();
            }
        }
    }
}