using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using Modbus.Device;
using renstech.GPIO.NET;
using renstech.NET.InterProcPipe;
using renstech.NET.SIPUA;
using Modbus.IO;

namespace renstech.NET.SupernovaDispatcher.Model.Handset
{
    public class Handset
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Handset));
        private readonly HandsetProcess _handsetprocess;
        private bool _isoffhook;

        public Handset(int id, string uafile, int gpio, string recdir)
        {
            Id = id;
            GPIOPort = gpio;
            RemoteNumber = "";
 
            _handsetprocess = new HandsetProcess(id, uafile, recdir);
            _handsetprocess.MsgReceived += HandleHandsetMsg;
        }

        public event EventHandler<HandsetOnlineArgs> OnHandsetOnline;
        public event EventHandler<HandsetOffHookArgs> OnHookStateChanged;
        public event EventHandler<HandsetCallStateArgs> OnCallStateChanged;

        public int Id { get; private set; }
        public string HostAddr { get; private set; }
        public int UdpPort { get; private set; }
        public int GPIOPort { get; set; }
        //public bool IsEnabled { get { return (GPIOPort != 0); } }
        public bool IsEnabled { get { return (true); } }
        public string CaptureDevice { get; set; }
        public string PlaybakDevice { get; set; }

        public bool IsOnLine { get; set; }
        public bool IsBusy { get; set; }
        public string RemoteUri { get; set; }
        public string RemoteNumber { get; set; }
        public sua_inv_state CallState { get; set; }
        public sua_role_e CallRole { get; set; }

        public bool IsOffHook
        {
            get { return _isoffhook; }
            set
            {
                _isoffhook = value;

                if (OnHookStateChanged != null)
                    OnHookStateChanged(this, new HandsetOffHookArgs(Id, _isoffhook));
            }
        }

        public void Run()
        {
            _handsetprocess.Run();
        }

        public void Stop()
        {
            if (IsBusy)
                Hangup();

            _handsetprocess.Stop();
        }

        private void HandleHandsetMsg(object sender, HandsetMsgArgs args)
        {
            HandsetNotifyMsg notify = args.Message as HandsetNotifyMsg;
            if (notify == null)
            {
                return;
            }

            switch (notify.MsgNotifyCode)
            {
                case HANDSETMSG_NOTIFY_CODE.MSG_NOTIFY_CALLSTATE:
                    HandsetNotifyCallStateMsg statemsg = notify as HandsetNotifyCallStateMsg;
                    HandleCallStateMsg(statemsg);
                    break;
                case HANDSETMSG_NOTIFY_CODE.MSG_NOTIFY_INFO:
                    HandsetNotifyInfoMsg info = notify as HandsetNotifyInfoMsg;
                    if (info != null)
                        OnHandsetAddrInfoUpdated(info);
                    break;
                case HANDSETMSG_NOTIFY_CODE.MSG_NOTIFY_ONLINE:
                    RunSIPUA(CaptureDevice, PlaybakDevice);
                    break;
            }
        }

        private void HandleCallStateMsg(HandsetNotifyCallStateMsg msg)
        {
            if (msg == null)
                return;

            RemoteUri = msg.remoteUri;
            CallState = (sua_inv_state)msg.state;
            CallRole = (sua_role_e)msg.role;
            IsBusy = (CallState != sua_inv_state.PJSIP_INV_STATE_DISCONNECTED);

            if (OnCallStateChanged == null)
                return;

            HandsetCallStateArgs arg = new HandsetCallStateArgs(msg.remoteUri,
                (sua_role_e)msg.role, (sua_inv_state)msg.state);
            OnCallStateChanged(this, arg);
        }

        private void OnHandsetAddrInfoUpdated(HandsetNotifyInfoMsg info)
        {
            HostAddr = info.host_addr;
            UdpPort = info.udp_port;

            IsOnLine = true;
            if (OnHandsetOnline != null)
                OnHandsetOnline(this, new HandsetOnlineArgs(Id));
        }

        public bool RunSIPUA(string input, string output)
        {
            HandsetReqUAStartMsg msg = new HandsetReqUAStartMsg
                                           {
                                               CaptureDevice = App.SIPUA.GetAudioDeviceId(input),
                                               PlaybackDevice = App.SIPUA.GetAudioDeviceId(output)
                                           };

            if (_handsetprocess.SendMessage(msg))
            {
                return true;
            }

            return false;
        }

        public bool AddAccount(string user, string password, string domain, string proxy, bool isreg)
        {
            HandsetReqAddAccountMsg msg = new HandsetReqAddAccountMsg
                                              {
                                                  User = user,
                                                  Password = password,
                                                  Domain = domain,
                                                  Proxy = proxy,
                                                  IsReg = isreg,
                                                  IsAutoAnswer = true
                                              };

            if (_handsetprocess.SendMessage(msg))
                return true;

            return false;
        }

        public bool MakeCall(string dest)
        {
            //发起通话时，将RemoteNumber设置为目标号码
            RemoteNumber = dest;
            HandsetReqMakeCallMsg msg = new HandsetReqMakeCallMsg {DestUri = dest};

            if (_handsetprocess.SendMessage(msg))
                return true;

            return false;
        }

        public bool Answer(int accountId, int callId, string destNum)
        {
            //接通对方来电时，RemoteNumber为调度台号码
            if (string.IsNullOrEmpty(HostAddr) || UdpPort == 0)
                return false;

            string uri = string.Format("sip:{0}@{1}:{2}", destNum,
                HostAddr, UdpPort);
            return App.SIPUA.Redirect(accountId, callId, uri);
        }

        public bool TransferAnswer(int accountId, int callId, string destNum)
        {
            //转接时，将RemoteNumber设置为空
            RemoteNumber = string.Empty;
            if (string.IsNullOrEmpty(HostAddr) || UdpPort == 0)
                return false;

            string uri = string.Format("sip:{0}@{1}:{2}", destNum,
                HostAddr, UdpPort);
            return App.SIPUA.Xfer(accountId, callId, uri);
        }

        public bool Hangup()
        {
            //结束通话时，将RemoteNumber设置为空
            RemoteNumber = string.Empty;
            HandsetReqHangupMsg msg = new HandsetReqHangupMsg();

            if (_handsetprocess.SendMessage(msg))
                return true;

            return false;
        }

        public bool RecordingDirChanged(string newRecordingDir)
        {
            //录音文件存放路径更改后，需要通知手柄
            HandsetReqRecordingDirChangedMsg msg = new HandsetReqRecordingDirChangedMsg();
            msg.newRecordingDir = newRecordingDir;

            if (_handsetprocess.SendMessage(msg))
                return true;
            
            return false;
        }
    }

    public class HandsetManager
    {
        private GPIOInfo _gpioInst;
        private SerialPort _modbusComInst;
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Handset));
        
        public bool IsInitialized { get; private set; }
        public string SipuaExecPath { get; private set; }
        public Handset HandsetLeft { get; private set; }
        public Handset HandsetRight { get; private set; }
        public int LeftGpio { get; set; }
        public string LeftCaptureDevice { get; set; }
        public string LeftPlaybackDevice { get; set; }
        public int RightGpio { get; set; }
        public string RightCaptureDevice { get; set; }
        public string RightPlaybackDevice { get; set; }

        public string ModbusComNum { get; set; }


        public bool Initialize(string uafile,SupernovaSetting spnvSetting)
        {
            SipuaExecPath = uafile;

            if (!KillProcesses(SipuaExecPath))
            {
                return false;
            }

            HandsetLeft = new Handset(0, SipuaExecPath, LeftGpio,
                                      spnvSetting.RecordingFileDir) { CaptureDevice = LeftCaptureDevice, PlaybakDevice = LeftPlaybackDevice };
            if (HandsetLeft.IsEnabled)
            {
                HandsetLeft.Run();
            }

            HandsetRight = new Handset(1, SipuaExecPath, RightGpio,
                                       spnvSetting.RecordingFileDir) { CaptureDevice = RightCaptureDevice, PlaybakDevice = RightPlaybackDevice };
            if (HandsetRight.IsEnabled)
            {
                HandsetRight.Run();
            }

            InitGpioDetectThread();

            IsInitialized = true;
            return true;
        }

        private bool InitGpioDetectThread()
        {
            //if ((!HandsetLeft.IsEnabled && !HandsetRight.IsEnabled) ||
            // Environment.Is64BitOperatingSystem)

            if ((!HandsetLeft.IsEnabled && !HandsetRight.IsEnabled) )
            {
             Log.Debug("________________handsets disabled");
             return false;
            }

            try
            {
                Thread thread = new Thread(HandsetModbusThread) {Name = "HandsetModbusThread", IsBackground = true};
                thread.Start();

                return true;
            }
            catch (Exception)
            {
                Log.Debug("GPIO Log: InitGpioDetectThread Fail");
                return false;
            }
        }

        private void HandsetModbusThread()
        {
            Log.Debug("_____HandsetModbusThread__1");

            Log.Debug(String.Format("_____HandsetModbusThread___com___{0}", ModbusComNum));
            try
            {
                _modbusComInst = new SerialPort(ModbusComNum);
            }
            catch (Exception ex)
            {
                Log.Debug("____HandsetModbusThread.Show ex, BEFORE");
                System.Windows.MessageBox.Show(ex.Message);
                Log.Debug("____HandsetModbusThread.Show ex, AFTER");
                return;
            }
            
            using (_modbusComInst)
            {
                Log.Debug("_____HandsetModbusThread__using SerialPort COM3__BaudRate 9600 DataBits 8 Parity None StopBits One");
                _modbusComInst.BaudRate = 9600;
                _modbusComInst.DataBits = 8;
                _modbusComInst.Parity = Parity.None;
                _modbusComInst.StopBits = StopBits.One;
                _modbusComInst.Open();

                IModbusSerialMaster master = ModbusSerialMaster.CreateRtu(_modbusComInst);
                // create modbus master
                //byte slaveId = 1;
                byte slaveId = 254;
                ushort startAddress = 0;
                ushort numRegisters = 2;

                bool[] bools;

                //ushort[] registers = master.ReadInputRegisters(slaveId, startAddress, numRegisters);

                while (true)
                {
                    Log.Debug("_____HandsetModbusThread__using SerialPort COM3__ while (true)_1");
                    bools = master.ReadInputs(slaveId, startAddress, numRegisters);

                    if (HandsetLeft != null && HandsetLeft.IsEnabled)
                    {
                        Log.Debug("_____HandsetModbusThread__using SerialPort COM3__ while (true)__HandsetLeft_1");

                        bool offhook = !bools[0];

                        if (offhook != HandsetLeft.IsOffHook)
                        {
                            HandsetLeft.IsOffHook = offhook;
                        }
                        Log.Debug("_____HandsetModbusThread__using SerialPort COM3__ while (true)__HandsetLeft_2");
                    }

                    if (HandsetRight != null && HandsetRight.IsEnabled)
                    {
                        Log.Debug("_____HandsetModbusThread__using SerialPort COM3__ while (true)__HandsetRight");

                        bool offhook = !bools[1];

                        if (offhook != HandsetRight.IsOffHook)
                        {
                            HandsetRight.IsOffHook = offhook;
                        }
                        Log.Debug("_____HandsetModbusThread__using SerialPort COM3__ while (true)__HandsetRight2");
                    }

                    Log.Debug("_____HandsetModbusThread__using SerialPort COM3__ while (true)_2");
                    Thread.Sleep(500);
                }

            }
            Log.Debug("_____HandsetModbusThread__1__using SerialPort COM3");
        }

        private void HandsetStateDetectThread()
        {
            try
            {
                _gpioInst = new GPIOInfo();
                _gpioInst.Initialize();
            }
            catch (Exception ex)
            {
                Log.Debug("HandsetStateDetectThread__MessageBox.Show ex, BEFORE");
                System.Windows.MessageBox.Show(ex.Message);
                Log.Debug("HandsetStateDetectThread__MessageBox.Show ex, AFTER");
                return;
            }

            while (true)
            {
                _gpioInst.UpdateGPIO();

                if (HandsetLeft != null && HandsetLeft.IsEnabled )
                {
                    int state = _gpioInst.ReadGPIO(HandsetLeft.GPIOPort);

                    bool offhook = (state == 0);
                    if (offhook != HandsetLeft.IsOffHook)
                    {
                        HandsetLeft.IsOffHook = offhook;
                    }
                }

                if (HandsetRight != null && HandsetRight.IsEnabled )
                {
                    int state = _gpioInst.ReadGPIO(HandsetRight.GPIOPort);

                    bool offhook = (state == 0);
                    if (offhook != HandsetRight.IsOffHook)
                    {
                        HandsetRight.IsOffHook = offhook;
                    }
                }

                Thread.Sleep(500);
            }
        }

        public void UnInitialize()
        {
            if (HandsetLeft != null)
                HandsetLeft.Stop();

            if (HandsetRight != null)
                HandsetRight.Stop();

            HandsetLeft = null;
            HandsetRight = null;
            IsInitialized = false;
        }

        public Handset GetOffhookHandset()
        {
            if (HandsetLeft != null && HandsetLeft.IsOffHook)
                return HandsetLeft;

            if (HandsetRight != null && HandsetRight.IsOffHook)
                return HandsetRight;

            return null;
        }

        public Handset GetPreparedHandset()
        {
            if (HandsetLeft != null && HandsetLeft.IsOffHook && !HandsetLeft.IsBusy)
                return HandsetLeft;

            if (HandsetRight != null && HandsetRight.IsOffHook && !HandsetRight.IsBusy)
                return HandsetRight;

            return null;
        }

        private bool KillProcesses(string processname)
        {
            string name = processname;

            int index = processname.IndexOf('.');
            if (index != -1)
            {
                name = processname.Substring(0, index);
            }

            Process[] exists = Process.GetProcessesByName(name);
            foreach (Process process in exists)
            {
                process.Kill();
            }
            return true;
        }
    }
}