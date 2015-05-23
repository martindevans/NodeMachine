using System.ComponentModel;
using NodeMachine.Model;
using System.Threading.Tasks;

namespace NodeMachine.Connection
{
    public interface IGameConnection
        : INotifyPropertyChanged
    {
        ProceduralNode Topology { get; }

        Task<ProceduralNode> RefreshTopology();

        Task ClearTopology();

        IScreenCollection Screens { get; }

        Task Connect();

        void Disconnect();
    }
}
