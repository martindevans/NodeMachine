using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Base_CityGeneration.Elements.Building.Design;
using Base_CityGeneration.Elements.Building.Internals.Floors.Design;
using Base_CityGeneration.Elements.Building.Internals.Floors.Plan;
using EpimetheusPlugins.Testing.MockScripts;
using Myre.Collections;
using NodeMachine.Connection;
using NodeMachine.Model;
using NodeMachine.Model.Project;
using NodeMachine.ViewModel.Tabs;
using SwizzleMyVectors.Geometry;

namespace NodeMachine.View.Controls
{
    /// <summary>
    /// Interaction logic for FloorEditor.xaml
    /// </summary>
    public partial class FloorEditor : BaseYamlEditorControl<Floor>, ITabName
    {
        public IReadOnlyDictionary<string, FloorPlanShape> Shapes { get { return FloorPlanShapes.Shapes; } }

        private Vector2 _canvasPosition;
        private Vector2 _previousMouse;
        private bool _isMouseCaptured;

        private string _selectedShape = FloorPlanShapes.Shapes.First().Key;

        public string SelectedShape
        {
            get { return _selectedShape; }
            set { _selectedShape = value; }
        }

        protected override string ValueMarkup
        {
            get { return Value.Markup; }
            set { Value.Markup = value; }
        }

        protected override string ValueName
        {
            get
            {
                return Value.Name;
            }
        }

        public FloorEditor(IProjectManager manager, IGameConnection connection, Floor floor)
            : base(manager, connection, floor)
        {
            InitializeComponent();
        }

        protected override ObservableCollection<Floor> ProjectDataModelCollection
        {
            get
            {
                return ProjectManager.CurrentProject.ProjectData.Floors;
            }
        }

        protected override void CheckMarkup(object sender, RoutedEventArgs e)
        {
            RenderPreview();
        }

        private async void RenderPreview()
        {
            if (Seed == null || !Seed.Value.HasValue)
                return;
            if (CompilationOutput == null)
                return;
            if (PreviewCanvas == null)
                return;
            if (GeneratingIndicator == null)
                return;
            if (Scale == null || !Scale.Value.HasValue)
                return;

            GeneratingIndicator.IsBusy = true;
            CompilationOutput.Text = "";
            PreviewCanvas.Children.Clear();

            var w = new Stopwatch();
            w.Start();

            try
            {
                var txt = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd).Text;
                if (string.IsNullOrWhiteSpace(txt))
                {
                    CompilationOutput.Text = "Error: No Markup!";
                }
                else
                {
                    var shape = Shapes[SelectedShape];

                    var r = new Random(Seed.Value.Value);
                    var m = new NamedBoxCollection();
                    var spec = Deserialize(txt, r.NextDouble, m);
                    var layout = await GenerateLayout(spec, r.NextDouble, m, shape);

                    DrawLayout(layout, Scale.Value.Value);
                }
            }
            catch (Exception err)
            {
                CompilationOutput.Text = err.ToString();
            }

            GeneratingIndicator.IsBusy = false;
        }

        private static Task<IFloorPlan> GenerateLayout(FloorDesigner spec, Func<double> random, NamedBoxCollection metadata, FloorPlanShape footprint)
        {
            return Task<IFloorPlan>.Factory.StartNew(() => {
                Func<IEnumerable<KeyValuePair<string, string>>, Type[], EpimetheusPlugins.Scripts.ScriptReference> s = (a, b) => ScriptReferenceFactory.Create(typeof(NullScript), Guid.Empty, string.Join(",", a));

                return spec.Design(
                    random,
                    metadata,
                    s,
                    footprint.Footprint,
                    footprint.Subsections,
                    0.075f,
                    footprint.Verticals,

                    //todo: verticals starting on this floor
                    new List<VerticalSelection>()
                );
            });
        }

        private static FloorDesigner Deserialize(string text, Func<double> random, INamedDataCollection metadata)
        {
            return FloorDesigner.Deserialize(new StringReader(text));
        }

        protected override void SendToGame(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SeedChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            RenderPreview();
        }

        private void ScaleChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            RenderPreview();
        }

        private void OpenHelpUrl(object sender, RoutedEventArgs e)
        {
            Process.Start("https://bitbucket.org/martindevans/base-citygeneration/src/d90d4251cf2a0d11bea7242a56ae318713de1cf6/Base-CityGeneration/Elements/Building/Internals/Floors/Design/FloorSpec.md");
        }

        private void DrawLayout(IFloorPlan plan, int scale)
        {
            //Draw outline
            //todo: rewrite floorplan outline rendering to show external windows and doors
            var outline = new Polygon {
                Stroke = Brushes.Gray,
                Fill = Brushes.Cornsilk,
                StrokeThickness = 2
            };
            foreach (var point in plan.ExternalFootprint)
                outline.Points.Add(new Point(point.X * scale, point.Y * scale));
            PreviewCanvas.Children.Add(outline);

            //Add Rooms
            foreach (var room in plan.Rooms)
            {
                //Sections
                foreach (var facade in room.GetWalls())
                {
                    var facadePolygon = new Polygon {
                        Stroke = facade.IsExternal ? Brushes.Green : Brushes.CornflowerBlue,
                        StrokeThickness = 1,
                        Fill = Brushes.DeepSkyBlue
                    };
                    facadePolygon.Points.Add(new Point(facade.Section.Inner1.X * scale, facade.Section.Inner1.Y * scale));
                    facadePolygon.Points.Add(new Point(facade.Section.Inner2.X * scale, facade.Section.Inner2.Y * scale));
                    facadePolygon.Points.Add(new Point(facade.Section.Outer1.X * scale, facade.Section.Outer1.Y * scale));
                    facadePolygon.Points.Add(new Point(facade.Section.Outer2.X * scale, facade.Section.Outer2.Y * scale));
                    PreviewCanvas.Children.Add(facadePolygon);
                }

                var fillPolygon = new Polygon {
                    Stroke = null,
                    Fill = Brushes.LightBlue
                };
                foreach (var vector2 in room.InnerFootprint)
                    fillPolygon.Points.Add(new Point(vector2.X * scale, vector2.Y * scale));
                PreviewCanvas.Children.Add(fillPolygon);

                var middle = (room.InnerFootprint.Aggregate((a, b) => a + b) / room.InnerFootprint.Count) * scale;

                var text = new TextBlock {
                    Text = room.Id.ToString(),
                    FontSize = 1.7f * scale / 2f,
                    Foreground = Brushes.MidnightBlue,
                    RenderTransform = new ScaleTransform(1, -1),
                    HorizontalAlignment = HorizontalAlignment.Center,
                };

                PreviewCanvas.Children.Add(text);
                text.UpdateLayout();

                var bound = BoundingRectangle.CreateFromPoints(room.OuterFootprint);
                if (bound.Extent.Y > bound.Extent.X)
                {
                    var group = new TransformGroup();
                    group.Children.Add(new ScaleTransform(-1, 1));
                    group.Children.Add(new RotateTransform(270, text.ActualWidth / 2f, text.ActualHeight / 2f));
                    group.Children.Add(new TranslateTransform(0, -text.ActualWidth * 1.25f));

                    text.RenderTransform = group;
                }

                Canvas.SetLeft(text, middle.X - text.ActualWidth / 2f);
                Canvas.SetTop(text, middle.Y + text.ActualHeight / 2f);
            }
        }

        private void CanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PreviewCanvas.ReleaseMouseCapture();
            _isMouseCaptured = false;
        }

        private void CanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PreviewCanvas.CaptureMouse();
            _isMouseCaptured = true;

            var p = e.GetPosition(PreviewCanvas);
            _previousMouse = new Vector2((float)p.X, (float)p.Y);
        }

        private void CanvasMove(object sender, MouseEventArgs e)
        {
            if (_isMouseCaptured)
            {
                var p = e.GetPosition(PreviewCanvas);
                var m = new Vector2((float)p.X, (float)p.Y);
                var delta = (m - _previousMouse);
                _previousMouse = m - delta * new Vector2(1, 1);

                // Create a TransformGroup to contain the transforms 
                // and add the transforms to it. 
                var group = new TransformGroup();
                group.Children.Add(PreviewCanvas.RenderTransform);
                group.Children.Add(new TranslateTransform { X = delta.X, Y = -delta.Y });

                // Associate the transforms to the object 
                PreviewCanvas.RenderTransform = group; 
            }
        }

        private void CenterCanvas(object sender, RoutedEventArgs routedEventArgs)
        {
            PreviewCanvas.RenderTransform = new ScaleTransform(1, -1);
        }
    }

    public class FloorPlanShape
    {
        public IReadOnlyList<Vector2> Footprint { get; private set; }

        public IReadOnlyList<IReadOnlyList<Subsection>> Subsections { get; private set; }

        public IReadOnlyList<IReadOnlyList<Vector2>> Verticals { get; private set; }

        public FloorPlanShape(IReadOnlyList<Vector2> footprint, IReadOnlyList<IReadOnlyList<Subsection>> subsections, IReadOnlyList<IReadOnlyList<Vector2>> verticals)
        {
            Footprint = footprint;
            Subsections = subsections;
            Verticals = verticals;
        }
    }

    public static class FloorPlanShapes
    {
        private static readonly FloorPlanShape _diagonalBendShape = new FloorPlanShape(
            new[] {
                new Vector2(10, 10), new Vector2(20, 0), new Vector2(23, 0), new Vector2(33, 10), new Vector2(43, 0),
                new Vector2(28, -15), new Vector2(15, -15), new Vector2(0, 0)
            },
            new[] {
                new Subsection[0],
                new Subsection[0],
                new Subsection[0],
                new Subsection[0],
                new Subsection[0],
                new Subsection[0],
                new Subsection[0],
                new Subsection[0]
            },
            new Vector2[][] {
            }
        );

        private static readonly FloorPlanShape _realFloorPlan = new FloorPlanShape(
            new[] {
                new Vector2(-25, 17),
                new Vector2(0, 17),
                new Vector2(3, 15),
                new Vector2(33, 15),
                new Vector2(38, 0),
                new Vector2(-25, -25)
            },
            new[] {
                new Subsection[0],
                new Subsection[0],
                new Subsection[0],
                new Subsection[0],
                new Subsection[0],
                new Subsection[0]
            },
            new[] {
                new[] {
                    new Vector2(0, 0),
                    new Vector2(7, 0),
                    new Vector2(7, -7),
                    new Vector2(0, -7),
                }
            }
        );

        public static readonly IReadOnlyDictionary<string, FloorPlanShape> Shapes = new Dictionary<string, FloorPlanShape> {
            { "Diagonal Bend", _diagonalBendShape },
            { "Real Office Plan", _realFloorPlan }
        };
    }
}
