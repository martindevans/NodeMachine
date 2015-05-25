using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NodeMachine.Annotations;

namespace NodeMachine.Model.Project
{
    public class Project
        : IProject
    {
        private ProjectData _projectData;
        public ProjectData ProjectData
        {
            get
            {
                return _projectData;
            }
            private set
            {
                //Unsubscribe from changes on old data
                if (_projectData != null)
                {
                    _projectData.PropertyChanged -= ProjectDataPropertyChanged;
                    _projectData.Blocks.CollectionChanged -= ProjectDataCollectionChanged;
                    _projectData.Buildings.CollectionChanged -= ProjectDataCollectionChanged;
                    _projectData.Cities.CollectionChanged -= ProjectDataCollectionChanged;
                    _projectData.Facades.CollectionChanged -= ProjectDataCollectionChanged;
                    _projectData.Floors.CollectionChanged -= ProjectDataCollectionChanged;
                    _projectData.Misc.CollectionChanged -= ProjectDataCollectionChanged;
                    _projectData.Rooms.CollectionChanged -= ProjectDataCollectionChanged;
                }

                _projectData = value;

                //Subscribe to changes on new data
                if (_projectData != null)
                {
                    _projectData.PropertyChanged += ProjectDataPropertyChanged;
                    SubscribeToCollectionUpdates(_projectData.Blocks);
                    //SubscribeToCollectionUpdates(_projectData.Buildings);
                    //SubscribeToCollectionUpdates(_projectData.Cities);
                    SubscribeToCollectionUpdates(_projectData.Facades);
                    SubscribeToCollectionUpdates(_projectData.Floors);
                    //SubscribeToCollectionUpdates(_projectData.Misc);
                    //SubscribeToCollectionUpdates(_projectData.Rooms);
                }

                OnPropertyChanged();
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

            ProjectData = new ProjectData();
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

                    j.Serialize(jsonWriter, ProjectData);
                }
            });

            UnsavedChanges = false;
        }

        public async Task Load()
        {
            ProjectData = await Task.Factory.StartNew(() =>
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

        private void ProjectDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UnsavedChanges = true;
        }

        private void SubscribeToCollectionUpdates<T>(ObservableCollection<T> collection) where T : INotifyPropertyChanged
        {
            collection.CollectionChanged += ProjectDataCollectionChanged;

            foreach (var item in collection)
                ((INotifyPropertyChanged) item).PropertyChanged += ProjectDataPropertyChanged;
        }

        private void ProjectDataCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UnsavedChanges = true;

            if (e.Action == NotifyCollectionChangedAction.Add)
                foreach (var item in e.NewItems)
                    ((INotifyPropertyChanged)item).PropertyChanged += ProjectDataPropertyChanged;

            if (e.Action == NotifyCollectionChangedAction.Remove)
                foreach (var item in e.OldItems)
                    ((INotifyPropertyChanged)item).PropertyChanged -= ProjectDataPropertyChanged;
        }
    }
}
