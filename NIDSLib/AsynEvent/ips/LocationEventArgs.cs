using System;

namespace renstech.NET.nids.AsynEvent.ips
{
    public class LocationEventArgs : EventArgs
    {
        public LocationEventArgs(ulong deviceId, uint mapId, int x, int y)
        {
            DeviceId = deviceId;
            MapId = mapId;
            X = x;
            Y = y;
        }

        public ulong DeviceId { get; private set; }
        public uint MapId { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
    }
}