
using System.ComponentModel;
using System.Threading.Tasks;

namespace NodeMachine.Project
{
    public interface IProjectManager
        : INotifyPropertyChanged
    {
        IProject CurrentProject { get; }

        Task OpenProject(string fileName);

        Task CloseProject();
    }
}
