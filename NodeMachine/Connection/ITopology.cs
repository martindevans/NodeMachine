using System.ComponentModel;
using Construct_Gamemode.Map;
using NodeMachine.Model;
using System;
using System.Threading.Tasks;

namespace NodeMachine.Connection
{
    public interface ITopology
        : INotifyPropertyChanged
    {
        ProceduralNode Root { get; }

        Task<ProceduralNode> Refresh();

        Task Clear();

        Task<bool> SetRoot(Guid scriptId, RemoteRootInit data);

        bool HasLiveConnection { get; }
    }
}
