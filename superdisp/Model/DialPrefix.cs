using System.Collections.Generic;
using System.Linq;

namespace renstech.NET.SupernovaDispatcher.Model
{
    public enum DialPrefixType
    {
        DialIntercom,
        DialIntercept,
        DialThreeway,
        DialEavesdrop,
        DialPickup,
        DialConference,
        DialPaging,
        DialFindcall,
        DialBroadcast,
        DialPark,
        DialUnpark,
        DialNoprefix,
        DialUnknown,
    }

    public enum DialPrefixGroup
    {
        DialGroupNone,
        DialGroupPublic,
        DialGroupPrivate,
        DialGroupMixed,
        DialGroupAll,
    }

    public class DialPrefix
    {
        private const string GroupPublicPrefix = "000";
        private const string GroupPrivatPrefix = "001";
        private const string GroupAllusrPrefix = "002";
        private const string GroupMixedPrefix = "004";

        private readonly Dictionary<DialPrefixType, string> _prefix = new Dictionary<DialPrefixType, string>();
        private readonly Dictionary<DialPrefixType, string> _display = new Dictionary<DialPrefixType, string>();
        private readonly Dictionary<DialPrefixGroup, string> _groupprefix = new Dictionary<DialPrefixGroup, string>();

        public DialPrefix()
        {
            _groupprefix.Add(DialPrefixGroup.DialGroupPublic, GroupPublicPrefix);
            _groupprefix.Add(DialPrefixGroup.DialGroupPrivate, GroupPrivatPrefix);
            _groupprefix.Add(DialPrefixGroup.DialGroupAll, GroupAllusrPrefix);
            _groupprefix.Add(DialPrefixGroup.DialGroupMixed, GroupMixedPrefix);
        }

        public void AddPrefix(DialPrefixType type, string prefix, string display = null)
        {
            _prefix[type] = prefix;

            if (display != null)
                _display[type] = display;
        }

        public string GetPrefix(DialPrefixType type)
        {
            if (!_prefix.ContainsKey(type))
                return null;

            return _prefix[type];
        }

        public string GetGroupPrefix(DialPrefixGroup type)
        {
            if (!_groupprefix.ContainsKey(type))
                return null;
            return _groupprefix[type];
        }

        public string GetPrefixName(DialPrefixType type)
        {
            if (!_display.ContainsKey(type))
                return null;

            return _display[type];
        }

        public bool IsGroupCallPrefix(DialPrefixType type)
        {
            return (type == DialPrefixType.DialConference || 
                type == DialPrefixType.DialPaging || 
                type == DialPrefixType.DialFindcall);
        }

        public bool IsGroupCallPrefix(string dialNum)
        {
            DialPrefixType type = GetPrefixType(dialNum);
            return IsGroupCallPrefix(type);
        }

        public DialPrefixType GetPrefixType(string num)
        {
            if (string.IsNullOrEmpty(num))
                return DialPrefixType.DialUnknown;

            foreach (KeyValuePair<DialPrefixType, string> prefix in _prefix)
            {
                if (string.Compare(prefix.Value, 0, num, 0, prefix.Value.Length) == 0)
                    return prefix.Key;
            }

            return DialPrefixType.DialNoprefix;
        }

        public string StripDialPrefix(string num)
        {
            DialPrefixType type = GetPrefixType(num);
            if (type == DialPrefixType.DialNoprefix)
                return num;

            string prefix = GetPrefix(type);

            return num.Substring(prefix.Length);
        }

        public DialPrefixGroup GetGroupPrefixType(string num)
        {
            DialPrefixType type = GetPrefixType(num);
            if (type == DialPrefixType.DialNoprefix)
                return DialPrefixGroup.DialGroupNone;

            if (!IsGroupCallPrefix(type))
                return DialPrefixGroup.DialGroupNone;

            string groupnum = StripDialPrefix(num);
            return (from prefix in _groupprefix where string.Compare(prefix.Value, 0, groupnum, 0, prefix.Value.Length) == 0 select prefix.Key).FirstOrDefault();
        }

        public string StripGroupDialPrefix(DialPrefixGroup type, string num)
        {
            string prefix = GetGroupPrefix(type);
            return num.Substring(prefix.Length);
        }

        public int GetGroupId(string num)
        {
            DialPrefixGroup type = GetGroupPrefixType(num);
            if (type == DialPrefixGroup.DialGroupNone)
                return -1;

            string id = StripDialPrefix(num);
            id = StripGroupDialPrefix(type, id);
            
            int result;
            if (int.TryParse(id, out result))
            {
                return result;
            }
            return -1;
        }

        public string GetGroupDialString(Group group)
        {
            switch (group.GroupType)
            {
                case Group.Type.AllUser:
                    return GroupAllusrPrefix;
                case Group.Type.Public:
                    return string.Format("{0}{1}", GroupPublicPrefix, group.Id);
                case Group.Type.Private:
                    return string.Format("{0}{1}", GroupPrivatPrefix, group.Id);
                case Group.Type.Mixed:
                    return string.Format("{0}{1}", GroupMixedPrefix, group.Id);
            }
            return string.Empty;
        }
    }
}
 