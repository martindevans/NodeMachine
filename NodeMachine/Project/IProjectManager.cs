
using System.ComponentModel;
using System.Threading.Tasks;

namespace NodeMachine.Project
{
    public interface IProjectManager
        : INotifyPropertyChanged
    {
        IProject CurrentProject { get; set; }

        Task OpenProject(string fileName);
    }
}
