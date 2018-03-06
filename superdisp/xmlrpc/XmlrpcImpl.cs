using System;
using System.Windows;
using System.Windows.Threading;
using CookComputing.XmlRpc;
using renstech.NET.SupernovaDispatcher.Model;

namespace renstech.NET.SupernovaDispatcher.xmlrpc
{
    public class XmlrpcImpl : XmlRpcListenerService
    {
        public XmlrpcImpl()
        {
            CallId = -1;
        }

        public SpnvSubSystem System { get; set; }
        public int CallId { get; set; }
        public int StateCode { get; set; }
        public string RecordFileName { get; set; }
        public bool IsDisconnected { get; set; }

        [XmlRpcMethod("start_call")]
        public int StartCall(string callee, string caller)
        {
            CallId = -1;
            RecordFileName = null;
            StateCode = -1;

            int callId = -1;

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal,
                new Action(
                    delegate
                        {
                            
                            bool result = System.Channels.MakeCall(System.AccountId, callee, ref callId);
                            if (result == false)
                                callId = -1;
                        }
                ));

            CallId = callId;
            return callId;
        }

        [XmlRpcMethod("terminate_call")]
        public void TerminateCall(int callId)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal,
                new Action( () => System.SIPUAMgr.Hangup(callId) ));

            CallId = -1;
            RecordFileName = null;
            StateCode = -1;
        }
    }
}
