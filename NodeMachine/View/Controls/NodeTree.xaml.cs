using Construct_Gamemode.Map;
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

        private void RebuildTopology(object sender, RoutedEventArgs e)
        {
            //Set the root to whatever the root currently is - this will start a new build with the same root
            var root = Connection.Topology.Root.Script.Guid;
            Connection.Topology.SetRoot(root, new RemoteRootInit());
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
