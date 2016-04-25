using Dragablz;
using Ninject.Modules;
using NodeMachine.ViewModel.Tabs;

namespace NodeMachine.Ninject
{
    public class TabModule
        : NinjectModule
    {
        public override void Load()
        {
            Bind<IInterTabClient>().To<InterTabClient>();
            Bind<IInterLayoutClient>().To<InterLayoutClient>();
        }
    }
}
