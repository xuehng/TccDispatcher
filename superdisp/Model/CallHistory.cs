using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace renstech.NET.SupernovaDispatcher.Model
{
    public delegate void CallHistoryChanged();

    public class CallHistory : INotifyPropertyChanged
    {
        private int _missedCalls;
        private List<HistoryItem> _items = new List<HistoryItem>();

        public event CallHistoryChanged OnCallHistoryChanged;

        public CallHistory()
        {
            _missedCalls = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        public List<HistoryItem> Items { get { return _items; } }
        public string FileName { get; set; }
        public bool IsModified { get; private set; }
        public bool IsExistUnread { get; set; }

        public int MissingCalls
        {
            get { return _missedCalls; }
            set
            {
                _missedCalls = value;

                if (value != 0 && !IsExistUnread)
                {
                    IsExistUnread = true;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsExistUnread"));
                }

                if (value == 0 && IsExistUnread)
                {
                    IsExistUnread = false;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsExistUnread"));
                }

                OnPropertyChanged(new PropertyChangedEventArgs("MissingCalls"));
            }
        }

        public void ClearUnreadMissing()
        {
            MissingCalls = 0;
        }

        public void AddHistory(HistoryItem item)
        {
            _items.Insert(0, item);
            IsModified = true;

            if (item.IsInbound && !item.IsAnswered)
            {
                MissingCalls++;
            }

            if (OnCallHistoryChanged != null)
            {
                OnCallHistoryChanged();
            }
        }

        public bool DeleteCallHistory(HistoryItem item)
        {
            IsModified = true;
            if (_items.Remove(item))
            {
                OnCallHistoryChanged();
                return true;
            }
            return false;
        }

        public void DeleteAll()
        {
            IsModified = true;
            _items.Clear();
            OnCallHistoryChanged();
        }

        public void Save()
        {
            if (IsModified == false)
            {
                return;
            }

            using (Stream stream = File.Open(FileName, FileMode.Create))
            {
                BinaryFormatter bin = new BinaryFormatter();
                bin.Serialize(stream, _items);
            }
        }

        public void Load()
        {
            _items.Clear();

            using (Stream stream = File.Open(FileName, FileMode.Open))
            {
                BinaryFormatter bin = new BinaryFormatter();
                _items = (List<HistoryItem>)bin.Deserialize(stream);
            }

            IsModified = false;
        }
    }
}