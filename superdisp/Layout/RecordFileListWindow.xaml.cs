using System.Windows;
using System.Windows.Controls;
using renstech.NET.SupernovaDispatcher.Model;
using renstech.NET.SupernovaDispatcher.Utils;
using MessageWindow = renstech.NET.SupernovaDispatcher.Control.MessageWindow;

namespace renstech.NET.SupernovaDispatcher.Layout
{
    /// <summary>
    /// RecordFileListWindow.xaml 的交互逻辑
    /// </summary>
    public partial class RecordFileListWindow : Window
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(RecordFileListWindow));

        private SpnvSubSystem _subsystem = null;
        private RecordingFiles _recording = null;

        public RecordFileListWindow(SpnvSubSystem system)
        {
            this.InitializeComponent();

            App app = App.Current as App;
            if (app.AppBkBrush != null)
                Background = app.AppBkBrush;

            _subsystem = system;

            _recording = new RecordingFiles(_subsystem);
            _recording.LoadRecordingFiles(system.Setting.RecordingFileDir);
            if (_recording.Files.Count != 0)
                lsvFiles.ItemsSource = _recording.Files;
            else
                lsvFiles.ItemsSource = null;

            btnPlay.IsEnabled = false;
            btnDelete.IsEnabled = false;

            if (_recording.Files.Count == 0)
            {
                btnDeleteAll.IsEnabled = false;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            RecordingFile file = lsvFiles.SelectedItem as RecordingFile;
            if (file == null)
            {
                return;
            }
            _recording.RemoveRecordingFile(file);
        }

        private void btnDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            if (lsvFiles.Items.Count == 0)
            {
                return;
            }

            Log.Debug("btnDeleteAll_Click__MessageWindow.ShowDialog BEFORE");

            MessageWindow dialog = new MessageWindow(
                Properties.Resources.IDS_RECORD_DELETE_ALL_TITLE,
                Properties.Resources.IDS_RECORD_DELETE_ALL_CONTENT,
                MessageWindow.ButtonListType.ButtonOkCancel) {Owner = this};
            
            if (dialog.ShowDialog() != true)
            {
                return;
            }
            
            Log.Debug("btnDeleteAll_Click__MessageWindow.ShowDialog AFTER");

            _recording.RemoveAll();
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (lsvFiles.SelectedItem == null)
            {
                return;
            }

            RecordingFile file = lsvFiles.SelectedItem as RecordingFile;
            if (file == null)
            {
                return;
            }

            FilePlayerWindow dialog = new FilePlayerWindow(file.Path);
            dialog.Owner = this;

            Log.Debug("btnPlay_Click__FilePlayerWindow.ShowDialog BEFORE");
            dialog.ShowDialog();
            Log.Debug("btnPlay_Click__FilePlayerWindow.ShowDialog AFTER");
        }

        private void lsvFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnPlay.IsEnabled = (lsvFiles.SelectedItem != null);
            btnDelete.IsEnabled = (lsvFiles.SelectedItem != null);
        }
    }
}