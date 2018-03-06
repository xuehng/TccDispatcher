using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace renstech.NET.SupernovaDispatcher.PADispatch
{
    internal enum PA_ACTION_TYPE
    {
        PA_ACT_MANUAL,
        PA_ACT_MUSIC,
        PA_ACT_AUTO,
        PA_ACT_EAVESDROP,
        PA_ACT_NONE,
    }

    internal class DialRule
    {
        public static string DialPrefix { get; set; }
        public static int SectionLength { get; set; }
        public static int ZoneLength { get; set; }
        public static int MusicFolderLength { get; set; }
        public static int MusicFileLength { get; set; }

        public PA_ACTION_TYPE ActionType { get; set; }
        public bool StartAction { get; set; }
        public int LoopCount { get; set; }
        public int GroupId { get; set; }
        public List<PASection> Sections { get; set; }

        public string SectionsId { get; private set; }
        public string Zones { get; private set; }
        public string DialString { get; private set; }

        static DialRule()
        {
            SectionLength = 0;
            ZoneLength = 0;
        }

        public DialRule()
        {
            StartAction = true;
            LoopCount = 1;
            GroupId = 0;
            ActionType = PA_ACTION_TYPE.PA_ACT_NONE;
        }

        private PA_ACTION_TYPE GetActionType(string type)
        {
            if (type == "1")
                return PA_ACTION_TYPE.PA_ACT_MANUAL;
            if (type == "2")
                return PA_ACTION_TYPE.PA_ACT_MUSIC;
            if (type == "3")
                return PA_ACTION_TYPE.PA_ACT_AUTO;
            if (type == "4")
                return PA_ACTION_TYPE.PA_ACT_EAVESDROP;
            return PA_ACTION_TYPE.PA_ACT_NONE;
        }

        private bool ParseZones(string zones)
        {
            return true;
        }

        private bool ParseSections(string sections, string zones)
        {
            int sectionIds;
            if (!Int32.TryParse(sections, out sectionIds))
                return false;

            return true;
        }

        public bool Parse(string number)
        {
            string dialstr = number;

            if (string.IsNullOrEmpty(dialstr))
                return false;

            string sType = dialstr.Substring(0, 1);
            dialstr = dialstr.Remove(0, 1);

            ActionType = GetActionType(sType);
            if (ActionType == PA_ACTION_TYPE.PA_ACT_NONE)
                return false;

            dialstr = dialstr.Remove(0, 1);

            string sLoop = dialstr.Substring(0, 1);
            dialstr = dialstr.Remove(0, 1);

            int loop;
            if (!Int32.TryParse(sLoop, out loop))
                return false;

            LoopCount = loop;

            string sGroupId = dialstr.Substring(0, 1);
            dialstr = dialstr.Remove(0, 1);

            string section = dialstr.Substring(0, SectionLength);
            dialstr = dialstr.Remove(0, SectionLength);

            string zones = dialstr;

            if (!ParseSections(section, zones))
                return false;

            return true;
        }

        public bool BuildDialString()
        {
            if (string.IsNullOrEmpty(DialPrefix))
                return false;

            if (ActionType == PA_ACTION_TYPE.PA_ACT_NONE)
                return false;

            if (Sections == null)
                return false;

            string dialstring = DialPrefix;

            switch (ActionType)
            {
                case PA_ACTION_TYPE.PA_ACT_MANUAL:
                    dialstring += "11";
                    break;
                case PA_ACTION_TYPE.PA_ACT_MUSIC:
                    dialstring += "2";
                    break;
                case PA_ACTION_TYPE.PA_ACT_AUTO:
                    dialstring += "3";
                    break;
                case PA_ACTION_TYPE.PA_ACT_EAVESDROP:
                    dialstring += "4";
                    break;
                default:
                    return false;
            }

            dialstring += LoopCount.ToString();
            dialstring += GroupId.ToString();

            string section = null, zones = null;
            BuildSectionString(ref section, ref zones);

            if (section == null || zones == null)
                return false;

            SectionsId = section;
            Zones = zones;

            dialstring += section;
            dialstring += zones;
            DialString = dialstring;
            return true;
        }

        private PASection PopMiniIdSection()
        {
            PASection min = null;
            foreach (PASection section in Sections)
            {
                if (min == null)
                {
                    min = section;
                    continue;
                }

                if (min.Id > section.Id)
                    min = section;
            }

            if (min != null)
                Sections.Remove(min);

            return min;
        }

        private string GetPlaceHolderString(int count)
        {
            string placeholder = null;
            for (int i = 0; i < count; i++)
            {
                placeholder += "0";
            }
            return placeholder;    
        }

        private bool BuildSectionString(ref string sections, ref string zones)
        {
            int secIds = 0;
            GetSecionZoneIds(ref secIds, ref zones);

            string placeholder = GetPlaceHolderString(SectionLength);
            string sformat = string.Format("{{0:{0}}}", placeholder);
            sections = string.Format(sformat, secIds);

            return true;
        }

        private string BuildZoneString(List<PAZone> zones)
        {
            int id = 0;
            foreach (PAZone zone in zones)
            {
                id |= zone.Id;
            }

            string placeholder = GetPlaceHolderString(ZoneLength);
            string sformat = string.Format("{{0:{0}}}", placeholder);
            return string.Format(sformat, id);
        }

        private bool GetSecionZoneIds(ref int sectionIds, ref string zones)
        {
            sectionIds = 0;
            while (true)
            {
                PASection min = PopMiniIdSection();
                if (min == null)
                    break;

                sectionIds |= min.Id;

                zones += BuildZoneString(min.PAZones);
            }
            return true;
        }

        public bool GetFileMusicIds(BKMusicFolder folder, ref int folderId, ref int fileId)
        {
            folderId = folder.Id;

            foreach (BKMusicFile file in folder.Files)
            {
                fileId |= file.Id;
            }
            return true;
        }
    }
}
