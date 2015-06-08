﻿using System.Collections.ObjectModel;
using NodeMachine.Connection;
using NodeMachine.Model;
using NodeMachine.Model.Project;
using NodeMachine.ViewModel.Tabs;

namespace NodeMachine.View.Controls
{
    /// <summary>
    /// Interaction logic for FloorEditor.xaml
    /// </summary>
    public partial class FloorEditor : BaseEditorControl<Floor>, ITabName
    {
        public FloorEditor(IProjectManager manager, IGameConnection connection, Floor value)
            : base(manager, connection, value)
        {
        }

        protected override ObservableCollection<Floor> ProjectDataModelCollection
        {
            get
            {
                return ProjectManager.CurrentProject.ProjectData.Floors;
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
