using System.Collections.Generic;
using BeautifulBlueprints.Elements;
using BeautifulBlueprints.Layout;
using BeautifulBlueprints.Layout.Svg;
using BeautifulBlueprints.Serialization;
using Construct_Gamemode.Map;
using Construct_Gamemode.Map.Facade;
using Construct_Gamemode.Map.Models;
using Newtonsoft.Json;
using NodeMachine.Connection;
using NodeMachine.Model;
using NodeMachine.Model.Project;
using NodeMachine.ViewModel.Tabs;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Color = System.Windows.Media.Color;
using Path = BeautifulBlueprints.Elements.Path;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

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

        protected override ObservableCollection<Facade> ProjectDataModelCollection
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

            //Preconditions for subdivision
            if (PreviewWidthValue == null || PreviewHeightValue == null || ShowAllNodes == null)
                return;
            if (!PreviewWidthValue.Value.HasValue || !PreviewHeightValue.Value.HasValue || !ShowAllNodes.IsChecked.HasValue)
                return;

            //Layout facade
            var text = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd).Text;
            var node = Yaml.Deserialize(new StringReader(text));
            var w = PreviewWidthValue.Value.Value;
            var h = PreviewHeightValue.Value.Value;
            var solution = Solver.Solve(-w / 2m, w / 2m, h / 2m, -h / 2m, node).ToArray();
            var paths = solution.Where(a => a.Tag is PathLayout).ToArray();

            //Create the facade in game
            await Connection.Topology.SetRoot(Guid.Parse("CFF595C4-4C67-4CCB-9E5F-AB9AE0F9AF54"), new RemoteRootInit {
                Children = new ChildDefinition[] {
                    new ChildDefinition {
                        Prism = new PrismModel(new[] {new Point2(-w / 200f, 0.1f), new Point2(w / 200f, 0.1f), new Point2(w / 200f, -0.1f), new Point2(-w / 200f, -0.1f)}, h / 100f),
                        ChildType = "Construct_Gamemode.Map.Facade.RemoteFacade",
                        Center = new Point3(1, 1, 1),
                        ChildData = JsonConvert.SerializeObject(new RemoteFacadeInit {
                            Stamps = paths.SelectMany(a => Convert((PathLayout)a.Tag, (Path)a.Element)).ToArray()
                        })
                    }
                }
            });
        }

        private IEnumerable<RemoteStamp> Convert(PathLayout path, Path element)
        {
            var shapes = ExtractShapes(path);

            foreach (var shape in shapes)
            {
                yield return new RemoteStamp() {
                    Additive = element.Additive,
                    StartDepth = (float)element.StartDepth,
                    EndDepth = (float)(element.StartDepth + element.Thickness),
                    Material = element.Brush,
                    Shape = shape.ToArray()
                };
            }
        }

        private IEnumerable<List<Point2>> ExtractShapes(PathLayout path)
        {
            List<List<Point2>> collection = new List<List<Point2>>();

            List<Point2> polygon = null;
            foreach (var point in path.Points)
            {
                if (point.StartOfLine)
                {
                    if (polygon != null)
                        collection.Add(polygon);
                    polygon = new List<Point2>();
                }

// ReSharper disable PossibleNullReferenceException
                var p = new Point2((float)point.X / 100f, (float)point.Y / 100f);
                if (!polygon.Contains(p))
// ReSharper restore PossibleNullReferenceException
                    polygon.Add(p);
            }

            if (polygon != null)
                collection.Add(polygon);
            return collection;
        }

        private void CheckMarkup(object sender, RoutedEventArgs e)
        {
            RenderPreview();
        }

        private void Editor_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab && (Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
            {
                var richTextBox = (RichTextBox) sender;
                if (richTextBox == null)
                    return;

                richTextBox.Selection.Text = string.Empty;

                var caretPosition = richTextBox.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);

                richTextBox.CaretPosition.InsertTextInRun("  ");
                richTextBox.CaretPosition = caretPosition;
                e.Handled = true;
            }
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

        private void ShowShapesCheckedChanged(object sender, RoutedEventArgs e)
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

                var node = Yaml.Deserialize(new StringReader(text));

                var w = PreviewWidthValue.Value.Value;
                var h = PreviewHeightValue.Value.Value;
                var solution = Solver.Solve(-w / 2m, w / 2m, h / 2m, -h / 2m, node).ToArray();

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

                            Width = (double)(sol.Right - sol.Left),
                            Height = (double)(sol.Top - sol.Bottom)
                        };

                        Canvas.SetLeft(r, (double)sol.Left);
                        Canvas.SetTop(r, (double)sol.Bottom);   //Yes, setting top to bottom is correct... who knows?
                        PreviewCanvas.Children.Add(r);
                    }

                    foreach (var sol in solution)
                    {
                        var path = sol.Tag as PathLayout;
                        if (path != null)
                            RenderPath(path, (Path)sol.Element);
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

        private void RenderPath(PathLayout path, Path fill)
        {
            if (ShowShapeNodes.IsChecked.HasValue && !ShowShapeNodes.IsChecked.Value)
                return;

            var points = path.Points.ToArray();

            //Connect the points
            Polygon polygon = null;
            

            for (int i = 0; i < points.Length - 1; i++)
            {
                if (points[i].StartOfLine)
                {
                    if (polygon != null)
                        PreviewCanvas.Children.Add(polygon);

                    SolidColorBrush brush;
                    var b = Math.Max(-255, Math.Min(255, fill.Thickness * 255));
                    if (fill.Thickness < 0)
                        brush = new SolidColorBrush(Color.FromRgb((byte)-b, 0, (byte)-b));
                    else
                        brush = new SolidColorBrush(Color.FromRgb((byte)b, (byte)b, (byte)b));

                    polygon = new Polygon() {
                        Stroke = Brushes.DarkBlue,
                        StrokeThickness = 2,
                        Fill = brush
                    };
                }

                // ReSharper disable once PossibleNullReferenceException
                polygon.Points.Add(new Point((double)points[i].X, (double)points[i].Y));
            }

            if (polygon != null)
                PreviewCanvas.Children.Add(polygon);

            //Draw the points
            foreach (var point in points)
            {
                var r = new Rectangle
                {
                    Fill = Brushes.LightBlue,
                    Width = 3,
                    Height = 3
                };

                Canvas.SetLeft(r, (double)point.X - 1.5);
                Canvas.SetTop(r, (double)point.Y - 1.5);
                PreviewCanvas.Children.Add(r);
            }
        }

        private void SaveToModel()
        {
            Value.Markup = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd).Text.TrimEnd('\r', '\n');
        }

        private bool _suppressSave = false;
        private void Editor_OnLoaded(object sender, EventArgs e)
        {
            _suppressSave = true;
            try
            {
                Editor.Document.Blocks.Clear();
                Editor.Document.Blocks.Add(new Paragraph(new Run(Value.Markup)));
            }
            finally
            {
                _suppressSave = false;
            }
        }

        private void Editor_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (Editor.IsLoaded && !_suppressSave)
                SaveToModel();
        }
    }
}
