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
using Base_CityGeneration.Utilities;
using EpimetheusPlugins.Testing.MockScripts;
using Myre.Collections;
using NodeMachine.Connection;
using NodeMachine.Model;
using NodeMachine.Model.Project;
using NodeMachine.ViewModel.Tabs;
using SwizzleMyVectors.Geometry;
using Section = Base_CityGeneration.Elements.Building.Internals.Floors.Design.Section;

namespace NodeMachine.View.Controls
{
    /// <summary>
    /// Interaction logic for FloorEditor.xaml
    /// </summary>
    public partial class FloorEditor : BaseYamlEditorControl<Floor>, ITabName
    {
        public IReadOnlyDictionary<string, IReadOnlyList<BasePolygonRegion<FloorplanRegion, Section>.Side>> Shapes { get { return FloorPlanShapes.Shapes; } }

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

        private static Task<FloorPlan> GenerateLayout(FloorDesigner spec, Func<double> random, NamedBoxCollection metadata, IReadOnlyList<BasePolygonRegion<FloorplanRegion, Section>.Side> footprint)
        {
            return Task<FloorPlan>.Factory.StartNew(() => {

                Func<IEnumerable<KeyValuePair<string, string>>, Type[], EpimetheusPlugins.Scripts.ScriptReference> s = (a, b) => ScriptReferenceFactory.Create(typeof(NullScript), Guid.Empty, string.Join(",", a));

                return spec.Design(random, metadata, s, footprint, 0.075f,
                    new List<IReadOnlyList<Vector2>>(),
                    new List<VerticalSelection>());
            });
        }

        private static FloorDesigner Deserialize(string text, Func<double> random, INamedDataCollection metadata)
        {
            return FloorDesigner.Deserialize(new StringReader(text), random, metadata);
        }

        protected override void SendToGame(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
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

        private void DrawLayout(FloorPlan plan, int scale)
        {
            //Draw outline
            //todo: rewrite floorplan outline rendering to show external windows and doors
            var outline = new Polygon
            {
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
                foreach (var facade in room.GetFacades())
                {
                    Brush c = Brushes.DarkBlue;
                    if (facade.IsExternal && facade.Section.IsCorner)
                        c = Brushes.Purple;
                    else if (facade.IsExternal)
                        c = Brushes.Green;
                    else if (facade.Section.IsCorner)
                        c = Brushes.CornflowerBlue;
                    else if (facade.NeighbouringRoom != null)
                        c = Brushes.Gray;

                    var facadePolygon = new Polygon {
                        Stroke = c,
                        StrokeThickness = 1
                    };
                    facadePolygon.Points.Add(new Point(facade.Section.A.X * scale, facade.Section.A.Y * scale));
                    facadePolygon.Points.Add(new Point(facade.Section.B.X * scale, facade.Section.B.Y * scale));
                    facadePolygon.Points.Add(new Point(facade.Section.C.X * scale, facade.Section.C.Y * scale));
                    facadePolygon.Points.Add(new Point(facade.Section.D.X * scale, facade.Section.D.Y * scale));
                    PreviewCanvas.Children.Add(facadePolygon);
                }

                var scr = room.Scripts.FirstOrDefault();
                if (scr != null)
                {
                    var middle = (room.InnerFootprint.Aggregate((a, b) => a + b) / room.InnerFootprint.Count) * scale;

                    var text = new TextBlock() {
                        Text = room.Name,
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

                Console.WriteLine(delta);
            }
        }

        private void CenterCanvas(object sender, RoutedEventArgs routedEventArgs)
        {
            PreviewCanvas.RenderTransform = new ScaleTransform(1, -1);
        }
    }

    public static class FloorPlanShapes
    {
        private static readonly FloorplanRegion.Side[] _rectangleWithWindow = {
            new FloorplanRegion.Side(new Vector2(9, 0), new Vector2(9, -6), new Section[] {
                new Section(0, 1, Section.Types.Window)
            }),
            new FloorplanRegion.Side(new Vector2(9, -6), new Vector2(0, -6), new Section[0]),
            new FloorplanRegion.Side(new Vector2(0, -6), new Vector2(0, 0), new Section[0]),
            new FloorplanRegion.Side(new Vector2(0, 0), new Vector2(9, 0), new Section[0]),
        };

        private static readonly FloorplanRegion.Side[] _rectangle = {
            new FloorplanRegion.Side(new Vector2(9, 0), new Vector2(9, -6), new Section[0]),
            new FloorplanRegion.Side(new Vector2(9, -6), new Vector2(0, -6), new Section[0]),
            new FloorplanRegion.Side(new Vector2(0, -6), new Vector2(0, 0), new Section[0]),
            new FloorplanRegion.Side(new Vector2(0, 0), new Vector2(9, 0), new Section[0]),
        };

        private static readonly FloorplanRegion.Side[] _2Rectangles = {
                new FloorplanRegion.Side(new Vector2(9, 5), new Vector2(9, -6), new Section[] { new Section(0, 1, Section.Types.Window) }),
                new FloorplanRegion.Side(new Vector2(9, -6), new Vector2(0, -6), new Section[0]),
                new FloorplanRegion.Side(new Vector2(0, -6), new Vector2(0, 0), new Section[0]),
                new FloorplanRegion.Side(new Vector2(0, 0), new Vector2(-4, 0), new Section[0]),
                new FloorplanRegion.Side(new Vector2(-4, 0), new Vector2(-4, 5), new Section[] { new Section(0, 1, Section.Types.Window) }),
                new FloorplanRegion.Side(new Vector2(-4, 5), new Vector2(9, 5), new Section[0]),
        };

        public static readonly IReadOnlyDictionary<string, IReadOnlyList<BasePolygonRegion<FloorplanRegion, Section>.Side>> Shapes = new Dictionary<string, IReadOnlyList<BasePolygonRegion<FloorplanRegion, Section>.Side>> {
            { "Rectangle (Window)", _rectangleWithWindow },
            { "Rectangle", _rectangle },
            { "2 Rectangles", _2Rectangles }
        };
    }
}
