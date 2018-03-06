using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace renstech.NET.nids
{
    public enum PRESENCE_ACTION
    {
        PRESENCE_ACTION_NONE,
        PRESENCE_ACTION_ADD,
        PRESENCE_ACTION_UPDTAE,
        PRESENCE_ACTION_DELETE,
    }

    public enum CHANNEL_EVENT
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
    }

    public enum CHANNEL_STATE
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

    public enum CHANNEL_DIRECTION
    {
        CHANNEL_DIRECTION_UNKOWN,
        CHANNEL_DIRECTION_INBOUND,
        CHANNEL_DIRECTION_OUTBOUND,
    }

    public class SigMediaEvent
    {
        private static Dictionary<string, PRESENCE_ACTION> _dictPresenceEvent = new Dictionary<string, PRESENCE_ACTION>();
        private static Dictionary<string, CHANNEL_EVENT> _dictChannelEvent = new Dictionary<string, CHANNEL_EVENT>();
        private static Dictionary<string, CHANNEL_DIRECTION> _dictDirection = new Dictionary<string, CHANNEL_DIRECTION>();
        private static Dictionary<string, CHANNEL_STATE> _dictChannelState = new Dictionary<string, CHANNEL_STATE>();
    
        static SigMediaEvent()
        {
            _dictPresenceEvent.Add("add", PRESENCE_ACTION.PRESENCE_ACTION_ADD);
            _dictPresenceEvent.Add("update", PRESENCE_ACTION.PRESENCE_ACTION_UPDTAE);
            _dictPresenceEvent.Add("delete", PRESENCE_ACTION.PRESENCE_ACTION_DELETE);

            _dictChannelEvent.Add("CHANNEL_CREATE", CHANNEL_EVENT.CHANNEL_CREATE);
            _dictChannelEvent.Add("CHANNEL_ANSWER", CHANNEL_EVENT.CHANNEL_ANSWERED);
            _dictChannelEvent.Add("CHANNEL_OUTGOING", CHANNEL_EVENT.CHANNEL_OUTGOING);
            _dictChannelEvent.Add("CHANNEL_HANGUP", CHANNEL_EVENT.CHANNEL_HANGUP);
            _dictChannelEvent.Add("CHANNEL_DESTROY", CHANNEL_EVENT.CHANNEL_DESTROY);

            _dictDirection.Add("inbound", CHANNEL_DIRECTION.CHANNEL_DIRECTION_INBOUND);
            _dictDirection.Add("outbound", CHANNEL_DIRECTION.CHANNEL_DIRECTION_OUTBOUND);

            _dictChannelState.Add("CS_NEW", CHANNEL_STATE.CS_NEW);
            _dictChannelState.Add("CS_INIT", CHANNEL_STATE.CS_INIT);
            _dictChannelState.Add("CS_ROUTING", CHANNEL_STATE.CS_ROUTING);
            _dictChannelState.Add("CS_SOFT_EXECUTE", CHANNEL_STATE.CS_SOFT_EXECUTE);
            _dictChannelState.Add("CS_EXECUTE", CHANNEL_STATE.CS_EXECUTE);
            _dictChannelState.Add("CS_EXCHANGE_MEDIA", CHANNEL_STATE.CS_EXCHANGE_MEDIA);
            _dictChannelState.Add("CS_CONSUME_MEDIA", CHANNEL_STATE.CS_CONSUME_MEDIA);
            _dictChannelState.Add("CS_HIBERNATE", CHANNEL_STATE.CS_HIBERNATE);
            _dictChannelState.Add("CS_RESET", CHANNEL_STATE.CS_RESET);
            _dictChannelState.Add("CS_HANGUP", CHANNEL_STATE.CS_HANGUP);
            _dictChannelState.Add("CS_REPORTING", CHANNEL_STATE.CS_REPORTING);
            _dictChannelState.Add("CS_DESTROY", CHANNEL_STATE.CS_DESTROY);
        }

        public static PRESENCE_ACTION GetPresenceAction(string action)
        {
            if (_dictPresenceEvent.ContainsKey(action))
                return _dictPresenceEvent[action];
            else
                return PRESENCE_ACTION.PRESENCE_ACTION_NONE;
        }

        public static CHANNEL_EVENT GetChannelEventbyChannelState(string state)
        {
            CHANNEL_STATE stateType = CHANNEL_STATE.CS_UNKNOWN;
            if (_dictChannelState.ContainsKey(state))
                stateType = _dictChannelState[state];

            if (stateType == CHANNEL_STATE.CS_UNKNOWN)
                return CHANNEL_EVENT.CHANNEL_UNKNOWN;

            switch (stateType)
            {
                case CHANNEL_STATE.CS_NEW:
                case CHANNEL_STATE.CS_INIT:
                case CHANNEL_STATE.CS_ROUTING:
                    return CHANNEL_EVENT.CHANNEL_CREATE;
                case CHANNEL_STATE.CS_SOFT_EXECUTE:
                case CHANNEL_STATE.CS_EXCHANGE_MEDIA:
                case CHANNEL_STATE.CS_EXECUTE:
                case CHANNEL_STATE.CS_PARK:
                case CHANNEL_STATE.CS_HIBERNATE:
                case CHANNEL_STATE.CS_RESET:
                    return CHANNEL_EVENT.CHANNEL_ANSWERED;
                case CHANNEL_STATE.CS_HANGUP:
                case CHANNEL_STATE.CS_REPORTING:
                    return CHANNEL_EVENT.CHANNEL_HANGUP;
                case CHANNEL_STATE.CS_DESTROY:
                    return CHANNEL_EVENT.CHANNEL_DESTROY;
            }
            return CHANNEL_EVENT.CHANNEL_UNKNOWN;
        }

        public static CHANNEL_EVENT GetChannelEvent(string eventType)
        {
            CHANNEL_EVENT channelEvent = CHANNEL_EVENT.CHANNEL_UNKNOWN;
            if (_dictChannelEvent.ContainsKey(eventType))
                channelEvent = _dictChannelEvent[eventType];
            return channelEvent;
        }

        public static CHANNEL_DIRECTION GetChannelDirection(string direction)
        {
            CHANNEL_DIRECTION direct = CHANNEL_DIRECTION.CHANNEL_DIRECTION_UNKOWN;
            if (_dictDirection.ContainsKey(direction))
                direct = _dictDirection[direction];
            return direct;
        }
    }
}
