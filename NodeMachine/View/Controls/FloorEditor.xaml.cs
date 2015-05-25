using NodeMachine.Annotations;
using NodeMachine.Model;
using NodeMachine.ViewModel.Tabs;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace NodeMachine.View.Controls
{
    /// <summary>
    /// Interaction logic for FloorEditor.xaml
    /// </summary>
    public partial class FloorEditor : UserControl, ITabName
    {
        private Floor _floor;
        public Floor Floor
        {
            get
            {
                return _floor;
            }
            set
            {
                if (_floor != null)
                    _floor.PropertyChanged -= FloorPropertyChanged;
                _floor = value;
                if (_floor != null)
                    _floor.PropertyChanged += FloorPropertyChanged;
                
                OnPropertyChanged();
                OnPropertyChanged("TabName");
            }
        }

        public string TabName
        {
            get
            {
                return _floor == null ? "Floor Editor" : string.Format("{0}.floor", _floor.Name);
            }
        }

        public FloorEditor(Floor floor)
        {
            InitializeComponent();

            Floor = floor;
        }

        private void FloorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
                OnPropertyChanged("TabName");
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
