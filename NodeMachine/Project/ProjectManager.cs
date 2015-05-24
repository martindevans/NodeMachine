
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NodeMachine.Annotations;

namespace NodeMachine.Project
{
    public class ProjectManager
        : IProjectManager
    {
        private IProject _currentProject = new Project(null);
        public IProject CurrentProject
        {
            get
            {
                return _currentProject;
            }
            private set
            {
                _currentProject = value;
                OnPropertyChanged();
            }
        }

        public async Task OpenProject(string fileName)
        {
            var p = new Project(fileName);
            await p.Load();
            CurrentProject = p;
        }

        public async Task CloseProject()
        {
            CurrentProject = new Project(null);
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
