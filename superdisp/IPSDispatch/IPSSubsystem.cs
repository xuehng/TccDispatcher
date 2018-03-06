using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FtpLib;
using Newtonsoft.Json;
using log4net;
using renstech.NET.PJSIP;
using renstech.NET.SIPUA;
using renstech.NET.SupernovaDispatcher.IPSDispatch.Layout;
using renstech.NET.SupernovaDispatcher.IPSDispatch.Ldap;
using renstech.NET.SupernovaDispatcher.Interface;
using renstech.NET.SupernovaDispatcher.Model;
using renstech.NET.nids;
using renstech.NET.nids.AsynEvent.ips;
using renstech.NET.nids.RemoteProc;

namespace renstech.NET.SupernovaDispatcher.IPSDispatch
{
    public class IPSSubsystem : Subsystem
    {
        private const string MapDir = "maps";
        private const string PortraitDir = "portrait";
        private const string MapListFile = "maps.list";
        private static readonly ILog Log = LogManager.GetLogger(typeof (IPSSubsystem));
        private readonly List<Device> _devices = new List<Device>();
        private EventSubscriber _eventSubscriber;
        private LDAPClient _ldapClient;
        private List<Map> _maps;

        private WiPosPage _page;
        private IPSRemoteProc _remoteProc;

        public IPSSubsystem(SubsysId id, string name) : base(id, name)
        {
            Device.DefaultPortrait = @"pack://application:,,,/SupernovaDispatcher;component/Resources/ips_device.ico";
        }

        public IPSSetting Setting { get; set; }

        internal List<Map> Maps
        {
            get { return _maps; }
        }

        public override bool Initialize(ref string errMsg)
        {
            InitResult = InitResult.InitFail;

            bool configured = ConfigMgr.Contains(typeof (IPSSetting).FullName);

            Setting = ConfigMgr.GetSetting(typeof (IPSSetting), true) as IPSSetting;
            if (Setting == null)
            {
                Log.Error("IPS Setting can not be instantiated");
                return false;
            }

            if (!configured)
            {
                Log.Error("IPS subsystem has not been configured");
                return true;
            }

            InitializeDirectory();

            if (!string.IsNullOrEmpty(Setting.ServerAddr))
                _remoteProc = new IPSRemoteProc(Setting.ServerAddr, Setting.InstructPort);

            if (!InitializeDevices())
            {
                return false;
            }

            if (!InitializeMaps())
            {
                return false;
            }

            _eventSubscriber = new EventSubscriber();
            _eventSubscriber.ServerAddr = Setting.ServerAddr;
            _eventSubscriber.ServrtPort = Setting.EventPort;
            _eventSubscriber.LocationTag = Setting.EventTag;
            _eventSubscriber.OnLocationChanged += OnLocationUpdate;
            _eventSubscriber.OnCameraStateChanged += OnCameraStateChanged;
            _eventSubscriber.StartNidsEvent();

            if (Setting.IsLDAPEnabled)
            {
                _ldapClient = new LDAPClient(Setting.LDAPServerAddr);
                _ldapClient.Port = Setting.LDAPPort;
                _ldapClient.User = Setting.LDAPUser;
                _ldapClient.Password = Setting.LDAPPassowrd;
                _ldapClient.PortraitDir = PortraitDir;
                _ldapClient.OnPortaitDownloaded += OnPortraitDownloaded;                
            }

            return true;
        }

        public void InitializeDirectory()
        {
            try
            {
                if (!Directory.Exists(MapDir))
                    Directory.CreateDirectory(MapDir);

                if (!Directory.Exists(PortraitDir))
                    Directory.CreateDirectory(PortraitDir);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        public override Account GetSIPAccount()
        {
            return null;
        }

        public override IDispatchPage GetDispatchPage()
        {
            return _page ?? (_page = new WiPosPage(this));
        }

        public override void Uninitialize()
        {
            if (_eventSubscriber != null)
                _eventSubscriber.StopNIDSEvent();

            if (_ldapClient != null)
                _ldapClient.Disconnect();
        }

        private List<Map> LoadMapListFromServer()
        {
            if (_remoteProc == null)
                return null;

            MapDesc[] maps = _remoteProc.GetMapsDesc();
            if (maps == null || !maps.Any())
                return null;

            var maplist = new List<Map>();
            foreach (MapDesc mapDesc in maps)
            {
                maplist.Add(new Map(mapDesc.id, mapDesc.map_ver, mapDesc.map_name, mapDesc.map_filename));
            }
            return maplist;
        }

        private void SaveMapList2File()
        {
            string val = JsonConvert.SerializeObject(_maps);

            try
            {
                using (FileStream fs = File.Open(MapListFile, FileMode.Create))
                {
                    Byte[] info = new UTF8Encoding(true).GetBytes(val);
                    fs.Write(info, 0, info.Length);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private List<Map> LoadMapListFromFile()
        {
            if (!File.Exists(MapListFile))
                return null;

            try
            {
                using (StreamReader sr = File.OpenText(MapListFile))
                {
                    string val = sr.ReadToEnd();
                    return JsonConvert.DeserializeObject(val, typeof (List<Map>)) as List<Map>;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return null;
            }
        }

        private bool DownloadMapFiles()
        {
            int port = _remoteProc.GetFtpPort();

            using (var ftp = new FtpConnection(Setting.ServerAddr, port, "anonymous", "password"))
            {
                ftp.Open();
                ftp.Login();
                ftp.SetCurrentDirectory(MapDir);

                foreach (Map map in _maps)
                    ftp.GetFile(map.FileName, Path.Combine(MapDir, map.FileName), false);
            }
            return true;
        }

        private bool CompareMapList(IEnumerable<Map> remote, IEnumerable<Map> local)
        {
            if (remote == null && local == null)
                return false;

            if (remote == null && local != null)
                return false;

            if (remote != null && local == null)
                return true;

            IEnumerable<Map> diff = remote.Where(x => local.Any(x1 => x1 == x));
            return (diff.Count() == remote.Count());
        }

        private bool InitializeMaps()
        {
            List<Map> remote;
            try
            {
	            remote = LoadMapListFromServer();
            }
            catch (Exception ex)
            {
            	Log.Error(ex.ToString());
                return false;
            }

            List<Map> local;
            try
            {
            	local = LoadMapListFromFile();
            }
            catch (Exception ex)
            {
            	Log.Error(ex.ToString());
                return false;
            }

            if (CompareMapList(remote, local))
            {
                _maps = remote;
                DownloadMapFiles();
                SaveMapList2File();
            }
            else
            {
                _maps = local;
            }

            foreach (Map map in _maps)
                map.FileName = @"maps\" + map.FileName;

            return true;
        }

        private bool InitializeDevices()
        {
            try
            {
                if (_remoteProc == null)
                    return false;

                DeviceTag[] tags = _remoteProc.GetDeviceTags();
                foreach (var deviceTag in tags)
                {
                    Device device = new Device(Convert.ToUInt64(deviceTag.mac_addr), 
                        deviceTag.description, deviceTag.extension);
                    _devices.Add(device);
                }
                return true;
            }
            catch (Exception ex)
            {   
                Log.Error(ex.ToString());
                return false;
            }
        }

        internal Device GetDevice(ulong deviceId)
        {
            return _devices.FirstOrDefault(device => device.MacAddr == deviceId);
        }

        internal Device GetDevice(string num)
        {
            return _devices.FirstOrDefault(device => device.Number == num);
        }

        internal List<Device> GetMapDevices(int mapId)
        {
            return _devices.Where(device => device.Location != null && device.Location.MapId == mapId).ToList();
        }

        internal Map GetMap(uint id)
        {
            return _maps.FirstOrDefault(map => map.Id == id);
        }

        private void OnLocationUpdate(object sender, LocationEventArgs args)
        {
            Device dev = GetDevice(args.DeviceId);
            if (dev == null)
            {
                dev = new Device(args.DeviceId, "untitled", "");

                Map map = GetMap(args.MapId);
                if (map != null)
                    map.Devices.Add(dev);

                _devices.Add(dev);
            }

            dev.Location.MapId = args.MapId;
            dev.Location.X = args.X;
            dev.Location.Y = args.Y;

            if (_page == null)
                return;

            Action action = () => _page.UpdateDeviceLocation(dev);
            _page.Dispatcher.BeginInvoke(action);

            if (_ldapClient != null && !string.IsNullOrEmpty(dev.Number))
                _ldapClient.AsynDownloadPortrait(dev.Number);
        }

        private void OnCameraStateChanged(object sender, CameraStateArgs args)
        {
            CameraState.CameraType type = args.CamType == CameraStateArgs.CameraType.FrontCamera
                                              ? CameraState.CameraType.FrontCamera
                                              : CameraState.CameraType.BackCamera;
            CameraState state = new CameraState(type, args.IsCameraOn, args.FaceAngle, args.TiltAngle);

            Device device = GetDevice(args.DeviceId);
            if (device == null)
                return;

            device.CameraSate = state;

            Action action = () => _page.UpdateCameraState(device);
            _page.Dispatcher.BeginInvoke(action);
        }

        private void OnPortraitDownloaded(object sender, PortraitDownloadedArgs args)
        {
            if (!args.IsSucceeded)
                return;

            Action action = () => _page.UpdatePortrait(args.User, args.FilePath);
            _page.Dispatcher.BeginInvoke(action);
        }
    }
}