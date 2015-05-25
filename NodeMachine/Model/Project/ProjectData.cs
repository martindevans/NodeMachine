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
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Cities { get; private set; }
        public ObservableCollection<Block> Blocks { get; private set; }
        public ObservableCollection<string> Buildings { get; private set; }
        public ObservableCollection<Floor> Floors { get; private set; }
        public ObservableCollection<string> Rooms { get; private set; }
        public ObservableCollection<string> Misc { get; private set; }
        public ObservableCollection<Facade> Facades { get; private set; }

        public ProjectData()
        {
            Cities = new ObservableCollection<string>();
            Blocks = new ObservableCollection<Block>();
            Buildings = new ObservableCollection<string>();
            Floors = new ObservableCollection<Floor>();
            Rooms = new ObservableCollection<string>();
            Misc = new ObservableCollection<string>();
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
}
