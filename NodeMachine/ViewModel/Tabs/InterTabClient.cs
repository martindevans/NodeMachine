using Dragablz;
using Ninject;
using System.Windows;
using NodeMachine.View;

namespace NodeMachine.ViewModel.Tabs
{
    public class InterTabClient : IInterTabClient
    {
        private readonly IKernel _kernel;

        public InterTabClient(IKernel kernel)
        {
            _kernel = kernel;
        }

        public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            var view = _kernel.Get<MainWindow>();
            return new NewTabHost<Window>(view, view.InitialTabablzControl);
        }

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            return TabEmptiedResponse.CloseWindowOrLayoutBranch;
        }
    }
}
