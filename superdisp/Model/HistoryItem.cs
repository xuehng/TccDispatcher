using System;

namespace renstech.NET.SupernovaDispatcher.Model
{
    [Serializable]
    public class HistoryItem
    {
        public string PartyDispName { get; set; }
        public string PartyDispNumber { get; set; }
        public string PartyNumber { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsInbound { get; set; }
        public bool IsAnswered { get; set; }
        public string FailureReason { get; set; }
    }
}