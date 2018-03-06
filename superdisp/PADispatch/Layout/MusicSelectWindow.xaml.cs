using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;

namespace renstech.NET.SupernovaDispatcher.PADispatch
{
    /// <summary>
    /// Interaction logic for MusicSelectWindow.xaml
    /// </summary>
    public partial class MusicSelectWindow : Window
    {
        private PASubSystem _system;
        private PASection _section = null;
        private PAZone _zone = null;

        public MusicSelectWindow(PASubSystem system, PASection section, PAZone zone)
        {
            InitializeComponent();

            App app = App.Current as App;
            if (app.AppBkBrush != null)
                Background = app.AppBkBrush;

            _system = system;
            _section = section;
            _zone = zone;
        }

        public BK_MUSIC_SOURCE MusicSource { get; set; }
        public BKMusicFolder MusicFolder { get; set; }
        public BKMusicCDFile MusicCDFiles { get; set; }
        public BKMusicRadioChannel MusicChannel { get; set; }

        private PASection GetPASection()
        {
            if (_section != null)
                return _section;

            if (_zone != null)
            {
                return _system.GetPASectionById(_zone.SectionId);
            }

            return null;
        }

        private void btnFile_Checked(object sender, RoutedEventArgs e)
        {
            btnCD.IsChecked = false;
            btnRadio.IsChecked = false;

            PASection section = GetPASection();
            if (section == null)
            {
                lstFolders.ItemsSource = null;
                lstFiles.ItemsSource = null;
                return;
            }

            List<string> foldersName = new List<string>();
            foreach (BKMusicFolder folder in section.MusicFiles)
            {
                foldersName.Add(folder.Name);
            }

            lstFolders.ItemsSource = foldersName;
        }

        private void btnCD_Checked(object sender, RoutedEventArgs e)
        {
            btnFile.IsChecked = false;
            btnRadio.IsChecked = false;

            PASection section = GetPASection();
            if (section == null)
            {
                lstFolders.ItemsSource = null;
                lstFiles.ItemsSource = null;
                return;
            }

            List<string> files = new List<string>();
            foreach (BKMusicCDFile file in section.MusicCDFiles)
            {
                files.Add(file.Name);
            }

            lstFolders.ItemsSource = null;
            lstFiles.ItemsSource = files;
        }

        private void btnRadio_Checked(object sender, RoutedEventArgs e)
        {
            btnFile.IsChecked = false;
            btnCD.IsChecked = false;

            PASection section = GetPASection();
            if (section == null)
            {
                lstFolders.ItemsSource = null;
                lstFiles.ItemsSource = null;
                return;
            }

            List<string> files = new List<string>();
            foreach (BKMusicRadioChannel channel in section.MusicRadioChannels)
            {
                files.Add(channel.Name);
            }

            lstFolders.ItemsSource = files;
            lstFiles.ItemsSource = null;
        }

        private void lstFolders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (btnFile.IsChecked != true)
                return;

            string name = lstFolders.SelectedItem as string;
            Debug.Assert(name != null);

            PASection section = GetPASection();
            if (section == null)
            {
                lstFiles.ItemsSource = null;
                return;
            }

            BKMusicFolder folder = section.GetMusicFolderByName(name);
            if (folder == null)
            {
                lstFiles.ItemsSource = null;
                return;
            }

            List<string> files = new List<string>();
            foreach (BKMusicFile file in folder.Files)
            {
                files.Add(file.Name);
            }

            lstFiles.ItemsSource = files;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (btnFile.IsChecked == true)
                MusicSource = BK_MUSIC_SOURCE.MUSIC_FILE;
            else if (btnCD.IsChecked == true)
                MusicSource = BK_MUSIC_SOURCE.MUSIC_CD;
            else if (btnRadio.IsChecked == true)
                MusicSource = BK_MUSIC_SOURCE.MUSIC_RADIO;
            else
                MusicSource = BK_MUSIC_SOURCE.MUSIC_NONE;


            switch (MusicSource)
            {
                case BK_MUSIC_SOURCE.MUSIC_FILE:
                    SetMusicFiles();
                    break;
                case BK_MUSIC_SOURCE.MUSIC_CD:
                    SetMusicCDFiles();
                    break;
                case BK_MUSIC_SOURCE.MUSIC_RADIO:
                    SetMusicChannel();
                    break;
            }
            DialogResult = true;
        }

        private void SetMusicFiles()
        {
            string name = lstFolders.SelectedItem as string;
            if (name == null)
            {
                MusicFolder = null;
                return;
            }

            PASection section = GetPASection();
            BKMusicFolder folder = section.GetMusicFolderByName(name);
            if (folder == null)
            {
                MusicFolder = null;
                return;
            }

            if (lstFiles.SelectedItems.Count == 0)
            {
                MusicFolder = folder;
            }
            else
            {
                MusicFolder = new BKMusicFolder(folder.Id, folder.Name);
                foreach (string fileName in lstFiles.SelectedItems)
                {
                    BKMusicFile file = folder.GetMusicFile(fileName);
                    if (file != null)
                        MusicFolder.AddMusicFile(file);
                }
            }
        }

        private void SetMusicCDFiles()
        {

        }

        private void SetMusicChannel()
        {

        }
    }
}
