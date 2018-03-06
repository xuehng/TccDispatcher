using System.Collections.Generic;
using renstech.NET.PJSIP;
using renstech.NET.SupernovaDispatcher.IPSDispatch;
using renstech.NET.SupernovaDispatcher.Interface;
using renstech.NET.SupernovaDispatcher.Model.Handset;
using renstech.NET.SupernovaDispatcher.PADispatch;

namespace renstech.NET.SupernovaDispatcher.Model
{
    public enum SubsysId
    {
        SysSupernova = 0x00001,
        SysPa = 0x00002,
        SysVideo = 0x00004,
        SysIps = 0x00008,
    }

    public enum InitResult
    {
        InitNone,
        InitOk,
        InitFail,
    }

    public abstract class Subsystem
    {
        protected Subsystem(SubsysId id, string name)
        {
            Id = id;
            Name = name;
            InitResult = InitResult.InitNone;
        }

        public SubsysId Id { get; private set; }

        public string Name { get; private set; }

        public int AccountId { set; get; }

        public InitResult InitResult { get; set; }

        public PJSIP.SIPUA SIPUAMgr { get; set; }

        public Handset.Handset LeftHandset { get; set; }

        public Handset.Handset RightHandset { get; set; }

        public ConfigManager ConfigMgr { get; set; }

        public abstract Account GetSIPAccount();

        public abstract IDispatchPage GetDispatchPage();

        public abstract bool Initialize(ref string errMsg);

        public abstract void Uninitialize();
    }

    public class SubsystemManager
    {
        private readonly List<Subsystem> _systems = new List<Subsystem>();

        public List<Subsystem> Subsystems
        {
            get { return _systems; }
        }

        public PJSIP.SIPUA SIPUA { get; set; }

        public HandsetManager HandsetMgr { get; set; }

        public ConfigManager ConfigMgr { get; set; } 

        public void Initialize(int code)
        {
            //TODO：记得给合适的系统
            //调度系统
            if ((code & ((int)SubsysId.SysSupernova)) != 0)
                Subsystems.Add(new SpnvSubSystem(SubsysId.SysSupernova, Properties.Resources.IDS_SUBSYSTEM_SUPERNOVADISPATCH));

            //安防中心
            //if ((code & ((int)SubsysId.SysSupernova)) != 0)
               // Subsystems.Add(new SpnvSubSystem(SubsysId.SysSupernova, Properties.Resources.IDS_SUBSYSTEM_SECURITY));

            if ((code & ((int)SubsysId.SysPa)) != 0)
                Subsystems.Add(new PASubSystem(SubsysId.SysPa, Properties.Resources.IDS_SUBSYSTEM_DAVINCI));

            if ((code & ((int)SubsysId.SysIps)) != 0)
                Subsystems.Add(new IPSSubsystem(SubsysId.SysIps, Properties.Resources.IDS_SUBSYSTEM_WIPOS));
        
        
            foreach (Subsystem sys in _systems)
            {
                sys.ConfigMgr = ConfigMgr;
                sys.SIPUAMgr = SIPUA;
                sys.LeftHandset = HandsetMgr.HandsetLeft;
                sys.RightHandset = HandsetMgr.HandsetRight;
            }
        }

        public Subsystem GetSpnvSubSystem()
        {
            foreach (var system in _systems)
            {
                if (system.Id == SubsysId.SysSupernova)
                    return system;
            }
            return null;
        }
    }
}