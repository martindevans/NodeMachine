using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using NodeMachine.Annotations;
using NodeMachine.ViewModel.Tabs;
using Block = NodeMachine.Model.Block;

namespace NodeMachine.View.Controls
{
    /// <summary>
    /// Interaction logic for BlockEditor.xaml
    /// </summary>
    public partial class BlockEditor : UserControl, ITabName
    {
        private Block _block;
        public Block Block
        {
            get
            {
                return _block;
            }
            set
            {
                if (_block != null)
                    _block.PropertyChanged -= BlockPropertyChanged;
                _block = value;
                if (_block != null)
                    _block.PropertyChanged += BlockPropertyChanged;
                
                OnPropertyChanged();
                OnPropertyChanged("TabName");
            }
        }

        public string TabName
        {
            get
            {
                return _block == null ? "Block Editor" : string.Format("{0}.block", _block.Name);
            }
        }

        public BlockEditor(Block block)
        {
            InitializeComponent();

            Block = block;
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
