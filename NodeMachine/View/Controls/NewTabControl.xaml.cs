using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Ninject;
using NodeMachine.Annotations;
using NodeMachine.Connection;
using NodeMachine.ViewModel.Tabs;

namespace NodeMachine.View.Controls
{
    /// <summary>
    /// Interaction logic for NewTabControl.xaml
    /// </summary>
    public partial class NewTabControl : UserControl, ITabName
    {
        private readonly IKernel _kernel;
        private readonly IGameConnection _connection;

        private string _tabName = "New Tab";
        public string TabName
        {
            get
            {
                return _tabName;
            }
            set
            {
                _tabName = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<TileOption> ControlTiles
        {
            get
            {
                yield return new TileOption("Start Game", () => {
                    // ReSharper disable once CSharpWarnings::CS4014
                    StartGame();
                    return null;
                });

                yield return new TileOption("Project", () => _kernel.Get<ProjectControl>());
                yield return new TileOption("Status Monitor", () => _kernel.Get<StatusControl>());
                yield return new TileOption("Topology Tree", () => _kernel.Get<NodeTree>());
                yield return new TileOption("City Editor", () => _kernel.Get<CityEditor>());
                yield return new TileOption("Block Editor", () => _kernel.Get<BlockEditor>());
                yield return new TileOption("Building Editor", () => _kernel.Get<BuildingEditor>());
                yield return new TileOption("Floor Editor", () => _kernel.Get<FloorEditor>());
                //yield return new TileOption("Room Editor", () => null);
                yield return new TileOption("Facade Editor", () => _kernel.Get<FacadeEditor>());
                yield return new TileOption("Compile", () => _kernel.Get<CompileControl>());
            }
        }

        public NewTabControl()
        {
            InitializeComponent();
        }

        public NewTabControl(TileOption option)
        {
            InitializeComponent();

            SetContent(option);
        }

        private async Task StartGame()
        {
            var window = (MetroWindow)Window.GetWindow(this);
            var progress = await window.ShowProgressAsync("Starting Game", "", false, new MetroDialogSettings {
                AnimateHide = false
            });

            //Check that we're on the main menu
            progress.SetMessage("Checking Menu State");
            var head = await _connection.Screens.Head();
            if (head == null || head.Name != "Epimetheus.UI.Screens.MainMenu2.MainMenu")
            {
                await WaitForCancel(progress, "Not On Main Menu");
                return;
            }

            //Close progress dialog, ask user some questions, then show progress again
            await progress.CloseAsync();
            var gamemode = await window.ShowInputAsync("Game Mode", "Which Gamemode Should Be Started?", new MetroDialogSettings {
                DefaultText = "Construct_Gamemode.Gamemode.Construct, Construct Gamemode",
                AnimateShow = false,
                AnimateHide = false,
            });
            var root = await window.ShowInputAsync("Root Node", "Which Root Node Should Be Used?", new MetroDialogSettings {
                DefaultText = "Construct_Gamemode.Map.WsRoot, Construct Gamemode",
                AnimateShow = false,
                AnimateHide = false
            });
            progress = await window.ShowProgressAsync("Starting Game", "", false, new MetroDialogSettings {
                AnimateShow = false
            });

            //Check that the game has the mods installed for the specified gamemode and root node
            //if (!_connection.Modifications.Scripts(gamemode).Any())
            //{
            //    await WaitForCancel(progress, "Cannot Find Specified Gamemode");
            //    return;
            //}

            //if (!_connection.Modifications.Scripts(root).Any())
            //{
            //    await WaitForCancel(progress, "Cannot Find Specified Root Node");
            //    return;
            //}

            await WaitForCancel(progress, "Not Implemented...");
            return;

            progress.SetMessage("Creating Lobby");
            await Task.Delay(1000);

            progress.SetMessage("Configuring Lobby");
            await Task.Delay(1000);

            progress.SetMessage("Starting Game");
            await Task.Delay(1000);

            await progress.CloseAsync();
        }

        private static async Task WaitForCancel(ProgressDialogController dialog, string message)
        {
            dialog.SetMessage("Failed: " + message);
            dialog.SetCancelable(true);

            while (!dialog.IsCanceled)
                await Task.Delay(10);

            await dialog.CloseAsync();
        }

        private void TileClicked(object sender, RoutedEventArgs args)
        {
            var clicked = (TileOption)((Control)sender).DataContext;

            SetContent(clicked);
        }

        public void SetContent(TileOption option)
        {
            var content = option.Control();
            if (content == null)
                return;

            Container.Children.Clear();
            Container.Children.Add(content);
            TabName = option.Title;

            OptionsSelector.Visibility = Visibility.Collapsed;
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

        public NewTabControl(IKernel kernel, IGameConnection connection)
        {
            _kernel = kernel;
            _connection = connection;
            InitializeComponent();
        }

        public class TileOption
        {
            private readonly Func<UIElement> _child;

            public string Title { get; private set; }

            public TileOption(string title, Func<UIElement> child)
            {
                _child = child;
                Title = title;
            }

            public UIElement Control()
            {
                return _child();
            }
        }
    }
}
