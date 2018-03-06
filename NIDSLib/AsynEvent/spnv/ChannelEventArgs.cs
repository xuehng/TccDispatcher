using System;

namespace renstech.NET.nids.AsynEvent.spnv
{
    public class ChannelEventArgs : EventArgs
    {
        public ChannelEventArgs(
            ChannelEvent @event, 
            ChannelDirection direction, 
            string uuid, 
            string from,
            string to)
        {
            Event = @event;
            Direction = direction;
            UUID = uuid;
            From = from;
            To = to;
        }

        public ChannelEventArgs()
        {
        }

        public ChannelEvent Event { get;set; }
        public string UUID { get;set; }
        public ChannelDirection Direction { get;set; }
        public string From { get;set; }
        public string To { get; set; }
        public string RecordFileName { get; set; }
        public bool IsSequenceLost { get; set; }
        public bool IsServerUuidChanged { get; set; }
        public bool IsEventUuidChanged { get; set; }
    }
}