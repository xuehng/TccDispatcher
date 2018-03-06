using System.Collections.Generic;

namespace renstech.NET.nids.AsynEvent
{
    public enum PresenceAction
    {
        PRESENCE_ACTION_NONE,
        PRESENCE_ACTION_ADD,
        PRESENCE_ACTION_UPDTAE,
        PRESENCE_ACTION_DELETE,
    }

    public enum ChannelEvent
    {
        CHANNEL_CREATE,
        CHANNEL_ANSWERED,
        CHANNEL_OUTGOING,
        CHANNEL_ORIGINATE,
        CHANNEL_HANGUP,
        CHANNEL_DESTROY,
        CHANNEL_PARK,
        CHANNEL_UNPARK,
        CHANNEL_UNKNOWN,
        RECORD_START,
    }

    public enum ChannelState
    {
        CS_NEW,
        CS_INIT,
        CS_ROUTING,
        CS_SOFT_EXECUTE,
        CS_EXECUTE,
        CS_EXCHANGE_MEDIA,
        CS_PARK,
        CS_CONSUME_MEDIA,
        CS_HIBERNATE,
        CS_RESET,
        CS_HANGUP,
        CS_REPORTING,
        CS_DESTROY,
        CS_UNKNOWN,
    }

    public enum ChannelDirection
    {
        CHANNEL_DIRECTION_UNKOWN,
        CHANNEL_DIRECTION_INBOUND,
        CHANNEL_DIRECTION_OUTBOUND,
    }

    public class SigMediaEvent
    {
        private static readonly Dictionary<string, PresenceAction> DictPresenceEvent = new Dictionary<string, PresenceAction>();
        private static readonly Dictionary<string, ChannelEvent> DictChannelEvent = new Dictionary<string, ChannelEvent>();
        private static readonly Dictionary<string, ChannelDirection> DictDirection = new Dictionary<string, ChannelDirection>();
        private static readonly Dictionary<string, ChannelState> DictChannelState = new Dictionary<string, ChannelState>();
    
        static SigMediaEvent()
        {
            DictPresenceEvent.Add("add", PresenceAction.PRESENCE_ACTION_ADD);
            DictPresenceEvent.Add("update", PresenceAction.PRESENCE_ACTION_UPDTAE);
            DictPresenceEvent.Add("delete", PresenceAction.PRESENCE_ACTION_DELETE);

            DictChannelEvent.Add("CHANNEL_CREATE", ChannelEvent.CHANNEL_CREATE);
            DictChannelEvent.Add("CHANNEL_ANSWER", ChannelEvent.CHANNEL_ANSWERED);
            DictChannelEvent.Add("CHANNEL_OUTGOING", ChannelEvent.CHANNEL_OUTGOING);
            DictChannelEvent.Add("CHANNEL_HANGUP", ChannelEvent.CHANNEL_HANGUP);
            DictChannelEvent.Add("CHANNEL_DESTROY", ChannelEvent.CHANNEL_DESTROY);
            DictChannelEvent.Add("RECORD_START", ChannelEvent.RECORD_START);

            DictDirection.Add("inbound", ChannelDirection.CHANNEL_DIRECTION_INBOUND);
            DictDirection.Add("outbound", ChannelDirection.CHANNEL_DIRECTION_OUTBOUND);

            DictChannelState.Add("CS_NEW", ChannelState.CS_NEW);
            DictChannelState.Add("CS_INIT", ChannelState.CS_INIT);
            DictChannelState.Add("CS_ROUTING", ChannelState.CS_ROUTING);
            DictChannelState.Add("CS_SOFT_EXECUTE", ChannelState.CS_SOFT_EXECUTE);
            DictChannelState.Add("CS_EXECUTE", ChannelState.CS_EXECUTE);
            DictChannelState.Add("CS_EXCHANGE_MEDIA", ChannelState.CS_EXCHANGE_MEDIA);
            DictChannelState.Add("CS_CONSUME_MEDIA", ChannelState.CS_CONSUME_MEDIA);
            DictChannelState.Add("CS_HIBERNATE", ChannelState.CS_HIBERNATE);
            DictChannelState.Add("CS_RESET", ChannelState.CS_RESET);
            DictChannelState.Add("CS_HANGUP", ChannelState.CS_HANGUP);
            DictChannelState.Add("CS_REPORTING", ChannelState.CS_REPORTING);
            DictChannelState.Add("CS_DESTROY", ChannelState.CS_DESTROY);
        }

        public static PresenceAction GetPresenceAction(string action)
        {
            return DictPresenceEvent.ContainsKey(action) ? DictPresenceEvent[action] : PresenceAction.PRESENCE_ACTION_NONE;
        }

        public static ChannelEvent GetChannelEventbyChannelState(string state)
        {
            ChannelState stateType = ChannelState.CS_UNKNOWN;
            if (DictChannelState.ContainsKey(state))
                stateType = DictChannelState[state];

            if (stateType == ChannelState.CS_UNKNOWN)
                return ChannelEvent.CHANNEL_UNKNOWN;

            switch (stateType)
            {
                case ChannelState.CS_NEW:
                case ChannelState.CS_INIT:
                case ChannelState.CS_ROUTING:
                    return ChannelEvent.CHANNEL_CREATE;
                case ChannelState.CS_SOFT_EXECUTE:
                case ChannelState.CS_EXCHANGE_MEDIA:
                case ChannelState.CS_EXECUTE:
                case ChannelState.CS_PARK:
                case ChannelState.CS_HIBERNATE:
                case ChannelState.CS_RESET:
                    return ChannelEvent.CHANNEL_ANSWERED;
                case ChannelState.CS_HANGUP:
                case ChannelState.CS_REPORTING:
                    return ChannelEvent.CHANNEL_HANGUP;
                case ChannelState.CS_DESTROY:
                    return ChannelEvent.CHANNEL_DESTROY;
            }
            return ChannelEvent.CHANNEL_UNKNOWN;
        }

        public static ChannelEvent GetChannelEvent(string eventType)
        {
            ChannelEvent channelEvent = ChannelEvent.CHANNEL_UNKNOWN;
            if (DictChannelEvent.ContainsKey(eventType))
                channelEvent = DictChannelEvent[eventType];
            return channelEvent;
        }

        public static ChannelDirection GetChannelDirection(string direction)
        {
            ChannelDirection direct = ChannelDirection.CHANNEL_DIRECTION_UNKOWN;
            if (DictDirection.ContainsKey(direction))
                direct = DictDirection[direction];
            return direct;
        }
    }
}
