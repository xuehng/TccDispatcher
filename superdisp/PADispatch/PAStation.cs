using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace renstech.NET.SupernovaDispatcher.PADispatch
{
    public class PAZone : INotifyPropertyChanged
    {
        private bool _isAction;

        public int Id { get; private set; }
        public int SectionId { get; set; }
        public string Name { get; private set; }

        public PAZone(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        public bool IsAction
        {
            get { return _isAction; }
            set
            {
                _isAction = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsAction"));
            }
        }
    }

    public class PASection
    {
        private List<PAZone> _zones = new List<PAZone>();
        private List<BKMusicFolder> _musics = new List<BKMusicFolder>();
        private List<BKMusicCDFile> _cdfiles = new List<BKMusicCDFile>();
        private List<BKMusicRadioChannel> _radioChannels = new List<BKMusicRadioChannel>();

        public string Name { get; private set; }
        public int Id { get; private set; }

        public PASection(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public List<PAZone> PAZones { get { return _zones; } }
        public List<BKMusicFolder> MusicFiles { get { return _musics; } }
        public List<BKMusicCDFile> MusicCDFiles { get { return _cdfiles; } }
        public List<BKMusicRadioChannel> MusicRadioChannels { get { return _radioChannels; } }

        public bool AddPAZone(PAZone zone)
        {
            zone.SectionId = Id;
            _zones.Add(zone);
            return true;
        }

        public bool AddBKMusic(BKMusicFolder folder)
        {
            _musics.Add(folder);
            return true;
        }

        public List<List<object>> GetZoneLines(int lineItemCount)
        {
            if (lineItemCount <= 0)
                return null;

            int lineCount = _zones.Count / lineItemCount;
                
            List<List<object>> lines = new List<List<object>>();
            for (int i = 0; i < lineCount; i++)
            {
                List<object> line = new List<object>();
                for (int j = i * lineItemCount; j < i * lineItemCount + lineItemCount; j++)
                {
                    line.Add(_zones[j]);
                }
                lines.Add(line);
            }

            if (_zones.Count % lineItemCount != 0)
            {
                List<object> line = new List<object>();
                for (int i = lineCount * lineItemCount; i < _zones.Count; i++)
                {
                    line.Add(_zones[i]);
                }
                lines.Add(line);
            }
            return lines;
        }

        public BKMusicFolder GetMusicFolderByName(string name)
        {
            foreach (BKMusicFolder folder in MusicFiles)
            {
                if (folder.Name == name)
                    return folder;
            }
            return null;
        }
    }
}
