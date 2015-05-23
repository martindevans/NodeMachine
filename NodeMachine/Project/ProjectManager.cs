
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NodeMachine.Annotations;

namespace NodeMachine.Project
{
    public class ProjectManager
        : IProjectManager
    {
        private IProject _currentProject;
        public IProject CurrentProject {
            get
            {
                return _currentProject;
            }
            set
            {
                _currentProject = value;
                OnPropertyChanged();
            }
        }

        public async Task OpenProject(string fileName)
        {
            CurrentProject = new Project(fileName);
            await CurrentProject.Load();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
