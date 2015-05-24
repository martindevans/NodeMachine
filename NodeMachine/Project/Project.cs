using Newtonsoft.Json;
using NodeMachine.Annotations;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace NodeMachine.Project
{
    public class Project
        : IProject
    {
        private ProjectData _projectData = new ProjectData();

        public ProjectData ProjectData
        {
            get
            {
                return _projectData;
            }
            private set
            {
                _projectData = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get
            {
                return _projectData.Name;
            }
            set
            {
                _projectData.Name = value;
                OnPropertyChanged();

                UnsavedChanges = true;
            }
        }

        private bool _unsavedChanges = false;
        public bool UnsavedChanges
        {
            get
            {
                return _unsavedChanges;
            }
            private set
            {
                _unsavedChanges = value;
                OnPropertyChanged();
            }
        }

        private string _filePath;
        public string ProjectFile
        {
            get
            {
                return _filePath;
            }
            set
            {
                _filePath = value;
                OnPropertyChanged();

                UnsavedChanges = true;
            }
        }

        public Project(string filePath)
        {
            _filePath = filePath;
        }

        public async Task Save()
        {
            await Task.Factory.StartNew(() =>
            {
                using (var file = File.Open(ProjectFile, FileMode.Create))
                using (var writer = new StreamWriter(file))
                using (JsonWriter jsonWriter = new JsonTextWriter(writer))
                {
                    JsonSerializer j = new JsonSerializer()
                    {
                        Formatting = Formatting.Indented,
                    };

                    j.Serialize(jsonWriter, _projectData);
                }
            });

            UnsavedChanges = false;
        }

        public async Task Load()
        {
            _projectData = await Task.Factory.StartNew(() =>
            {
                using (var file = File.Open(ProjectFile, FileMode.OpenOrCreate))
                using (var reader = new StreamReader(file))
                using (JsonReader jsonReader = new JsonTextReader(reader))
                {
                    JsonSerializer j = new JsonSerializer()
                    {
                        Formatting = Formatting.Indented,
                    };

                    return j.Deserialize<ProjectData>(jsonReader) ?? new ProjectData();
                }
            });

            UnsavedChanges = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
