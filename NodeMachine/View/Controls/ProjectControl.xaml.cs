using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Dragablz;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Ninject;
using Ninject.Parameters;
using NodeMachine.Model;
using NodeMachine.Model.Project;
using System.Windows.Controls;
using NodeMachine.ViewModel.Tabs;

namespace NodeMachine.View.Controls
{
    /// <summary>
    /// Interaction logic for ProjectControl.xaml
    /// </summary>
    public partial class ProjectControl : UserControl
    {
        private readonly IKernel _kernel;
        public IProjectManager ProjectManager { get; private set; }

        public ProjectControl(IKernel kernel, IProjectManager projectManager)
        {
            _kernel = kernel;
            ProjectManager = projectManager;

            InitializeComponent();
        }

        private static async Task<string> ChooseOpenProjectFile(bool checkExists)
        {
            var d = new OpenFileDialog
            {
                Filter = "NODE/Machine Project|*.nmproj",
                CheckPathExists = true,
                CheckFileExists = checkExists,
                Multiselect = false,
            };

            //This is a bool? so a direct compare to a boolean value *does* make sense here
            if (d.ShowDialog() != true)
                return null;

            return d.FileName;
        }

        private static async Task<string> ChooseSaveProjectFile(bool checkExists)
        {
            var d = new SaveFileDialog
            {
                Filter = "NODE/Machine Project|*.nmproj",
                CheckPathExists = true,
                CheckFileExists = checkExists,
            };

            //This is a bool? so a direct compare to a boolean value *does* make sense here
            if (d.ShowDialog() != true)
                return null;

            return d.FileName;
        }

        public static async Task<bool> SaveUnsavedChanges(IProjectManager projectManager, MetroWindow window, bool preSavePrompt = true, bool forceChooseFile = false)
        {
            if (preSavePrompt)
            {
                if (!projectManager.CurrentProject.UnsavedChanges)
                    return true;

                var result = await window.ShowMessageAsync("Unsaved Changes", string.Format("Save '{0}' First?", projectManager.CurrentProject.ProjectData.Name ?? "Unnamed Project"), MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, new MetroDialogSettings
                {
                    AffirmativeButtonText = "Save",
                    NegativeButtonText = "Discard",
                    FirstAuxiliaryButtonText = "Cancel"
                });

                //Cancel option
                if (result == MessageDialogResult.FirstAuxiliary)
                    return false;

                //Discard option
                if (result == MessageDialogResult.Negative)
                    return true;
            }

            //Save option
            if (projectManager.CurrentProject.ProjectFile == null || forceChooseFile)
            {
                var path = await ChooseSaveProjectFile(false);
                if (path == null)
                    return false;
                projectManager.CurrentProject.ProjectFile = path;
            }

            await projectManager.CurrentProject.Save();
            return true;
        }

        private async Task<bool> SaveUnsavedChanges(bool preSavePrompt = true, bool forceChooseFile = false)
        {
            return await SaveUnsavedChanges(ProjectManager, (MetroWindow)Window.GetWindow(this), preSavePrompt, forceChooseFile);
        }

        private async Task OpenProjectDialog(bool checkExists)
        {
            var path = await ChooseOpenProjectFile(checkExists);
            if (path == null)
                return;

            await ProjectManager.OpenProject(path);
        }

        private async void NewProject(object sender, RoutedEventArgs e)
        {
            if (!await SaveUnsavedChanges())
                return;
            await ProjectManager.CloseProject();

            await OpenProjectDialog(false);
        }

        private async void OpenProject(object sender, RoutedEventArgs e)
        {
            if (!await SaveUnsavedChanges())
                return;
            await OpenProjectDialog(true);
        }

        private async void ReloadProject(object sender, RoutedEventArgs e)
        {
            if (!await SaveUnsavedChanges(true, true))
                return;

            if (ProjectManager.CurrentProject.ProjectFile != null)
                await ProjectManager.OpenProject(ProjectManager.CurrentProject.ProjectFile);
        }

        private void OpenProjectInExplorer(object sender, RoutedEventArgs e)
        {
            if (ProjectManager.CurrentProject.ProjectFile != null)
            {
                var dir = Path.GetDirectoryName(ProjectManager.CurrentProject.ProjectFile);
                if (dir != null)
                    Process.Start(dir);
            }
        }

        private async void SaveProject(object sender, RoutedEventArgs e)
        {
            await SaveUnsavedChanges(preSavePrompt: false);
        }

        private async void SaveProjectAs(object sender, RoutedEventArgs e)
        {
            await SaveUnsavedChanges(false, true);
        }

        private async void DeleteProject(object sender, RoutedEventArgs e)
        {
            var window = (MetroWindow)Window.GetWindow(this);
            var result = await window.ShowMessageAsync("Delete", string.Format("Delete '{0}'?", ProjectManager.CurrentProject.ProjectData.Name ?? "Unnamed Project"), MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
            {
                AffirmativeButtonText = "Delete",
                NegativeButtonText = "Cancel"
            });

            if (result == MessageDialogResult.Negative)
                return;

            if (ProjectManager.CurrentProject.ProjectFile != null)
                File.Delete(ProjectManager.CurrentProject.ProjectFile);
            ProjectManager.CurrentProject.ProjectFile = null;
        }

        private void OpenInEditor(object sender, RoutedEventArgs e)
        {
            var context = ((Control)sender).DataContext;
            var window = (MainWindow)Window.GetWindow(this);
            if (window == null)
                throw new NullReferenceException();

            //if (OpenEditor<CityEditor, City>(window, context, "city"))
            //    return;

            if (OpenEditor<BlockEditor, Block>(window, context, "block"))
                return;

            if (OpenEditor<BuildingEditor, Building>(window, context, "building"))
                return;

            if (OpenEditor<FloorEditor, Floor>(window, context, "floor"))
                return;

            //if (OpenEditor<RoomEditor, Room>(window, context, "room"))
            //    return;

            //if (OpenEditor<MiscNodeEditor, MiscNode>(window, context, "node"))
            //    return;

            if (OpenEditor<FacadeEditor, Facade>(window, context, "facade"))
                return;

            throw new NotImplementedException();
        }

        private bool OpenEditor<TEditor, TArg>(MainWindow window, object context, string argName)
            where TArg : class
            where TEditor : Control
        {
            var arg = context as TArg;
            if (arg == null)
                return false;

            window.TabContents.Add(new TabContent(_kernel.Get<TEditor>(new ConstructorArgument(argName, arg))));
            return true;
        }
    }
}
