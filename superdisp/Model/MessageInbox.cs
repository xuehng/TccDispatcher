using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace renstech.NET.SupernovaDispatcher.Model
{
    public class Message
    {
        public Message(int id, string from, string content, string date)
        {
            Readed = false;
            Id = id;
            From = from;
            Content = content;
            DateTime = date;
        }

        public int Id { get; private set; }
        public bool Readed { get; set; }
        public string From { get; private set; }
        public string Content { get; private set; }
        public string DateTime { get; private set; }
    }

    public class MessageInbox : INotifyPropertyChanged
    {
        private int _unReadMsg;

        private readonly ObservableCollection<Message> _listInMessages = new ObservableCollection<Message>();
        public ObservableCollection<Message> Messages
        {
            get { return _listInMessages; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        public int UnreadMsg 
        { 
            get { return _unReadMsg;}
            set 
            { 
                _unReadMsg = value;

                if (_unReadMsg == 0)
                {
                    foreach(Message msg in _listInMessages)
                        msg.Readed = true;
                }

                OnPropertyChanged(new PropertyChangedEventArgs("UnreadMsg"));                
                OnPropertyChanged(new PropertyChangedEventArgs("IsExistUnread"));                
            } 
        }

        public bool IsExistUnread 
        {
            get { return (_unReadMsg !=0 ); }
        }

        public Message AddMessage(string pFrom, string pContent)
        {
            Message msg = new Message(_listInMessages.Count, pFrom, 
                pContent, DateTime.Now.ToString());
            _listInMessages.Insert(0, msg);

            UnreadMsg++;
            return msg;
        }

        public bool DeleteMessage(Message msg)
        {
            return _listInMessages.Remove(msg);
        }

        public void DeleteAll()
        {
            _listInMessages.Clear();
        }
    }
}