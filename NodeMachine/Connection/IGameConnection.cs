using System.ComponentModel;
using System.Threading.Tasks;

namespace NodeMachine.Connection
{
    public interface IGameConnection
        : INotifyPropertyChanged
    {
        ITopology Topology { get; }

        IScreenCollection Screens { get; }

        Task Connect();

        Task<bool> IsConnected();

        void Disconnect();
    }
}
