using Ninject.Modules;
using NodeMachine.Connection;

namespace NodeMachine.Ninject
{
    public class GameModule
        : NinjectModule
    {
        public override void Load()
        {
            Bind<IGameConnection>().To<GameConnection>().InSingletonScope();
        }
    }
}
