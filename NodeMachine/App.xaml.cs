using Dragablz;
using MahApps.Metro.Controls;
using Ninject;
using System.Windows;
using NodeMachine.Connection;
using NodeMachine.Model.Project;
using NodeMachine.View;
using NodeMachine.View.Controls;
using NodeMachine.ViewModel.Tabs;

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
            _container = new StandardKernel();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ConfigureContainer();
            ComposeObjects();
            Current.MainWindow.Show();
        }

        private void ConfigureContainer()
        {
            _container.Bind<IGameConnection>().ToConstant(_container.Get<GameConnection>());
            _container.Bind<IProjectManager>().ToConstant(_container.Get<ProjectManager>());

            _container.Bind<IInterTabClient>().To<InterTabClient>();
        }

        private void ComposeObjects()
        {
            Current.MainWindow = _container.Get<MainWindow>();
            ((MainWindow)Current.MainWindow).TabContents.Add(new TabContent(_container.Get<NewTabControl>()));
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            _container.Get<IGameConnection>().Disconnect();
        }
    }
}
