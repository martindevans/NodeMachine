using System.ComponentModel;
using System.Threading.Tasks;

namespace NodeMachine.Model.Project
{
    public interface IProject
        : INotifyPropertyChanged
    {
        ProjectData ProjectData { get; }

        string ProjectFile { get; set; }

        bool UnsavedChanges { get; }

        Task Save();
    }
}
