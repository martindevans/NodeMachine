using Ninject.Modules;
using NodeMachine.Model.Project;

namespace NodeMachine.Ninject
{
    public class ProjectModule
        : NinjectModule
    {
        public override void Load()
        {
            Bind<IProjectManager>().To<ProjectManager>().InSingletonScope();
        }
    }
}
