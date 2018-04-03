using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using renstech.NET.SIPUA;
using renstech.NET.SupernovaDispatcher.Control;
using renstech.NET.SupernovaDispatcher.Model;
using renstech.NET.SupernovaDispatcher.Utils;
using Application = System.Windows.Application;

namespace renstech.NET.SupernovaDispatcher.Layout
{
    /// <summary>
    /// OptionsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OptionsWindow : Window
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(OptionsWindow));

        private readonly SpnvSubSystem _system;
        private AudioPlayer _player;
        private readonly bool _adminUser;

        private readonly GeneralSetting _general;
        private readonly SupernovaSetting _supernova;

        public OptionsWindow(SpnvSubSystem system)
        {
            InitializeComponent();

            App app = Application.Current as App;   
            if (app != null && app.AppBkBrush != null)
                Background = app.AppBkBrush;

            _system = system;

            _supernova = _system.Setting.Clone() as SupernovaSetting;
            if (app != null)
                _general = app.GeneralSetting.Clone() as GeneralSetting;

            InitializeGeneralPage();
            InitializeAccountPage();
            InitializeSystemPage();
            InitializeCodecsPage();

            _adminUser = (app.LoginRole.Charater == LoginUserRole.Role.Admin);
            if (!_adminUser)
            {
                tabControl.IsEnabled = false;
            }
        }

        #region Initialize

        private void InitializeGeneralPage()
        {
            if (_general == null)
                return;

            lstSystemUser.ItemsSource = _general.Users;
            checkboxMaxWindow.DataContext = _general;
            checkboxAutoRun.DataContext = _general;
            //modbusport.ItemsSource = new List<string>{"COM1", "COM2", "COM3", "COM4", "COM5", "COM6" };
            try
            {
                _general.IsAutoRun = RegistryInfo.IsAutoRunEnabled();
                //_general.ModbusCOM = RegistryInfo.ModbusCom();
                checkboxAutoRun.IsChecked = _general.IsAutoRun;

            }
            catch (Exception)
            {                
            }
        }

        private void InitializeAccountPage()
        {
            groupAccount.DataContext = _supernova;
            groupRing.DataContext = _supernova;
            groupNids.DataContext = _supernova;
            groupXmlrpcService.DataContext = _supernova;
            groupLog.DataContext = _general;
            groupRecording.DataContext = _supernova;

            cbxRingWavs.ItemsSource = LoadRingingFiles(Properties.Settings.Default.RingingFileDir);
            cbxLogLevel.ItemsSource = LogInfo.LogLevels;

        }

        private void InitializeSystemPage()
        {
            groupGPIO.DataContext = _general;
            groupUI.DataContext = _supernova;
            groupDevices.DataContext = _general;

            List<string> inputdev = new List<string>();
            List<string> outputdev = new List<string>();

            List<Device> devices = App.SIPUA.GetAudioDevices();
            foreach (var device in devices)
            {
                if (device.InputCount > 0)
                    inputdev.Add(device.Name);

                if (device.OutputCount > 0)
                    outputdev.Add(device.Name);
            }

            cbxMainInput.ItemsSource = inputdev;
            cbxMainOutput.ItemsSource = outputdev;
            cbxLeftInput.ItemsSource = inputdev;
            cbxLeftOutput.ItemsSource = outputdev;
            cbxRightInput.ItemsSource = inputdev;
            cbxRightOutput.ItemsSource = outputdev;

            //comboxTheme.ItemsSource = ThemeInfo.ThemeList;
            ////以下代码需要消耗很多的时间，因为不需要更改主题，所以直接去掉
            //comboxTheme.SelectedItem = _general.Theme;
            
            //comboxTheme.IsEnabled = false;
        }

        private void InitializeCodecsPage()
        {
            var audioCodecs = App.SIPUA.GetAudioCodecs();
            lbxAudioCodec.ItemsSource = audioCodecs;

            var videoCodecs = App.SIPUA.GetVideoCodecs();
            lbxVideoCodec.ItemsSource = videoCodecs;
        }
        
        #endregion

        private void BtnOkClick(object sender, RoutedEventArgs e)
        {
            if (_general.IsModified)
            {
                App app = Application.Current as App;   
                if ( app != null )
                {
                    try
                    {
                        if (RegistryInfo.IsAutoRunEnabled() != _general.IsAutoRun)
                            RegistryInfo.SetAutoRun(_general.IsAutoRun);
                        //if (RegistryInfo.ModbusCom() != _general.ModbusCOM)
                            //RegistryInfo.SetModbusCom(_general.ModbusCOM);
                    }
                    catch (Exception)
                    {
                    }

                    app.GeneralSetting.Copy(_general);
                    app.GeneralSetting.IsModified = true;
                }
            }

            if (_supernova.IsRecordingFileDirChanged)
            {
                //通知手柄，改变录音文件保存路径
                App.HandsetMgr.HandsetLeft.RecordingDirChanged(_supernova.RecordingFileDir);
                App.HandsetMgr.HandsetRight.RecordingDirChanged(_supernova.RecordingFileDir);
                _supernova.IsRecordingFileDirChanged = false;
            }

            if (_supernova.IsModified)
            {
                _system.Setting.Copy(_supernova);
                _system.Setting.IsModified = true;
            }

            if (_player != null && (_player.IsPlaying || _player.IsPaused))
            {
                _player.Stop();
            }

            DialogResult = true;
        }

        private void BtnCancelClick(object sender, RoutedEventArgs e)
        {
            if (_player != null && (_player.IsPlaying || _player.IsPaused))
            {
                _player.Stop();
            }

            DialogResult = false;
        }

        private void BtnAddSysUserClick(object sender, RoutedEventArgs e)
        {
            var dialog = new CreateSystemUserWindow {Owner = this};
            if (dialog.ShowDialog() != true)
                return;

            _general.AddLoginUser(dialog.UserName, dialog.Password);
            
            lstSystemUser.Items.Refresh();
        }

        private void BtnDelSysUserClick(object sender, RoutedEventArgs e)
        {
            if (lstSystemUser.SelectedIndex == -1)
                return;

            LoginUser user = lstSystemUser.SelectedItem as LoginUser;
            if (user != null)
                _general.DelLoginUser(user.Name);

            lstSystemUser.Items.Refresh();
        }

        private static IEnumerable<string> LoadRingingFiles(string dir)
        {
            List<string> rings = new List<string>();

            string[] fileEntries = Directory.GetFiles(dir);
            foreach (string filename in fileEntries)
            {
                string[] names = filename.Split('.');

                if (names.Length != 2)
                {
                    continue;
                }

                if (string.Compare(names[1], "wav", true) != 0)
                {
                    continue;
                }

                int index = names[0].LastIndexOf('\\');

                if (index == -1)
                {
                    continue;
                }

                rings.Add(names[0].Substring(index + 1));
            }
            return rings;
        }

        private void BtnMainPlayClick(object sender, RoutedEventArgs e)
        {
            Play(_general.MainOutput, _supernova.RingFileName);
        }

        private void BtnLeftPlayClick(object sender, RoutedEventArgs e)
        {
            Play(_general.LeftOutput, _supernova.RingFileName);
        }

        private void BtnRightPlayClick(object sender, RoutedEventArgs e)
        {
            Play(_general.RightOutput, _supernova.RingFileName);
        }

        private void Play(string device, string file)
        {
            if (string.IsNullOrEmpty(device) || string.IsNullOrEmpty(file))
                return;

            if (_player == null)
                _player = new AudioPlayer();

            if (_player.IsPlaying)
                _player.Stop();

            _player.DeviceName = device;
            _player.FilePath = string.Format("rings/{0}.wav", file);
            _player.Play();             
        }

        private void ComboxThemeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string name = comboxTheme.SelectedItem as string;
            if (name == null)
                return;

            App app = Application.Current as App;
            if (app != null) app.SetTheme(name);

            _general.Theme = name;
        }

        private void BtnKeyboardClick(object sender, RoutedEventArgs e)
        {
            OnScreenKeyboard.StartOsk(this);
        }

        private void BtnEditRecordingFileDir_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = true;
            var currentPath = dialog.SelectedPath = _supernova.RecordingFileDir;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK && currentPath != dialog.SelectedPath)
            {
                _supernova.RecordingFileDir = dialog.SelectedPath;
            }    
        }

        private void BtnDelPreviousRecordings_OnClick(object sender, RoutedEventArgs e)
        {
            string content = String.Format("请确认是否删除{0}天之前的所有录音文件？", _supernova.RecordingDelDate);

            Log.Debug("BtnDelPreviousRecordings_OnClick__MessageWindow.ShowDialog BEFORE");
            
            MessageWindow dialog = new MessageWindow(
                Properties.Resources.IDS_RECORD_DELETE_ALL_TITLE,
                content,
                MessageWindow.ButtonListType.ButtonOkCancel, MessageWindow.IconType.IconWarn) { Owner = this };

            if (dialog.ShowDialog() == true)
            {
                var _recordingFiles = new RecordingFiles(_system);
                _recordingFiles.DelPreviousRecordings();
            }

            Log.Debug("BtnDelPreviousRecordings_OnClick__MessageWindow.ShowDialog AFTER");
        }
    }
}