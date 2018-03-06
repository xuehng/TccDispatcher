using System.Collections.Generic;
using System.Drawing;

namespace renstech.NET.SupernovaDispatcher.IPSDispatch
{
    internal class Map
    {
        private readonly List<Device> _devices = new List<Device>();

        public Map(int id, int version, string name, string filename)
        {
            Id = id;
            Version = version;
            Name = name;
            FileName = filename;
        }

        public int Id { get; private set; }
        public int Version { get; private set; }
        public string Name { get; private set; }
        public string FileName { get; set; }
        public Size Resolustion { get; set; }

        public List<Device> Devices
        {
            get { return _devices; }
        }

        public static bool operator ==(Map a, Map b)
        {
            if ((object) a == null && (object) b == null)
                return true;

            if ((object) a == null || (object) b == null)
                return false;

            return a.Id == b.Id && a.Version == b.Version && a.Name == b.Name &&
                   a.FileName == b.FileName;
        }

        public static bool operator !=(Map a, Map b)
        {
            return !(a == b);
        }
    }
}