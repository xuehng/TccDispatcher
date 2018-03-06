using System;
using System.Diagnostics;
using Newtonsoft.Json;
using renstech.NET.InterProcPipe;

namespace renstech.NET.SupernovaDispatcher.Model.Handset
{
    public class HandsetProcess
    {
        private readonly Process _process;
        private readonly string _pipename;
        private readonly NamedPipeServer _namedPipe;
   
        public HandsetProcess(int id, string file, string recdir)
        {
            _pipename = string.Format("pipe{0}", id);
            _namedPipe = new NamedPipeServer(_pipename);

            _process = new Process
                           {
                               StartInfo =
                                   {
                                       FileName = file,
                                       Arguments = string.Format("--pipe {0} --record-dir {1} --hide",
                                                                 _pipename, recdir),
                                       UseShellExecute = false,
                                       CreateNoWindow = false,
                                   }
                           };
        }

        public EventHandler<HandsetMsgArgs> MsgReceived;

        public bool Run()
        {
            if (_process == null || _namedPipe == null )
                return false;

            _process.Start();

            _namedPipe.Start();
            _namedPipe.OnReceivedMessage += OnReceiveMsg;
            
            return true;
        }

        public void Stop()
        {
            try
            {
                _namedPipe.Stop();

                if (_process != null) 
                    _process.Kill();
            }
            catch (Exception) {
            }
        }

        private void OnReceiveMsg(object sender, ReceivedMessageEventArgs args)
        {
            HandsetMsg msg;

            HandsetNotifyMsg notify = JsonConvert.DeserializeObject<HandsetNotifyMsg>(args.Message);
            switch (notify.MsgNotifyCode)
            {
                case HANDSETMSG_NOTIFY_CODE.MSG_NOTIFY_INFO:
                    msg = JsonConvert.DeserializeObject<HandsetNotifyInfoMsg>(args.Message);
                    break;
                case HANDSETMSG_NOTIFY_CODE.MSG_NOTIFY_CALLSTATE:
                    msg = JsonConvert.DeserializeObject<HandsetNotifyCallStateMsg>(args.Message);
                    break;
                default:
                    msg = notify;
                    break;
            }
            
            if (msg != null && MsgReceived != null)
                MsgReceived(this, new HandsetMsgArgs(msg));
        }

        public bool  SendMessage(HandsetReqMsg msg)
        {
            string output = JsonConvert.SerializeObject(msg);
            if (_namedPipe != null && _namedPipe.IsConnected)
            {
                _namedPipe.Write(output);
            }
            return true;
        }
    }
}