using CookComputing.XmlRpc;

namespace renstech.NET.nids.RemoteProc
{
    public class PARemoteProc
    {
        private readonly IPARemoteProc _proxy;

        public PARemoteProc(string addr, int port)
        {
            Addr = addr;
            Port = port;

            _proxy = (IPARemoteProc)XmlRpcProxyGen.Create<IPARemoteProc>();
            _proxy.Url = string.Format("http://{0}:{1}/", addr, port);
        }

        public string Addr { get; set; }
        public int Port { get; set; }

        public pa_section[] GetSections()
        {
            return _proxy.get_pa_sections();
        }

        public pa_zone[] GetZones(int sectionId)
        {
            return _proxy.get_pa_zones(sectionId);
        }

        public int GetPASectionIdLength()
        {
            return _proxy.get_pa_section_id_len();
        }

        public int GetPAZoneIdLength()
        {
            return _proxy.get_pa_zone_id_len();
        }

        public int GetMusicFolderIdLength()
        {
            return _proxy.get_music_folder_id_len();
        }

        public int GetMusicFileIdLength()
        {
            return _proxy.get_music_file_id_len();
        }

        public music_folder[] GetMusicFolders(int sectionId)
        {
            return _proxy.get_bkg_music_folder_list(sectionId);
        }

        public music_file[] GetMusicFiles(int sectionId, int folderId)
        {
            return _proxy.get_bkg_music_file_list(sectionId, folderId);
        }

        public cd_music_file[] GetMusicCDFiles(int sectiondId)
        {
            return _proxy.get_bkg_cd_music_list(sectiondId);
        }

        public music_channel[] getMusicChannels(int sectionId)
        {
            return _proxy.get_bkg_music_radio_channel_list(sectionId);
        }

        public bool StartFileMusic(string caller, int sectionId, string zones, int loop, int musicFolders, int musicFiles)
        {
            _proxy.start_background_file_music_by_zones(caller, sectionId, zones, loop, musicFolders, musicFiles);
            return true;
        }

        public bool StopFileMusic(string caller, int sectionId, string zones)
        {
            _proxy.stop_background_file_music_by_zones(caller, sectionId, zones);
            return true;
        }
    }
}
