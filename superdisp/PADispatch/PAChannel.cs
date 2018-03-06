using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using renstech.NET.nids;
using System.ComponentModel;
using renstech.NET.nids.RemoteProc;

namespace renstech.NET.SupernovaDispatcher.PADispatch
{
    internal class PAChannel : INotifyPropertyChanged
    {
        private PASubSystem _system = null;
        private bool _isBusy;
        private string _sectionName;
        private string _zoneName;

        public PAChannel(PASubSystem system)
        {
            _system = system;

            CallId = -1;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        public int CallId { get; set; }
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsBusy"));
            }
        }

        public string SectionName
        {
            get { return _sectionName; }
            set
            {
                _sectionName = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SectionName"));
            }
        }

        public string ZoneName
        {
            get { return _zoneName; }
            set
            {
                _zoneName = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ZoneName"));
            }
        }

        public bool Speech(List<PASection> targets)
        {
            if (targets == null)
                return false;

            DialRule rule = new DialRule();
            rule.ActionType = PA_ACTION_TYPE.PA_ACT_MANUAL;
            rule.Sections = targets;
            
            rule.BuildDialString();
            if (string.IsNullOrEmpty(rule.DialString))
            {
                return false;
            }

            int accId = _system.GetAccountId();
            if (accId == -1)
                return false;

            App.SIPUA.MakeCall(accId, rule.DialString, (int)_system.Id);
            return true;
        }

        public bool PlayFileMusic(List<PASection> targets, BKMusicFolder foler)
        {
            if (targets == null)
                return false;

            DialRule rule = new DialRule();
            rule.Sections = targets;
            rule.BuildDialString();

            int sectionIds;
            if (!int.TryParse(rule.SectionsId, out sectionIds))
                return false;

            int folderId = 0, filesId = 0;
            rule.GetFileMusicIds(foler, ref folderId, ref filesId);

            PARemoteProc proxy = _system.GetPARpc();
            proxy.StartFileMusic("111", sectionIds, rule.Zones, 1, folderId, filesId);
            return true;
        }

        public bool StopFileMusic(PASection secion)
        {
            return true;
        }

        public bool StopFileMusic(PAZone zone)
        {
            return true;
        }

        public bool PlayAutoSpeech(List<PASection> targets)
        {
            return false;
        }

        public bool StopAutoSpeech(List<PASection> targets)
        {
            return false;
        }

        public bool Eavesdrop(List<PASection> targets)
        {
            return false;
        }

        public bool Hangup()
        {
            if (CallId == -1)
                return false;

            App.SIPUA.Hangup(CallId);
            return true;
        }
    }
}
