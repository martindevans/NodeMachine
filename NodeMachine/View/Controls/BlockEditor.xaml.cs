using NodeMachine.Connection;
using NodeMachine.Model.Project;
using NodeMachine.ViewModel.Tabs;
using System.Collections.Generic;
using Block = NodeMachine.Model.Block;

namespace NodeMachine.View.Controls
{
    /// <summary>
    /// Interaction logic for BlockEditor.xaml
    /// </summary>
    public partial class BlockEditor : BaseEditorControl<Block>, ITabName
    {
        public BlockEditor(IProjectManager manager, IGameConnection connection, Block value)
            : base(manager, connection, value)
        {
        }

        protected override ICollection<Block> ProjectDataModelCollection
        {
            get
            {
                return ProjectManager.CurrentProject.ProjectData.Blocks;
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
