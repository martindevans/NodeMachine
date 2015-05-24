using System.ComponentModel;
using System.Threading.Tasks;

namespace NodeMachine.Project
{
    public interface IProject
        : INotifyPropertyChanged
    {
        string Name { get; set; }

        string ProjectFile { get; set; }

        bool UnsavedChanges { get; }

        Task Save();
    }
}
