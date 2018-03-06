namespace renstech.NET.SupernovaDispatcher.IPSDispatch
{
    internal class Location
    {
        public const int InvalidCoort = -1;

        public Location()
        {
            X = InvalidCoort;
            Y = InvalidCoort;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public uint MapId { get; set; }
    }
}