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

        private async void SaveProject(object sender, RoutedEventArgs e)
        {
            if (ProjectManager.CurrentProject != null)
                await ProjectManager.CurrentProject.Save();
        }

        private async Task SaveUnsavedChanges()
        {
            if (ProjectManager.CurrentProject != null && ProjectManager.CurrentProject.UnsavedChanges)
            {
                var window = (MetroWindow)Window.GetWindow(this);
                var result = await window.ShowMessageAsync("Unsaved Changes", string.Format("Save '{0}' First?", "Name"), MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, new MetroDialogSettings
                {
                    AffirmativeButtonText = "Save",
                    NegativeButtonText = "Discard",
                    FirstAuxiliaryButtonText = "Cancel"
                });

                if (result == MessageDialogResult.FirstAuxiliary)
                    return;
                if (result == MessageDialogResult.Affirmative)
                    await ProjectManager.CurrentProject.Save();
            }
        }

        private async Task OpenProjectDialog(bool checkExists)
        {
            var d = new OpenFileDialog
            {
                Filter = "NODE/Machine Project|*.nmproj",
                CheckPathExists = true,
                CheckFileExists = checkExists,
                Multiselect = false
            };

            //This is a bool? so a direct compare to a boolean value *does* make sense here
            if (d.ShowDialog() != true)
                return;

            await ProjectManager.OpenProject(d.FileName);
        }

        private async void NewProject(object sender, RoutedEventArgs e)
        {
            await SaveUnsavedChanges();
            await OpenProjectDialog(false);
        }

        private void OpenProjectInExplorer(object sender, RoutedEventArgs e)
        {
            if (ProjectManager.CurrentProject != null)
                Process.Start(Path.GetDirectoryName(ProjectManager.CurrentProject.ProjectFile));
        }

        private async void OpenProject(object sender, RoutedEventArgs e)
        {
            await SaveUnsavedChanges();
            await OpenProjectDialog(true);
        }

        private async void ReloadProject(object sender, RoutedEventArgs e)
        {
            await SaveUnsavedChanges();
            if (ProjectManager.CurrentProject != null)
                await ProjectManager.CurrentProject.Load();
        }
    }
}
