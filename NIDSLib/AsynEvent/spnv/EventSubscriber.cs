#region

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Timers;
using Newtonsoft.Json;

#endregion

namespace renstech.NET.nids.AsynEvent.spnv
{
    public class EventSubscriber : AsynEventSubscriber
    {
        private string _dialogEventTag;
        private string _presenceEventTag;
        private string _heartbeatEventTag;
        private DateTime _heartbeatTime;
        private Timer _heartbeatCheckTimer;

        private Timer _dialogTimer;
        private const double DialogTimerInterval = 500;
        private const int DialogTimerCount = 15;

        public string DialogEventSequencePrevious;
        public string DialogEvnetSequenceCurrent;
        public string DialogEventEventUuidPrevious;
        public string DialogEventEventUuidCurrent;
        public string DialogEventServerUuidPrevious;
        public string DialogEventServerUuidCurrent;

        public bool Heartbeat = false;

        private static readonly Object LockObject = new object();
        private static readonly Queue<ChannelEventArgs> QueueDialog = new Queue<ChannelEventArgs>();
        private static Queue<ChannelEventArgs> DialogQueue
        {
            get
            {
                lock (LockObject)
                {
                    return QueueDialog;
                }
            }
        }

        public EventSubscriber()
        {
            _heartbeatCheckTimer = new Timer(61*1000);
            _heartbeatCheckTimer.Elapsed += HeartbeatCheck;
            _heartbeatCheckTimer.Start();

            _dialogTimer = new Timer(DialogTimerInterval);
            _dialogTimer.Elapsed += delegate(object sender, ElapsedEventArgs args)
            {
                if (DialogInfo == null)
                {
                    return;
                }
                try
                {
                    for (int i = 0; i < DialogTimerCount; i++)
                    {
                        if (DialogQueue.Count != 0 && DialogQueue.Peek() != null)
                        {
                            Log(LOGLEVEL.DEBUG, string.Format("_dialogTimer, before count: {0}", DialogQueue.Count));
                            DialogInfo(this, DialogQueue.Dequeue());
                            Log(LOGLEVEL.DEBUG, string.Format("_dialogTimer, after count: {0}", DialogQueue.Count));
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log(LOGLEVEL.ERROR, ex.ToString());
                }
            };
            _dialogTimer.Start();
        }

        private void HeartbeatCheck(object sender, EventArgs e)
        {
            if (!Heartbeat)
            {
                Log(LOGLEVEL.DEBUG, "EventSubscriber, Func: HeartbeatCheck, Heartbeat Lost.");
                Connect();
            }
            Heartbeat = false;
        }

        public string PresenceEventTag
        {
            get { return _presenceEventTag; }
            set
            {
                _presenceEventTag = value;
                EventTags.Add(_presenceEventTag);
            }
        }

        public string DialogEventTag
        {
            get { return _dialogEventTag; }
            set
            {
                _dialogEventTag = value;
                EventTags.Add(_dialogEventTag);
            }
        }

        public string HeartbeatEventTag
        {
            get { return _heartbeatEventTag; }
            set
            {
                _heartbeatEventTag = value;
                EventTags.Add(_heartbeatEventTag);
            }
        }

        public event EventHandler<PresenceArgs> PresenceInfo;
        public event EventHandler<ChannelEventArgs> DialogInfo;

        private static bool ParseMsg(byte[] msg, ref string eventName, ref string eventBody)
        {
            var message = Encoding.ASCII.GetString(msg);
            var content = message.Split('\0');
            if (content.Length < 2)
                return false;

            eventName = content[0];
            eventBody = content[1];

            return true;
        }

        protected override void OnEventReceived(byte[] msg)
        {
            string eventName = null, eventBody = null;
            if (!ParseMsg(msg, ref eventName, ref eventBody))
                return;

            //减少一些日志里面的干扰元素。因为目前只有Dialog有可能会出一些问题
            if (eventName == "dialog@pubsub.renstech.com")
                Log(LOGLEVEL.DEBUG, string.Format("Receive event {0} : {1}", eventName, eventBody));

            Dictionary<string, string> dict = null;
            try
            {
                dict = JsonConvert.
                    DeserializeObject<Dictionary<string, string>>(eventBody);
            }
            catch (Exception ex)
            {
                Log(LOGLEVEL.ERROR, ex.ToString());
            }

            if (eventName == HeartbeatEventTag)
            {
                ProcessHeartbeatEvent();
            }

            if (dict == null)
                return;

            if (eventName == DialogEventTag)
            {
                if (!ProcessDialogEvent(dict))
                {
                    Log(LOGLEVEL.DEBUG, "Func: ProcessDialogEvent, Failed");
                }
            }
            else if (eventName == PresenceEventTag)
            {
                ProcessPrecenseEvent(dict);
            }
            else
            {
                Log(LOGLEVEL.ERROR, string.Format("Unrecognized node {0}", eventName));
            }
        }

        private bool ProcessDialogEvent(Dictionary<string, string> dict)
        {
            //TODO：记得关掉，发布时。人为的随机丢包
            //if (new Random().Next(0, 20) == 0)
            //{
            //    try
            //    {
            //        Log(LOGLEVEL.DEBUG, String.Format("Func: ProcessDialogEvent, Aborted Dialog Package. sequ:{0}", dict["sequence"]));
            //    }
            //    catch (Exception)
            //    {
            //        return false;
            //    }

            //    return true;
            //}

            var arg = new ChannelEventArgs();

            #region 序号检查

            if (!dict.ContainsKey("sequence"))
            {
                Log(LOGLEVEL.DEBUG, "Func: ProcessDialogEvent, no event sequence, False Leave");
                return false;
            }
            DialogEvnetSequenceCurrent = dict["sequence"];

            //-->之前没有收到过，这是第一次收到
            if (DialogEventSequencePrevious == "-1")
            {
                DialogEventSequencePrevious = DialogEvnetSequenceCurrent;
            }
                //-->非第一次收到
            else
            {
                arg.IsSequenceLost = Convert.ToInt32(DialogEvnetSequenceCurrent) -
                                     Convert.ToInt32(DialogEventSequencePrevious) != 1;
                DialogEventSequencePrevious = DialogEvnetSequenceCurrent;
            }

            #endregion

            #region ServerUuid检查

            if (!dict.ContainsKey("server_uuid"))
            {
                Log(LOGLEVEL.DEBUG, "Func: ProcessDialogEvent, no server uuid, False Leave");
                return false;
            }
            DialogEventServerUuidCurrent = dict["server_uuid"];

            if (DialogEventServerUuidPrevious == "-1")
            {
                DialogEventServerUuidPrevious = DialogEventServerUuidCurrent;
            }
            else
            {
                if (DialogEventServerUuidPrevious != DialogEventServerUuidCurrent)
                {
                    //主备发生切换
                    arg.IsServerUuidChanged = true;
                    DialogEventServerUuidPrevious = DialogEventServerUuidCurrent;
                }
                else
                {
                    arg.IsServerUuidChanged = false;
                }
            }

            #endregion

            #region EventUuid检查

            if (!dict.ContainsKey("event_uuid"))
            {
                Log(LOGLEVEL.DEBUG, "Func: ProcessDialogEvent, no event uuid, False Leave");
                return false;
            }
            DialogEventEventUuidCurrent = dict["event_uuid"];

            if (DialogEventEventUuidPrevious == "-1")
            {
                DialogEventEventUuidPrevious = DialogEventEventUuidCurrent;
            }
            else
            {
                if (DialogEventEventUuidPrevious != DialogEventEventUuidCurrent)
                {
                    //媒体服务器发生切换
                    arg.IsEventUuidChanged = true;
                    DialogEventEventUuidPrevious = DialogEventEventUuidCurrent;
                }
                else
                {
                    arg.IsEventUuidChanged = false;
                }
            }

            #endregion

            if (!dict.ContainsKey("Event_Name"))
            {
                Log(LOGLEVEL.ERROR, "Func: ProcessDialogEvent, no Event_Name, FalseLeave");
                return false;
            }

            var channelEvent = SigMediaEvent.GetChannelEvent(dict["Event_Name"]);

            //目前，CHANNEL_ORIGINATE，会被作为无法识别，调度台不需要处理这个
            if (channelEvent == ChannelEvent.CHANNEL_UNKNOWN)
            {
                Log(LOGLEVEL.ERROR,
                    string.Format("Func: ProcessDialogEvent, Event_Name is unrecognized:{0}, False Leave",
                        dict["Event_Name"]));
                return false;
            }

            string recordFile = null;
            if (channelEvent == ChannelEvent.RECORD_START)
            {
                if (!dict.ContainsKey("variable_record_file"))
                {
                    Log(LOGLEVEL.ERROR, "Func: ProcessDialogEvent, no variable_record_file, False Leave");
                    return false;
                }

                recordFile = dict["variable_record_file"];
            }

            if (!dict.ContainsKey("Unique_ID"))
            {
                Log(LOGLEVEL.ERROR, "Func: ProcessDialogEvent, no Unique_ID, False Leave");
                return false;
            }
            var uuid = dict["Unique_ID"];

            string from;
            if (dict.ContainsKey("Caller_Caller_ID_Number"))
            {
                from = dict["Caller_Caller_ID_Number"];
            }
            else if (dict.ContainsKey("variable_sip_from_user"))
            {
                from = dict["variable_sip_from_user"];
            }
            else
            {
                Log(LOGLEVEL.ERROR, "Func: ProcessDialogEvent, no from user, False Leave");
                return false;
            }

            string to;
            if (dict.ContainsKey("Caller_Destination_Number"))
            {
                to = dict["Caller_Destination_Number"];
            }
            else if (dict.ContainsKey("variable_sip_to_user"))
            {
                to = dict["variable_sip_to_user"];
            }
            else
            {
                Log(LOGLEVEL.ERROR, "Func: ProcessDialogEvent, no to user, False Leave");
                return false;
            }

            if (!dict.ContainsKey("Call_Direction"))
            {
                Log(LOGLEVEL.ERROR, "Func: ProcessDialogEvent, no Call_Direction, False Leave");
                return false;
            }

            var direction = dict["Call_Direction"];
            var direct = SigMediaEvent.GetChannelDirection(direction);

            //根据以上信息对事件参数进行统一设置
            arg.Event = channelEvent;
            arg.Direction = direct;
            arg.UUID = uuid;
            arg.From = from;
            arg.To = to;
            arg.RecordFileName = recordFile;

            //最后把事件放入队列
            DialogQueue.Enqueue(arg);

            return true;
        }

        private void ProcessPrecenseEvent(Dictionary<string, string> dict)
        {
            if (!dict.ContainsKey("method") || !dict.ContainsKey("uri"))
            {
                Log(LOGLEVEL.ERROR, "no method or uri");
                return;
            }

            var action = SigMediaEvent.GetPresenceAction(dict["method"]);
            if (action == PresenceAction.PRESENCE_ACTION_NONE)
            {
                Log(LOGLEVEL.ERROR, "unrecognized presence action");
                return;
            }

            var uri = dict["uri"];

            var index1 = uri.IndexOf(':');
            var index2 = uri.IndexOf('@');

            if (index1 == -1 || index2 == -1)
            {
                Log(LOGLEVEL.ERROR, "uri format is incorrect");
                return;
            }

            var username = uri.Substring(index1 + 1, index2 - index1 - 1);

            var args = new PresenceArgs(action, username);
            if (PresenceInfo != null)
                PresenceInfo(this, args);
        }

        private void ProcessHeartbeatEvent()
        {
            Heartbeat = true;
        }
    }
}