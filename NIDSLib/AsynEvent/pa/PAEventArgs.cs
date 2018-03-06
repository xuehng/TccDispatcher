using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace renstech.NET.nids
{
    public class PAEventArgs : EventArgs
    {
        public PA_EVENT_TYPE Type { get; set; }
        public PA_ACTION Action { get; set; }
        public string Caller { get; set; }
        public string CallerDesc { get; set; }
        public int SectionId { get; set; }
        public string SectionName { get; set; }
        public int ZoneId { get; set; }
        public string ZoneName { get; set; }
    }
}
