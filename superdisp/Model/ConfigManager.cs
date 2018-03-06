using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using renstech.NET.SupernovaDispatcher.IPSDispatch;
using renstech.NET.SupernovaDispatcher.PADispatch;
using renstech.NET.SupernovaDispatcher.Utils;

namespace renstech.NET.SupernovaDispatcher.Model
{
    [XmlInclude(typeof(GeneralSetting))]
    [XmlInclude(typeof(SupernovaSetting))]
    [XmlInclude(typeof(PASetting))]
    [XmlInclude(typeof(IPSSetting))]
    public class SettingItem
    {
        [XmlIgnore]
        public bool IsModified { get; set; }
    }

    public class ConfigManager
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ConfigManager));

        public List<SettingItem> Items = new List<SettingItem>();

        public ConfigManager()
        {            
        }

        public static string FilePath { get; private set; }

        public static ConfigManager NewInstance(string filepath)
        {
            FilePath = filepath;

            ConfigManager mgr = null;
            try
            {
                var serializer = new XmlFileSerializer<ConfigManager>(filepath);
                mgr = serializer.Load();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            return mgr ?? (new ConfigManager());
        }

        private bool IsSettingModifed()
        {
            return Items.Any(item => item.IsModified);
        }

        public bool SaveSettings()
        {
            if (!IsSettingModifed())
                return true;

            if (string.IsNullOrEmpty(FilePath))
                return false;

            try
            {
                var serializer = new XmlFileSerializer<ConfigManager>(FilePath);
                serializer.Save(this);

                foreach (var item in Items)
                    item.IsModified = false;

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return false;
            }
        }

        public bool Contains(string name)
        {
            return Items.Any(item => item.GetType().FullName == name);
        }

        public bool SetSettingItem(SettingItem item)
        {
            SettingItem exist = GetSettingItem(item.GetType());
            if (exist == null)
            {
                Items.Add(item);                
            }
            else
            {
                int index = Items.IndexOf(exist);
                Items[index] = item;
            }
            return true;
        }

        public SettingItem GetSettingItem(Type type)
        {
            return Items.FirstOrDefault(item => item.GetType() == type);
        }

        public SettingItem GetSettingItem(string name)
        {
            return Items.FirstOrDefault(item => item.GetType().FullName == name);
        }

        public SettingItem GetSetting(Type type, bool create)
        {
            SettingItem item = GetSettingItem(type.FullName);
            if (item == null && create )
            {
                item = Activator.CreateInstance(type) as SettingItem;
                SetSettingItem(item);
            }
            return item;
        }
    }
}