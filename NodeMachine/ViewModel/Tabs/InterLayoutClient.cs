using System.Windows;
using Dragablz;

namespace NodeMachine.ViewModel.Tabs
{
    public class InterLayoutClient
        : IInterLayoutClient
    {
        private readonly IInterTabClient _interTabClient;

        private readonly DefaultInterLayoutClient _client = new DefaultInterLayoutClient();

        public InterLayoutClient(IInterTabClient interTabClient)
        {
            _interTabClient = interTabClient;
        }

        public INewTabHost<UIElement> GetNewHost(object partition, TabablzControl source)
        {
            var b = _client.GetNewHost(partition, source);

            b.TabablzControl.InterTabController.InterTabClient = _interTabClient;

            return b;
        }
    }
}
