using System.Collections.ObjectModel;

namespace NodeMachine.Project
{
    public class ProjectData
    {
        public string Name { get; set; }

        public ObservableCollection<string> Floors = new ObservableCollection<string>();
    }
}
