using System;

namespace renstech.NET.SupernovaDispatcher.IPSDispatch.Ldap
{
    public class PortraitDownloadedArgs : EventArgs
    {
        public PortraitDownloadedArgs(bool succeeded, string user, string filepath)
        {
            IsSucceeded = succeeded;
            User = user;
            FilePath = filepath;
        }

        public bool IsSucceeded { get; private set; }
        public string User { get; private set; }
        public string FilePath { get; private set; }
    }
}
