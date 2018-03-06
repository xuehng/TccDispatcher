using System;
using System.Collections.Generic;
using System.Text;

namespace renstech.NET.nids.AsynEvent.pa
{
    public class PAEventSubscriber : AsynEventSubscriber
    {
        private string _eventTag;

        public event EventHandler<PAEventArgs> EventInfo;

        public string EventTag 
        {
            get { return _eventTag; }
            set { _eventTag = value;
                EventTags.Add(_eventTag);
            }
        }

        private Dictionary<string, string> ConvertToDictionary(string content)
        {
            try
            {
                Dictionary<string, string>  dict = Newtonsoft.Json.JsonConvert.
                    DeserializeObject<Dictionary<string, string>>(content);
                return dict;
            }
            catch (Exception ex)
            {
                Log(LOGLEVEL.ERROR, ex.ToString());
                return null;
            }
        }

        private string GetDictValue(Dictionary<string, string> dict,string key)
        {
            if (!dict.ContainsKey(key))
            {
                Log(LOGLEVEL.ERROR, string.Format("event msg does not contain {0}", key));
                return null;
            }
            return dict[key];
        }
    
        private static bool ParseMsg(byte[] msg, ref string eventName, ref string eventBody)
        {
            string message = Encoding.ASCII.GetString(msg);
            string[] content = message.Split('\0');
            if (content.Length < 2)
                return false;

            eventName = content[0];
            eventBody = content[1];
            return true;
        }

        protected override void OnEventReceived(byte[] msg)
        {
            string eventName = null, eventBody = null;
            if (!ParseMsg(msg, ref eventName, ref eventBody))
                return;

            Log(LOGLEVEL.DEBUG, string.Format("Receive event {0} : {1}", eventName, eventBody));

            PAEventArgs args = new PAEventArgs();

            Dictionary<string, string> dict = ConvertToDictionary(eventBody);
            if (dict == null)
                return;

            string sType = GetDictValue(dict, PAEvent.msg_pa_type);
            if (sType == null)    
                return;

            args.Type = PAEvent.GetPAType(sType);
            if (args.Type == PA_EVENT_TYPE.PA_NONE)
            {
                Log(LOGLEVEL.ERROR, string.Format("unrecognized pa type {0}", sType));
                return;
            }

            string sAct = GetDictValue(dict, PAEvent.msg_pa_act);
            if (sAct == null)
                return;

            args.Action = PAEvent.GetActionType(sType);
            if (args.Action == PA_ACTION.ACTION_NONE)
            {
                Log(LOGLEVEL.ERROR, string.Format("unrecognized pa action {0}", sAct));
                return;
            }

            args.Caller = GetDictValue(dict, PAEvent.msg_pa_caller_number);
            args.CallerDesc = GetDictValue(dict, PAEvent.msg_pa_caller_desc);
            
            string sectionId = GetDictValue(dict, PAEvent.msg_pa_section_id);
            int id;
            if (!Int32.TryParse(sectionId, out id))
            {
                Log(LOGLEVEL.ERROR, string.Format("Invalid section Id {0}", sectionId));
                return;
            }
            args.SectionId = id;

            args.SectionName = GetDictValue(dict, PAEvent.msg_pa_section_name);
            
            string zoneId = GetDictValue(dict, PAEvent.msg_pa_zone_id);
            if (!Int32.TryParse(zoneId, out id))
            {
                Log(LOGLEVEL.ERROR, string.Format("Invalid zone Id {0}", zoneId));
                return;
            }
            args.ZoneId = id;

            args.ZoneName = GetDictValue(dict, PAEvent.msg_pa_zone_name);

            if (EventInfo != null)
                EventInfo(this, args);
        }
    }
}
