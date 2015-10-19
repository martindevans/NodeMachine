using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NodeMachine.Compiler;
using NodeMachine.Model.Project;
using Clipboard = System.Windows.Forms.Clipboard;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using UserControl = System.Windows.Controls.UserControl;

namespace NodeMachine.View.Controls
{
    /// <summary>
    /// Interaction logic for CompileControl.xaml
    /// </summary>
    public partial class CompileControl : UserControl
    {
        public IProjectManager ProjectManager { get; private set; }

        public CompileControl(IProjectManager projectManager)
        {
            ProjectManager = projectManager;

            InitializeComponent();
        }

        private void RegenerateProjectGuid(object sender, RoutedEventArgs e)
        {
            ProjectManager.CurrentProject.ProjectData.Guid = Guid.NewGuid();
        }

        private void ChangeProjectCompileLocation(object sender, RoutedEventArgs e)
        {
            //Show dialog to select a folder to output the mod into
            var d = new FolderBrowserDialog() {
                Description = "Choose Location For Compiled Project",
                SelectedPath = Path.GetDirectoryName(ProjectManager.CurrentProject.ProjectFile),
                ShowNewFolderButton = true,
            };

            //Exit if nowhere is selected
            if (d.ShowDialog() != DialogResult.OK)
                return;

            //Set the directory into the project
            ProjectManager.CurrentProject.ProjectData.CompileOutputDirectory = d.SelectedPath;
        }

        private void CopyProjectCompileLocation(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ProjectManager.CurrentProject.ProjectData.CompileOutputDirectory);
        }

        private async void CompileProject(object sender, RoutedEventArgs e)
        {
            //Ensure the output directory exists
            if (!Directory.Exists(ProjectManager.CurrentProject.ProjectData.CompileOutputDirectory))
                Directory.CreateDirectory(ProjectManager.CurrentProject.ProjectData.CompileOutputDirectory);

            //Show a progress dialog whilst compiling
            var window = (MetroWindow)Window.GetWindow(this);
            var progress = await window.ShowProgressAsync("Compiling...", string.Format("Compiling Project \"{0}\"", ProjectManager.CurrentProject.ProjectData.Name), false, new MetroDialogSettings {
                AnimateHide = false,
                AnimateShow = true,
            });
            progress.SetIndeterminate();

            //Delay to give the progress dialog time to show up
            await Task.Delay(100);

            //Compile the project
            var compiler = new ProjectCompiler(ProjectManager.CurrentProject, new CompileSettings());
            var errors = new List<string>();
            var success = await compiler.Compile(errors);

            //Show progress bar as completed
            progress.SetProgress(1);

            //Begin closing the progress display in the background
            var pClose = progress.CloseAsync();

            //Display completion messages
            if (success)
            {
                await ((MetroWindow)Window.GetWindow(this)).ShowMessageAsync("Complete", string.Format("Project \"{0}\" Compiled Successfully", ProjectManager.CurrentProject.ProjectData.Name), MessageDialogStyle.Affirmative, new MetroDialogSettings {
                    AnimateHide = true,
                    AnimateShow = false,
                });
            }
            else
            {
                await ((MetroWindow)Window.GetWindow(this)).ShowMessageAsync("Error",
                    string.Format("Project \"{0}\" Failed To Compile:\n{1}",
                        ProjectManager.CurrentProject.ProjectData.Name,
                        string.Join("\n", errors)
                    ), MessageDialogStyle.Affirmative, new MetroDialogSettings
                {
                    AnimateHide = true,
                    AnimateShow = false,
                });
            }

            //Finish closing the progress display
            await pClose;
        }
    }
}
