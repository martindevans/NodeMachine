using System.ComponentModel;
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

        Task<bool> SetRoot(Guid scriptId);

        bool HasLiveConnection { get; }


    }
}
