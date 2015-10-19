using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NodeMachine.Annotations;

namespace NodeMachine.Model.Project
{
    public class ProjectData
        : INotifyPropertyChanged
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private Guid _guid = Guid.NewGuid();

        public Guid Guid
        {
            get { return _guid; }
            set
            {
                _guid = value;
                OnPropertyChanged();
            }
        }

        private string _compileOutputDirectory;
        public string CompileOutputDirectory
        {
            get { return _compileOutputDirectory; }
            set
            {
                _compileOutputDirectory = value;
                OnPropertyChanged();
            }
    }

        public ObservableCollection<MetadataValue> Metadata { get; private set; }

        public ObservableCollection<City> Cities { get; private set; }
        public ObservableCollection<Block> Blocks { get; private set; }
        public ObservableCollection<Building> Buildings { get; private set; }
        public ObservableCollection<Floor> Floors { get; private set; }
        public ObservableCollection<string> Rooms { get; private set; }
        public ObservableCollection<Facade> Facades { get; private set; }

        public ProjectData()
        {
            Metadata = new ObservableCollection<MetadataValue>();

            Cities = new ObservableCollection<City>();
            Blocks = new ObservableCollection<Block>();
            Buildings = new ObservableCollection<Building>();
            Floors = new ObservableCollection<Floor>();
            Rooms = new ObservableCollection<string>();
            Facades = new ObservableCollection<Facade>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MetadataValue
        : INotifyPropertyChanged
    {
        private string _key;
        public string Key
        {
            get { return _key; }
            set
            {
                _key = value;
                OnPropertyChanged();
            }
        }

        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [JetBrains.Annotations.NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
