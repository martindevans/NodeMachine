using EpimetheusPlugins.Procedural;
using NodeMachine.Connection;
using NodeMachine.Model;
using NodeMachine.Model.Project;
using NodeMachine.ViewModel.Tabs;
using System.Windows;

namespace NodeMachine.View.Controls
{
    /// <summary>
    /// Interaction logic for FacadeEditor.xaml
    /// </summary>
    public partial class FacadeEditor : BaseEditorControl<Facade>, ITabName
    {
        public FacadeEditor(IProjectManager manager, IGameConnection connection, Facade facade)
            : base(manager, connection, facade)
        {
            InitializeComponent();
        }

        protected override System.Collections.Generic.ICollection<Facade> ProjectDataModelCollection
        {
            get
            {
                return ProjectManager.CurrentProject.ProjectData.Facades;
            }
        }

        protected override string ValueName
        {
            get
            {
                return Value.Name;
            }
        }

        protected async void SendToGame(object sender, RoutedEventArgs e)
        {
            if (!await Connection.IsConnected())
                return;

            //todo: pass in a procedural script
            await SendToGame(new Prism(), (ProceduralScript)null);
        }
    }
}
