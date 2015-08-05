using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Dragablz;
using MahApps.Metro;
using Ninject;
using NodeMachine.Annotations;
using NodeMachine.Connection;
using NodeMachine.Model.Project;
using NodeMachine.View.Controls;
using NodeMachine.ViewModel.Tabs;

namespace NodeMachine.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
        : INotifyPropertyChanged
    {
        private readonly IKernel _kernel;

        private readonly IGameConnection _connection;
        public IGameConnection Connection
        {
            get
            {
                return _connection;
            }
        }

        private readonly IInterTabClient _interTabClient;
        public IInterTabClient InterTabClient
        {
            get { return _interTabClient; }
        }

        private readonly IProjectManager _projectManager;
        public IProjectManager ProjectManager
        {
            get
            {
                return _projectManager;
            }
        }

        private readonly ObservableCollection<TabContent> _tabContents = new ObservableCollection<TabContent>();
        // ReSharper disable ReturnTypeCanBeEnumerable.Global
        public ObservableCollection<TabContent> TabContents
        // ReSharper restore ReturnTypeCanBeEnumerable.Global
        {
            get { return _tabContents; }
        }

        private readonly IInterLayoutClient _interLayoutClient;
        public IInterLayoutClient InterLayoutClient
        {
            get
            {
                return _interLayoutClient;
            }
        }

        public MainWindow(IKernel kernel, IGameConnection connection, IInterTabClient interTab, IInterLayoutClient interLayout, IProjectManager projectManager)
        {
            _kernel = kernel;
            _connection = connection;
            _interTabClient = interTab;
            _interLayoutClient = interLayout;
            _projectManager = projectManager;

            InitializeComponent();

            Loaded += WindowLoaded;
        }

        private async void WindowLoaded(object sender, RoutedEventArgs e)
        {
            await WaitForGameConnection();
        }

        public Func<object> NewItemFactory
        {
            get { return () => new TabContent(_kernel.Get<NewTabControl>()); }
        }

        private void OnSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            SettingsFlyout.IsOpen = !SettingsFlyout.IsOpen;
        }

        private bool _isConnecting;
        public bool IsConnecting
        {
            get
            {
                return _isConnecting;
            }
            set
            {
                _isConnecting = value;
                OnPropertyChanged();
            }
        }

        private async Task WaitForGameConnection()
        {
            IsConnecting = true;
            var start = DateTime.Now;

            var t = _connection.Connect();
            await Task.Delay(100);

            if (!t.IsCompleted)
            {
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent("Crimson"), ThemeManager.GetAppTheme("BaseLight"));
                await t;
            }

            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent("Blue"), ThemeManager.GetAppTheme("BaseLight"));

            var remainingTime = TimeSpan.FromSeconds(2) - (DateTime.Now - start);
            if (remainingTime > TimeSpan.Zero)
                await Task.Delay(remainingTime);
            IsConnecting = false;
        }

        private async void OnRefreshConnectionClick(object sender, RoutedEventArgs e)
        {
            await WaitForGameConnection();
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

        private bool _suppressClosingSave = false;

        private async void WindowClosing(object sender, CancelEventArgs e)
        {
            //We only want to do this if we're the last window!
            if (Application.Current.Windows.Count > 1 || _suppressClosingSave)
                return;

            //Check if we actually have anything to save
            var manager = _kernel.Get<IProjectManager>();
            if (!manager.CurrentProject.UnsavedChanges)
                return;

            //Cancel the close
            e.Cancel = true;

            //Save, and then close again
            if (await ProjectControl.SaveUnsavedChanges(manager, this))
            {
                _suppressClosingSave = true;
                Close();
            }
        }
    }
}
