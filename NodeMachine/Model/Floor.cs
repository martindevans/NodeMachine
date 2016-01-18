using System.ComponentModel;
using System.Runtime.CompilerServices;
using NodeMachine.Annotations;

namespace NodeMachine.Model
{
    public class Floor
        : INotifyPropertyChanged
    {
        private string _name = "Unnamed Floor";
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

        private string _markup;
        public string Markup
        {
            get
            {
                return _markup;
            }
            set
            {
                _markup = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
