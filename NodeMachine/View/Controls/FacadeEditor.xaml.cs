using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using BeautifulBlueprints.Elements;
using BeautifulBlueprints.Layout;
using EpimetheusPlugins.Procedural;
using NodeMachine.Connection;
using NodeMachine.Model;
using NodeMachine.Model.Project;
using NodeMachine.ViewModel.Tabs;
using System.Collections.Generic;
using System.Windows;

namespace NodeMachine.View.Controls
{
    /// <summary>
    /// Interaction logic for FacadeEditor.xaml
    /// </summary>
    public partial class FacadeEditor : BaseEditorControl<Facade>, ITabName
    {
        public FacadeEditor(IProjectManager manager, IGameConnection connection, Facade facade)
            : base(manager, connection, facade)
        {
            InitializeComponent();
        }

        protected override ICollection<Facade> ProjectDataModelCollection
        {
            get
            {
                return ProjectManager.CurrentProject.ProjectData.Facades;
            }
        }

        protected override string ValueName
        {
            get
            {
                return Value.Name;
            }
        }

        protected async void SendToGame(object sender, RoutedEventArgs e)
        {
            if (!await Connection.IsConnected())
                return;

            //todo: pass in a procedural script
            await SendToGame(new Prism(), (ProceduralScript)null);
        }

        private void CheckMarkup(object sender, RoutedEventArgs e)
        {
            RenderPreview();
        }

        private void Editor_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Tab || (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                return;

            var richTextBox = (RichTextBox)sender;
            if (richTextBox == null) return;

            richTextBox.Selection.Text = string.Empty;

            var caretPosition = richTextBox.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);

            richTextBox.CaretPosition.InsertTextInRun("  ");
            richTextBox.CaretPosition = caretPosition;
            e.Handled = true;
        }

        private void PreviewSizeChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!IsInitialized)
                return;

            RenderPreview();
        }

        private void ShowAllCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized)
                return;

            RenderPreview();
        }

        private void RenderPreview()
        {
            if (PreviewWidthValue == null || PreviewHeightValue == null || ShowAllNodes == null)
                return;
            if (!PreviewWidthValue.Value.HasValue || !PreviewHeightValue.Value.HasValue || !ShowAllNodes.IsChecked.HasValue)
                return;

            var text = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd).Text;

            try
            {
                PreviewCanvas.Children.Clear();

                var node = BeautifulBlueprints.Serialization.Yaml.Deserialize(new StringReader(text));

                var w = PreviewWidthValue.Value.Value;
                var h = PreviewHeightValue.Value.Value;
                var solution = Solver.Solve(-w / 2f, w / 2f, h / 2f, -h / 2f, node).ToArray();

                StringBuilder output = new StringBuilder();
                if (solution.Length > 0)
                {
                    foreach (var sol in solution)
                    {
                        var container = sol.Element as BaseContainerElement;
                        if (!ShowAllNodes.IsChecked.Value && container != null && container.Children.Any())
                            continue;

                        output.AppendLine(string.Format("{0}\tL{1:0.0}\tR{2:0.0}\tT{3:0.0}\tB{4:0.0}", sol.Element.Name, sol.Left, sol.Right, sol.Top, sol.Bottom));

                        var r = new Rectangle
                        {
                            Stroke = Brushes.LightBlue,
                            StrokeThickness = 1,

                            Width = sol.Right - sol.Left,
                            Height = sol.Top - sol.Bottom
                        };

                        Canvas.SetLeft(r, sol.Left);
                        Canvas.SetTop(r, sol.Bottom);   //Yes, setting top to bottom is correct... who knows?
                        PreviewCanvas.Children.Add(r);
                    }
                }
                else
                {
                    output.AppendLine("Layout failed");
                }

                CompilationOutput.Text = output.ToString();
            }
            catch (Exception err)
            {
                CompilationOutput.Text = err.ToString();
            }
        }
    }
}
