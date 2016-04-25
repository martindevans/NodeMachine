using System.IO;
using System.Threading.Tasks;
using Dragablz;
using Ninject;
using NodeMachine.Connection;
using NodeMachine.Model.Project;
using NodeMachine.View;
using NodeMachine.View.Controls;
using NodeMachine.ViewModel.Tabs;
using System.Windows;
using NodeMachine.Ninject;

namespace NodeMachine
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IKernel _container;

        public App()
        {
            _container = new StandardKernel(
                new TabModule(),
                new ProjectModule(),
                new GameModule()
            );
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            await SetupMainWindow(e);

            Current.MainWindow.Show();
        }

        private async Task SetupMainWindow(StartupEventArgs startupEventArgs)
        {
            //Create the main window
            Current.MainWindow = _container.Get<MainWindow>();

            //Check that there is a file arg
            if (await LoadStartupFile(startupEventArgs))
            {
                //valid startup file specified and project loaded, show project tab
                ((MainWindow)Current.MainWindow).TabContents.Add(new TabContent(_container.Get<ProjectControl>(), "Project"));
            }
            else
            {
                //No (valid) startup file specified, just show the new tab control
                ((MainWindow)Current.MainWindow).TabContents.Add(new TabContent(_container.Get<NewTabControl>()));
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            _container.Get<IGameConnection>().Disconnect();
        }

        private async Task<bool> LoadStartupFile(StartupEventArgs e)
        {
            //Check that there is a file arg
            if (e.Args.Length == 0)
                return false;

            //Check the file really exists
            FileInfo file = new FileInfo(e.Args[0]);
            if (!file.Exists)
                return false;

            //Check that it has the right extension
            if (file.Extension != ".nmproj")
                return false;

            var manager = _container.Get<IProjectManager>();
            await manager.OpenProject(file.FullName);
            return true;
        }
    }
}
