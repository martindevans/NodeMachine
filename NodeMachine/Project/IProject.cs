using System.ComponentModel;
using System.Threading.Tasks;

namespace NodeMachine.Project
{
    public interface IProject
        : INotifyPropertyChanged
    {
        string Name { get; }

        string ProjectFile { get; }

        bool UnsavedChanges { get; }

        Task Save();

        Task Load();
    }
}
