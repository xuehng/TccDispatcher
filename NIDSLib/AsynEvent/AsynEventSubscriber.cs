using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace renstech.NET.nids.AsynEvent
{
    public abstract class AsynEventSubscriber
    {
        private Thread _thread;
        private ManualResetEvent _event;
        protected List<string> EventTags = new List<string>();
        private ZMQ.Context _context;
        private ZMQ.Socket _subscriber;

        public string ServerAddr { get; set; }
        public int ServrtPort { get; set; }
        public Encoding MsgEncoding { get; set; }
        public bool IsDetectServerSwitchover { get; set; }
        public int EchoPort { get; set; }
        public int EventIdleTime { get; set; }

        public int RecvExpire { get; set; }
        public LOGLEVEL LogLevel { get; set; }
        public event EventHandler<LogArgs> LogInfo;

        protected AsynEventSubscriber()
        {
            RecvExpire = 2000;
            MsgEncoding = Encoding.ASCII;
            IsDetectServerSwitchover = false;
            EventIdleTime = 30;
            LogLevel = LOGLEVEL.DEBUG;
        }

        protected void Log(LOGLEVEL level, string info)
        {
            if (level < LogLevel)
                return;

            if (LogInfo != null)
                LogInfo(this, new LogArgs(level, info));
        }

        public bool StartNidsEvent()
        {
            if (string.IsNullOrEmpty(ServerAddr) || ServrtPort == 0)
            {
                Log(LOGLEVEL.ERROR, "incorrect server parameters!");
                return false;
            }

            _context = new ZMQ.Context(1);
            _subscriber = _context.Socket(ZMQ.SocketType.SUB);
            Connect();

            if (EventTags.Count == 0)
            {
                Log(LOGLEVEL.ERROR, "event node can not be 0!");
                return false;
            }

            try
            {
                _event = new ManualResetEvent(false);
                _thread = new Thread(EventRecvThreadMain)
                              {
                                  Name = string.Format("nids_event_thread_{0}_{1}", ServerAddr, ServrtPort),
                                  IsBackground = true
                              };
                _thread.Start();
            }
            catch (Exception ex)
            {
                Log(LOGLEVEL.ERROR, ex.ToString());
                return false;
            }
            return true;
        }

        public void StopNIDSEvent()
        {
            if (_event != null)
            {
                _event.Set();
            }

            if (_thread == null)
            {
                return;
            }

            if (_thread.Join(3000))
            {
                _thread.Abort();
                _thread = null;
            }

            //将ZMQ的内容，销毁
            foreach (var tag in EventTags)
            {
                _subscriber.Unsubscribe(tag, MsgEncoding);
            }
            _subscriber.Dispose();
            _context.Dispose();
        }
        
        private void EventRecvThreadMain()
        {
            _event.Reset();

            RecvEventsLoop();
        }

        protected abstract void OnEventReceived(byte[] msg);

        protected void Connect()
        {
            string address = string.Format("tcp://{0}:{1}", ServerAddr, ServrtPort);
            _subscriber.Connect(address);

            Log(LOGLEVEL.DEBUG, String.Format("AsynEventSubscriber, Func: Connect, address:{0}",address));

            foreach (var tag in EventTags)
            {
                Log(LOGLEVEL.DEBUG, string.Format("AsynEventSubscriber, Func: Connect, ZMQ subscribe node {0}", tag));

                _subscriber.Subscribe(tag, MsgEncoding);
            }
        }

        private void RecvEventsLoop()
        {
            while (true)
            {
                byte[] msg = _subscriber.Recv(RecvExpire);
                if (msg != null)
                {
                    OnEventReceived(msg);
                }

                if (_event.WaitOne(0))
                {
                    break;
                }
            }
        }
    }
}
