using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Novell.Directory.Ldap;

namespace renstech.NET.SupernovaDispatcher.IPSDispatch.Ldap
{
    class LDAPClient
    {
        private const string AttrJpeg = "jpegPhoto";

        private LdapConnection _connection;
        private readonly Dictionary<string, Task> _tasks = new Dictionary<string, Task>(); 

        public LDAPClient(string addr)
        {
            Addr = addr;
            Port = 389;
        }

        public string Addr { get; private set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string PortraitDir { get; set; }

        public event EventHandler<PortraitDownloadedArgs> OnPortaitDownloaded; 

        private LdapConnection GetConnection()
        {
            if (_connection== null)
                _connection = new LdapConnection();

            if (_connection.Connected)
                return _connection;

            _connection.Connect(Addr, Port);
            _connection.Bind(User, Password);

            return _connection;
        }

        public void Disconnect()
        {
            if (_connection == null)
                return;

            if (!_connection.Connected)
                return;

            _connection.Disconnect();
        }

        public sbyte[] GetLdapInfo(string num, string attr)
        {
            string filter = string.Format("telephoneNumber={0}", num);

            LdapConnection conn = GetConnection();
            LdapSearchResults result = conn.Search("dc=renstech,dc=com", LdapConnection.SCOPE_SUB, filter, null, false);
            while (result.hasMore())
            {
                LdapEntry nextEntry;
                try
                {
                    nextEntry = result.next();
                }
                catch (LdapException)
                {
                    continue;
                }

                LdapAttributeSet attributeSet = nextEntry.getAttributeSet();
                System.Collections.IEnumerator ienum = attributeSet.GetEnumerator();
                while (ienum.MoveNext())
                {
                    LdapAttribute attribute = (LdapAttribute)ienum.Current;
                    string attributeName = attribute.Name;
                    if (attributeName == attr)
                        return attribute.ByteValue;
                    //string attributeVal = attribute.StringValue;
                }                
            }
            return null;
        }

        private bool DownloadPortrait(LDAPClient client,string num)
        {
            bool result = false;

            string path = Path.Combine(PortraitDir, string.Format("{0}.jpeg", num));
            string fullpath = Path.GetFullPath(path);

            if (File.Exists(path))
            {
                if (OnPortaitDownloaded != null)
                    OnPortaitDownloaded(this, new PortraitDownloadedArgs(true, num, fullpath));

                return true;
            }

            byte[] jpeg = (byte[])(Array)client.GetLdapInfo(num, AttrJpeg);
            if (jpeg != null)
            {
                try
                {
                    File.WriteAllBytes(path, jpeg);
                    result = true;
                }
                catch (Exception ex)
                {
                }
            }

            if (OnPortaitDownloaded != null)
                OnPortaitDownloaded(this, new PortraitDownloadedArgs(result, num, fullpath));

            return true;
        }

        private void CleanTasks()
        {
            List<string> keys = (from pair in _tasks where pair.Value.IsCompleted select pair.Key).ToList();

            foreach (var key in keys)
                _tasks.Remove(key);
        }

        public bool AsynDownloadPortrait(string num)
        {
            if (string.IsNullOrEmpty(PortraitDir))
                return false;

            if (_tasks.ContainsKey(num))
                return true;
            
            CleanTasks();

            Task<bool> task = Task.Factory.StartNew<bool>(() => DownloadPortrait(this, num));
            _tasks.Add(num, task);

            return true;
        }
    }
}
