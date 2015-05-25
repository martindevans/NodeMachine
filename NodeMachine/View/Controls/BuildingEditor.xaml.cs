using NodeMachine.Connection;
using NodeMachine.Model;
using NodeMachine.Model.Project;
using NodeMachine.ViewModel.Tabs;
using System.Collections.Generic;

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
        }

        protected override ICollection<Building> ProjectDataModelCollection
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
    }
}
