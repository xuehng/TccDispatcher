using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace renstech.NET.SupernovaDispatcher.Model
{
    [Serializable]
    public class User : INotifyPropertyChanged
    {
        private bool _present;
        private bool _highlight;
        private string _firstName;
        private string _lastName;
        private string _number;
        private readonly UserDialogInfo _dialog = new UserDialogInfo(); 

        public enum Type
        {
            None,
            Normal,
            Customized,
        }

        public User()
        {
            Id = -1;
        }

        public User(int id, string firstname, string lastname, string number)
        {
            Id = id;
            FirstName = firstname;
            LastName = lastname;
            Number = number;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        public int Id { get; set; }

        public string FirstName
        {
            get { return _firstName; }
            set 
            { 
                _firstName = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Name"));
            }
        }

        public string LastName
        {
            get { return _lastName; } 
            set 
            { 
                _lastName = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Name"));
            }
        }

        public string Number
        {
            get { return _number; }
            set 
            { 
                _number = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Number"));
            }
        }

        [XmlIgnore]
        public Type UserType { get; set; }

        [XmlIgnore]
        public bool HasCamera { get; set; }

        [XmlIgnore]
        public bool IsBaned { get; set; }

        [XmlIgnore]
        public bool IsLocalUser { get; set; }

        [XmlIgnore]
        public UserDialogInfo DialogInfo { get { return _dialog; } }

        [XmlIgnore]
        public string Name 
        { 
            get 
            {
                string name = LastName + FirstName;
                return name.Trim().Length == 0 ? Number : name;
            } 
        }

        [XmlIgnore]
        public bool IsRegistered
        {
            get { return _present; }
            set { _present = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsRegistered"));
            }
        }

        [XmlIgnore]
        public bool IsHighLight
        {
            get { return _highlight; }
            set { _highlight = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsHighLight"));
            }
        }
    }
}