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

namespace renstech.NET.SupernovaDispatcher.PADispatch
{
    /// <summary>
    /// Interaction logic for ZoneContextWindow.xaml
    /// </summary>
    public partial class ZoneContextWindow : Window
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ZoneContextWindow));

        private PASubSystem _system = null;
        private PASection _section = null;
        private PAZone _zone = null;

        public ZoneContextWindow(PASubSystem system, PASection section, PAZone zone)
        {
            InitializeComponent();

            App app = App.Current as App;
            if (app.AppBkBrush != null)
                Background = app.AppBkBrush;

            _system = system;
            _section = section;
            _zone = zone;

            if (_section != null)
                btnZoneName.Content = _section.Name;

            if (_zone != null)
                btnZoneName.Content = _zone.Name;
        }

        private List<PASection> GetTargetSections()
        {
            if (_section == null && _zone == null)
                return null;

            List<PASection> sections = new List<PASection>();
            if (_section != null)
            {
                sections.Add(_section);
            }
            else if (_zone != null)
            {
                PASection sc = _system.GetPASectionById(_zone.SectionId);
                if (sc != null)
                {
                    PASection section = new PASection(sc.Id, sc.Name);
                    section.AddPAZone(_zone);
                    sections.Add(section);
                }
            }

            return sections;
        }

        private void btnManualSpeech_Click(object sender, RoutedEventArgs e)
        {
            List<PASection> sections = GetTargetSections();
            _system.Channel.Speech(sections);
            DialogResult = true;    
        }

        private void btnBkMusic_Click(object sender, RoutedEventArgs e)
        {
            MusicSelectWindow dialog = new MusicSelectWindow(_system, _section, _zone);
            dialog.Owner = this;

            Log.Debug("btnBkMusic_Click__MusicSelectWindow.ShowDialog BEFORE");

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            Log.Debug("btnBkMusic_Click__MusicSelectWindow.ShowDialog AFTER");

            List<PASection> sections = GetTargetSections();

            switch (dialog.MusicSource)
            {
                case BK_MUSIC_SOURCE.MUSIC_FILE:
                    _system.Channel.PlayFileMusic(sections, dialog.MusicFolder);    
                    break;
                case BK_MUSIC_SOURCE.MUSIC_CD:
                    break;
                case BK_MUSIC_SOURCE.MUSIC_RADIO:
                    break;
            }
            DialogResult = true;
        }

        private void btnAutoPA_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEavesdrop_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
