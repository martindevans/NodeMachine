using System.Collections.ObjectModel;
using System.Windows;
using NodeMachine.Connection;
using NodeMachine.Model;
using NodeMachine.Model.Project;
using NodeMachine.ViewModel.Tabs;

namespace NodeMachine.View.Controls
{
    /// <summary>
    /// Interaction logic for BuildingEditor.xaml
    /// </summary>
    public partial class BuildingEditor : BaseEditorControl<Building>, ITabName
    {
        public BuildingEditor(IProjectManager manager, IGameConnection connection, Building value)
            : base(manager, connection, value)
        {
            InitializeComponent();
        }

        protected override ObservableCollection<Building> ProjectDataModelCollection
        {
            get
            {
                return ProjectManager.CurrentProject.ProjectData.Buildings;
            }
        }

        protected override string ValueName
        {
            get
            {
                return Value.Name;
            }
        }

        private void CheckMarkup(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void SendToGame(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
