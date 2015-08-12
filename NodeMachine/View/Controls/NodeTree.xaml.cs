using System.Threading.Tasks;
using Construct_Gamemode.Map;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NodeMachine.Annotations;
using NodeMachine.Connection;
using NodeMachine.ViewModel.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace NodeMachine.View.Controls
{
    /// <summary>
    /// Interaction logic for NodeTree.xaml
    /// </summary>
    public partial class NodeTree : UserControl, INotifyPropertyChanged
    {
        public string SearchText { get; set; }

        public IGameConnection Connection { get; private set; }

        private IDisposable _observer;

        private ProceduralNodeViewModel _rootTopology;
        public IEnumerable<ProceduralNodeViewModel> Topology
        {
            get
            {
                if (_rootTopology == null || !_rootTopology.IsFiltered)
                    yield break;
                yield return _rootTopology;
            }
        }

        private int _seed;
        public int Seed
        {
            get { return _seed; }
            set
            {
                _seed = value;
                OnPropertyChanged();
            }
        }

        public NodeTree(IGameConnection connection)
        {
            Connection = connection;
            if (connection.Topology.Root != null)
                _rootTopology = new ProceduralNodeViewModel(Connection.Topology.Root);

            InitializeComponent();

            _observer = Observable.FromEventPattern<TextChangedEventArgs>(NodeFilterTextInput, "TextChanged")
                      .Throttle(TimeSpan.FromMilliseconds(250))
                      .Subscribe(a => Dispatcher.Invoke(() => FilterChanged(a.Sender, a.EventArgs)));

            connection.Topology.PropertyChanged += ConnectionPropertyChanged;

            RefreshTopology();
        }

        private void ConnectionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Root":
                    _rootTopology = Connection.Topology.Root == null ? null : new ProceduralNodeViewModel(Connection.Topology.Root);
                    OnPropertyChanged("Topology");
                    break;
            }
        }

        private async void RefreshTopology()
        {
            await Connection.Topology.Refresh();

            if (Connection.Topology != null && Connection.Topology.Root != null)
                Seed = Connection.Topology.Root.InitialSeed;
        }

        private void RefreshTopology(object sender, RoutedEventArgs e)
        {
            RefreshTopology();
        }

        private async void FilterChanged(object sender, TextChangedEventArgs e)
        {
            await _rootTopology.Filter(((TextBox)sender).Text);
        }

        private async void ClearTopology(object sender, RoutedEventArgs e)
        {
            await Connection.Topology.Clear();
        }

        private async void RebuildTopology(object sender, RoutedEventArgs e)
        {
            await Rebuild();
        }

        private async void RebuildTopologyRandomSeed(object sender, RoutedEventArgs e)
        {
            Seed = new Random().Next();
            await Rebuild();
        }

        private async Task Rebuild()
        {
            if (Connection.Topology == null || Connection.Topology.Root == null)
            {
                var window = (MetroWindow)Window.GetWindow(this);

                var connected = await Connection.IsConnected();
                if (!connected)
                    await window.ShowMessageAsync("Cannot Rebuild", "Not Connected");
                else
                    await window.ShowMessageAsync("Cannot Rebuild", "No Root Node");
            }
            else
            {
                //Set the root to whatever the root currently is - this will start a new build with the same root
                var root = Connection.Topology.Root.Script.Guid;
                await Connection.Topology.SetRoot(root, new RemoteRootInit(), Seed);
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

        private void CopyGuid(object sender, RoutedEventArgs e)
        {
            var node = (ProceduralNodeViewModel)((Control)sender).DataContext;
            Clipboard.SetText(node.Guid.ToString());
        }

        private void CopyType(object sender, RoutedEventArgs e)
        {
            var node = (ProceduralNodeViewModel)((Control)sender).DataContext;
            Clipboard.SetText(node.TypeName);
        }

        private ProceduralNodeViewModel _selectedNode;
        public ProceduralNodeViewModel SelectedNode
        {
            get
            {
                return _selectedNode;
            }
            set
            {
                _selectedNode = value;
                OnPropertyChanged();
            }
        }

        private void NodeSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedNode = (ProceduralNodeViewModel)((TreeView) sender).SelectedItem;
        }
    }
}
