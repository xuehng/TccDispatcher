using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace renstech.NET.SupernovaDispatcher.PADispatch
{
    public enum BK_MUSIC_SOURCE
    {
        MUSIC_FILE,
        MUSIC_CD,
        MUSIC_RADIO,
        MUSIC_NONE,
    }

    public class BKMusicFile
    {
        public int Id { get; private set; }
        public string Name { get; private set; }

        public BKMusicFile(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    public class BKMusicFolder
    {
        private List<BKMusicFile> _files = new List<BKMusicFile>();

        public int Id { get; private set; }
        public string Name { get; private set; }

        public BKMusicFolder(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public List<BKMusicFile> Files { get { return _files; } }

        public void AddMusicFile(BKMusicFile file)
        {
            _files.Add(file);
        }

        public BKMusicFile GetMusicFile(string name)
        {
            foreach (BKMusicFile file in _files)
            {
                if (file.Name == name)
                    return file;
            }
            return null;
        }
    }

    public class BKMusicCDFile
    {
        public int Id { get; private set; }
        public string Name { get; private set; }

        public BKMusicCDFile(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    public class BKMusicRadioChannel
    {
        public int Id { get; private set; }
        public string Name { get; private set; }

        public BKMusicRadioChannel(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
