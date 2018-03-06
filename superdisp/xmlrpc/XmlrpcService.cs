using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using CookComputing.XmlRpc;
using renstech.NET.SIPUA;
using renstech.NET.SupernovaDispatcher.Model;

namespace renstech.NET.SupernovaDispatcher.xmlrpc
{
    public enum XmlrpcCommand
    {
        NotifyCallStateChanged,
        NotifyRecordFileName,
    }

    public class XmlrpcService
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(XmlrpcService));

        private readonly BlockingCollection<XmlrpcCommand> _commandQueue = new BlockingCollection<XmlrpcCommand>(10);

        private Thread _thread;
        private Thread _remoteThread;

        private readonly XmlrpcImpl _impl;

        public XmlrpcService(SpnvSubSystem system, string ipAddress, int port, string remoteAddr, int remotePort)
        {
            Address = ipAddress;
            Port = port;
            RemoteAddress = remoteAddr;
            RemotePort = remotePort;

            _impl = new XmlrpcImpl();
            _impl.System = system;
        }

        public string Address { get; private set; }
        public int Port { get; private set; }
        public string RemoteAddress { get; private set; }
        public int RemotePort { get; private set; }

        private bool ValidateCallId(int callId)
        {
            string msg = string.Format("validateCallId : {0} - {1}", 
                _impl.CallId, callId);
            Log.Debug(msg);

            if (_impl.CallId == -1)
                return false;

            if (callId != _impl.CallId)
                return false;

            return true;
        }

        private bool UpdateStateCode(sua_inv_state state, int statusCode)
        {
            _impl.IsDisconnected = false;

            int code = 0;
            switch (state)
            {
                case sua_inv_state.PJSIP_INV_STATE_EARLY:
                case sua_inv_state.PJSIP_INV_STATE_CALLING:
                case sua_inv_state.PJSIP_INV_STATE_CONNECTING:
                    code = 180;
                    break;
                case sua_inv_state.PJSIP_INV_STATE_CONFIRMED:
                    code = 200;
                    _commandQueue.Add(XmlrpcCommand.NotifyRecordFileName);
                    break;
                case sua_inv_state.PJSIP_INV_STATE_DISCONNECTED:
                    switch (statusCode)
                    {
                        case 408 :
                            code = 408;
                            break;
                        case 404 :
                            code = 404;
                            break;
                        case 486 :
                            code = 486;
                            break;
                        case 603 :
                            code = 603;
                            break;
                        default :
                            code = 800;
                            break;
                    }
                    _impl.IsDisconnected = true;
                    break;
            }

            if (code == 0 || _impl.StateCode == code)
                return false;

            _impl.StateCode = code;
            return true;
        }

        public void OnCallStateChanged(int callId, sua_inv_state state, int statusCode)
        {
            if (!ValidateCallId(callId))
            {
                return;
            }

            if (!UpdateStateCode(state, statusCode))
            {
                return;
            }

            _commandQueue.Add(XmlrpcCommand.NotifyCallStateChanged);
        }

        public void NotifyRecordFileName(int callId, string name)
        {
            if (!ValidateCallId(callId))
            {
                return;
            }

            _impl.RecordFileName = name;
        }

        public bool StartService()
        {
            Log.Debug("xmlrpc service start");

            if (string.IsNullOrEmpty(Address) || Port == 0 ||
                string.IsNullOrEmpty(RemoteAddress) || RemotePort == 0)
            {
                Log.Warn("xmlrpc service failed to start for invalid parameters");
                return false;
            }

            try
            {
                _thread = new Thread(ThreadMain)
                {
                    Name = "xmlrpcService",
                    IsBackground = true
                };

                _thread.Start();

                _remoteThread = new Thread(RemoteThreadMain)
                {
                    Name = "xmlrpcRemoteService",
                    IsBackground = true
                };

                _remoteThread.Start();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.ToString());
                return false;
            }
            return true;
        }

        public void StopService()
        {
            Log.Debug("xmlrpc service stop");

            try
            {
                if (_thread != null)
                    _thread.Abort();

                if (_remoteThread != null)
                    _remoteThread.Abort();
            }
            catch (Exception)
            {                
                return;
            }
        }

        private void ThreadMain()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(
                string.Format("http://{0}:{1}/", Address, Port));
            //当XMLRPC的IP地址填写错误时，此处抛出异常
            try
            {
                listener.Start();
            }
            catch (Exception ex)
            {
                return;
            }
            
            while (true)
            {
                try
                {
                    HttpListenerContext context = listener.GetContext();
                    _impl.ProcessRequest(context);
                }
                catch (Exception ex)
                {
                	
                }
            }     
        }


        private void RemoteThreadMain()
        {
            IXmlrpcRemote proxy = XmlRpcProxyGen.Create<IXmlrpcRemote>();
            proxy.Url = string.Format("http://{0}:{1}/",
                                      RemoteAddress, RemotePort);
            while (!_commandQueue.IsCompleted)
            {
                try
                {
                    XmlrpcCommand command = _commandQueue.Take();

                    switch (command)
                    {
                        case XmlrpcCommand.NotifyCallStateChanged:
                            try
                            {
                                proxy.NotifyCallStateChanged(_impl.CallId, _impl.StateCode);

                                if (_impl.IsDisconnected)
                                    _impl.CallId = -1;
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex.ToString());
                            }
                            break;
                        case XmlrpcCommand.NotifyRecordFileName:
                            try
                            {
                                if (!string.IsNullOrEmpty(_impl.RecordFileName))
                                    proxy.NotifyRecordFileName(_impl.CallId, _impl.RecordFileName);
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex.ToString());
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
        }
    }
}
