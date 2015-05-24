using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using NodeMachine.Project;
using System.Windows.Controls;

namespace NodeMachine.View.Controls
{
    /// <summary>
    /// Interaction logic for ProjectControl.xaml
    /// </summary>
    public partial class ProjectControl : UserControl
    {
        public IProjectManager ProjectManager { get; private set; }

        public ProjectControl(IProjectManager projectManager)
        {
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

        public static async Task<bool> SaveUnsavedChanged(IProjectManager projectManager, MetroWindow window, bool preSavePrompt = true, bool forceChooseFile = false)
        {
            if (preSavePrompt)
            {
                if (!projectManager.CurrentProject.UnsavedChanges)
                    return true;

                var result = await window.ShowMessageAsync("Unsaved Changes", string.Format("Save '{0}' First?", "Name"), MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, new MetroDialogSettings
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
            if (preSavePrompt)
            {
                if (!ProjectManager.CurrentProject.UnsavedChanges)
                    return true;

                var window = (MetroWindow) Window.GetWindow(this);
                var result = await window.ShowMessageAsync("Unsaved Changes", string.Format("Save '{0}' First?", "Name"), MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, new MetroDialogSettings {
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
            if (ProjectManager.CurrentProject.ProjectFile == null || forceChooseFile)
            {
                var path = await ChooseSaveProjectFile(false);
                if (path == null)
                    return false;
                ProjectManager.CurrentProject.ProjectFile = path;
            }

            await ProjectManager.CurrentProject.Save();
            return true;
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
            if (!await SaveUnsavedChanges())
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

        private void DeleteProject(object sender, RoutedEventArgs e)
        {
            if (ProjectManager.CurrentProject.ProjectFile != null)
                File.Delete(ProjectManager.CurrentProject.ProjectFile);

            ProjectManager.CurrentProject.ProjectFile = null;
        }
    }
}
