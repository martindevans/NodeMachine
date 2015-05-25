using NodeMachine.Annotations;
using NodeMachine.Model;
using NodeMachine.ViewModel.Tabs;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace NodeMachine.View.Controls
{
    /// <summary>
    /// Interaction logic for BuildingEditor.xaml
    /// </summary>
    public partial class BuildingEditor : UserControl, ITabName
    {
        private Building _building;
        public Building Building
        {
            get
            {
                return _building;
            }
            set
            {
                if (_building != null)
                    _building.PropertyChanged -= BlockPropertyChanged;
                _building = value;
                if (_building != null)
                    _building.PropertyChanged += BlockPropertyChanged;
                
                OnPropertyChanged();
                OnPropertyChanged("TabName");
            }
        }

        public string TabName
        {
            get
            {
                return _building == null ? "Block Editor" : string.Format("{0}.building", _building.Name);
            }
        }

        public BuildingEditor(Building building)
        {
            InitializeComponent();

            Building = building;
        }

        private void BlockPropertyChanged(object sender, PropertyChangedEventArgs e)
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
