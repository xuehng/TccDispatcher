using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace renstech.NET.nids
{
    public enum PA_EVENT_TYPE
    {
        PA_MANUAL,
        PA_SIGNAL,
        PA_FILE,
        PA_BK_MUSIC,
        PA_NONE,
    }

    public enum PA_ACTION
    {
        ACTION_START,
        ACTION_STOP,
        ACTION_NONE,
    }

    public class PAEvent
    {
        private static Dictionary<string, PA_EVENT_TYPE> _dictPAType = new Dictionary<string, PA_EVENT_TYPE>();
        private static Dictionary<string, PA_ACTION> _dictAction = new Dictionary<string, PA_ACTION>();

        public static readonly string msg_pa_type = "pa_type";
        public static readonly string msg_pa_act = "pa_act";
        public static readonly string msg_pa_caller_number = "pa_caller_number";
        public static readonly string msg_pa_caller_desc = "pa_caller_descrip";
        public static readonly string msg_pa_type_manual = "manual";
        public static readonly string msg_pa_type_signal = "signal";
        public static readonly string msg_pa_type_filepa = "filepa";
        public static readonly string msg_pa_type_bmusic = "backgroundMusic";
        public static readonly string msg_pa_action_start = "start";
        public static readonly string msg_pa_action_stop = "stop";
        public static readonly string msg_pa_section_id = "pa_section_id";
        public static readonly string msg_pa_section_name = "pa_section_name";
        public static readonly string msg_pa_zone_id = "pa_zone_id";
        public static readonly string msg_pa_zone_name = "pa_zone_name";

        static PAEvent()
        {
            _dictPAType.Add(msg_pa_type_manual, PA_EVENT_TYPE.PA_MANUAL);
            _dictPAType.Add(msg_pa_type_signal, PA_EVENT_TYPE.PA_SIGNAL);
            _dictPAType.Add(msg_pa_type_filepa, PA_EVENT_TYPE.PA_FILE);
            _dictPAType.Add(msg_pa_type_bmusic, PA_EVENT_TYPE.PA_BK_MUSIC);
        }

        public static PA_EVENT_TYPE GetPAType(string name)
        {
            if (_dictPAType.ContainsKey(name))
                return _dictPAType[name];
            return PA_EVENT_TYPE.PA_NONE;
        }

        public static PA_ACTION GetActionType(string name)
        {
            if (_dictAction.ContainsKey(name))
                return _dictAction[name];
            return PA_ACTION.ACTION_NONE;
        }
    }
}
