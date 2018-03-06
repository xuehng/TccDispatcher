using LumiSoft.Net;

namespace renstech.NET.PJSIP
{
    public class SIPUri
    {
        private SIP_Uri _uri;

        public SIPUri()
        {
            _uri = new SIP_Uri();
        }

        public SIPUri(string uri)
        {
            IsParsedError = false;

            try
            {
                _uri = SIP_Uri.Parse(uri);
            }
            catch (System.Exception ex)
            {
                IsParsedError = true;
            }
        }

        public bool IsParsedError { get; set; }
        public string Schema { get; set; }
        
        public string User 
        {
            get { return _uri.User; }
            set { _uri.User = value; } 
        }

        public string Host 
        {
            get { return _uri.Host; }
            set { _uri.Host = value;} 
        }

        public string Uri
        {
            get { return _uri.ToString(); }
        }
    }
}
