using CookComputing.XmlRpc;

namespace renstech.NET.nids
{
    public class pa_section
    {
        public int pa_section_id;
        public string pa_section_name;
    }

    public class pa_zone
    {
        public int pa_zone_id;
        public string pa_zone_name;
    }

    public class music_folder
    {
        public int music_folder_id;
        public string music_folder_name;
    }

    public class music_file
    {
        public int music_file_id;
        public string music_file_name;
    }

    public class cd_music_file
    {
        public int cd_music_id;
        public string cd_music_name;
    }

    public class music_channel
    {
        public int radio_channel_id;
        public string radio_channel_name;
    }

    public interface IPARemoteProc : IXmlRpcProxy
    {
        [XmlRpcMethod("pa.get_pa_sections")]
        pa_section[] get_pa_sections();

        [XmlRpcMethod("pa.get_pa_zones")]
        pa_zone[] get_pa_zones(int section_id);

        [XmlRpcMethod("pa.get_pa_section_id_len")]
        int get_pa_section_id_len();

        [XmlRpcMethod("pa.get_pa_zone_id_len")]
        int get_pa_zone_id_len();

        [XmlRpcMethod("pa.get_music_folder_id_len")]
        int get_music_folder_id_len();

        [XmlRpcMethod("pa.get_music_file_id_len")]
        int get_music_file_id_len();

        [XmlRpcMethod("pa.get_bkg_music_folder_list")]
        music_folder[] get_bkg_music_folder_list(int section_id);

        [XmlRpcMethod("pa.get_bkg_music_file_list")]
        music_file[] get_bkg_music_file_list(int section_id, int folder_id);

        [XmlRpcMethod("pa.get_bkg_cd_music_list")]
        cd_music_file[] get_bkg_cd_music_list(int section_id);

        [XmlRpcMethod("pa.get_bkg_music_radio_channel_list")]
        music_channel[] get_bkg_music_radio_channel_list(int section_id);

        [XmlRpcMethod("pa.start_background_file_music_by_zones")]
        void start_background_file_music_by_zones(string caller, int sectionId, string zones, int loop, int music_folder_id, int music_files);

        [XmlRpcMethod("pa.stop_background_file_music_by_zones")]
        void stop_background_file_music_by_zones(string caller, int sectionId, string zones);
    }
}
