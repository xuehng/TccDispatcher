using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using LumiSoft.Net.MIME;
using renstech.NET.SupernovaDispatcher.Utils;
using MessageWindow = renstech.NET.SupernovaDispatcher.Control.MessageWindow;

namespace renstech.NET.SupernovaDispatcher.Model
{
    public class RecordingFile
    {
        public int Index { get; set; }
        public string Path { get; set; }
        public string Caller { get; set; }
        public string Callee { get; set; }
        public DateTime Date { get; set; }
    }

    public class RecordingFiles
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(RecordingFiles));

        private readonly SpnvSubSystem _subsystem;

        public RecordingFiles(SpnvSubSystem system)
        {
            _subsystem = system;
        }

        readonly ObservableCollection<RecordingFile> _recordings = new ObservableCollection<RecordingFile>();
        public ObservableCollection<RecordingFile> Files
        {
            get { return _recordings; }
        }

        private static string NormalizeFileName(string filename)
        {
            filename = filename.Replace("*", "γ");
            return filename;
        }

        private static string DenormalizeFileName(string filename)
        {
            filename = filename.Replace("γ", "*"); 
            return filename;
        }

        public static string BuildRecordingFileName(string dir, DateTime time, string caller, string callee)
        {
            string filename = string.Format("{0}/{1}-{2}-{3}.wav", dir,
                time.ToString("yyyyMMddHHmmss", new CultureInfo("en-US")), caller, callee);

            return NormalizeFileName(filename);
        }

        public void DelPreviousRecordings()
        {
            var deadlineDate = DateTime.Now - new TimeSpan(_subsystem.Setting.RecordingDelDate, 0, 0, 0);
            LoadRecordingFiles(_subsystem.Setting.RecordingFileDir);
            
            List<RecordingFile> delFiles = new List<RecordingFile>();
            
            foreach (var file in _recordings)
            {
                if (file.Date.CompareTo(deadlineDate.Date) < 0)
                {
                    //实际的文件删除操作
                    try
                    {
                        Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(file.Path,
                            Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                            Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                        delFiles.Add(file);
                    }
                    catch (Exception ex)
                    {
                        Log.Debug("DelPreviousRecordings__MessageWindow.ShowDialog BEFORE");

                        MessageWindow dialog = new MessageWindow(
                            Properties.Resources.IDS_ERROR_WINDOW_TITLE,
                            Properties.Resources.IDS_FILE_DELETE_FAILURE,
                            MessageWindow.ButtonListType.ButtonOk,
                            MessageWindow.IconType.IconError);
                        Application.Current.Dispatcher.Invoke(new Action(() => dialog.ShowDialog()));
                        
                        Log.Debug("DelPreviousRecordings__MessageWindow.ShowDialog AFTER");

                        break;
                    }
                }
            }
            //集合中对应项的删除操作
            foreach (var delfile in delFiles)
            {
                _recordings.Remove(delfile);
            }
        }

        public bool LoadRecordingFiles(string dir)
        {
            _recordings.Clear();

            var fileEntries = Directory.GetFiles(dir);
            foreach (string filename in fileEntries)
            {
                string caller;
                string callee;
                DateTime date;
                
                if (!ProcessFile(filename, out caller, out callee, out date))
                    continue;

                RecordingFile file = new RecordingFile {Path = filename, Caller = caller, Callee = callee, Date = date};
                AddRecordingFile(file);
            }
            return true;
        }

        public void RemoveAll()
        {
            List<RecordingFile> delfiles = new List<RecordingFile>();

            foreach (RecordingFile file in _recordings)
            {
                try
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(file.Path,
                        Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                        Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                    delfiles.Add(file);
                }
                catch (Exception ex)
                {
                    Log.Debug("RemoveAll__MessageWindow.ShowDialog BEFORE");

                    MessageWindow dialog = new MessageWindow(
                        Properties.Resources.IDS_ERROR_WINDOW_TITLE,
                        Properties.Resources.IDS_FILE_DELETE_FAILURE, 
                        MessageWindow.ButtonListType.ButtonOk, 
                        MessageWindow.IconType.IconError);
                    dialog.ShowDialog();
                    
                    Log.Debug("RemoveAll__MessageWindow.ShowDialog AFTER"); 
                    
                    break;
                }
            }

            foreach (var delfile in delfiles)
            {
                _recordings.Remove(delfile);
            }
        }

        public bool RemoveRecordingFile(RecordingFile file)
        {
            try
            {
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(file.Path, 
                    Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                    Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                _recordings.Remove(file);
                return true;
            }
            catch (Exception ex)
            {
                Log.Debug("RemoveRecordingFile__MessageWindow.ShowDialog BEFORE");

                MessageWindow dialog = new MessageWindow(
                    Properties.Resources.IDS_ERROR_WINDOW_TITLE,
                    Properties.Resources.IDS_FILE_DELETE_FAILURE,
                    MessageWindow.ButtonListType.ButtonOk,
                    MessageWindow.IconType.IconError);
                dialog.ShowDialog();

                Log.Debug("RemoveRecordingFile__MessageWindow.ShowDialog AFTER"); 
                
                return false;
            }
        }

        private void AddRecordingFile(RecordingFile file)
        {
            if (_recordings.Count == 0)
            {
                _recordings.Add(file);
            }
            else
            {
                foreach (RecordingFile item in _recordings)
                {
                    if (item.Date.CompareTo(file.Date) < 0)
                    {
                        int index = _recordings.IndexOf(item);
                        _recordings.Insert(index, file);
                        return;
                    }
                }
                _recordings.Add(file);
            }
        }

        private bool ProcessFile(string filepath, out string caller, out string callee, out DateTime date)
        {
            caller = "";
            callee = "";
            date = DateTime.Now;

            string recordname = DenormalizeFileName(filepath);

            int index = recordname.LastIndexOf('\\');
            if (index != -1)
            {
                recordname = recordname.Substring(index + 1);
            }

            string[] filename = recordname.Split('.');
            if (filename.Length != 2)
            {
                return false;
            }

            string[] part = filename[0].Split('-');
            if (part.Length != 3)
                return false;

            if (!DateTime.TryParseExact(part[0], "yyyyMMddHHmmss", new CultureInfo("en-US"), DateTimeStyles.None, out date))
                return false;

            caller = part[1];
            callee = part[2];

            if (_subsystem != null)
            {
                string calleeDispName = null, calleeDispNum = null;
                _subsystem.GetDestDisplayinfo(callee, ref calleeDispName, ref calleeDispNum);

                if (!string.IsNullOrEmpty(calleeDispName))
                {
                    callee = calleeDispName;
                }

                string callerDispName = null, callerDispNum = null;
                _subsystem.GetDestDisplayinfo(caller, ref callerDispName, ref callerDispNum);

                if (!string.IsNullOrEmpty(callerDispName))
                {
                    caller = callerDispName;
                }
            }

            return true;
        }
    }
}