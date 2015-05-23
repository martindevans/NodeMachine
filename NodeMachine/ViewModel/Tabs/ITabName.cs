
using System.ComponentModel;

namespace NodeMachine.ViewModel.Tabs
{
    public interface ITabName
        : INotifyPropertyChanged
    {
        string TabName { get; }
    }
}
