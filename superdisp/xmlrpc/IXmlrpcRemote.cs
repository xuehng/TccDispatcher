using CookComputing.XmlRpc;

namespace renstech.NET.SupernovaDispatcher.xmlrpc
{
    public interface IXmlrpcRemote : IXmlRpcProxy
    {
        [XmlRpcMethod("call_state_change")]
        void NotifyCallStateChanged(int callId, int code);

        [XmlRpcMethod("notify_record_file_name")]
        void NotifyRecordFileName(int callId, string name);
    }
}
