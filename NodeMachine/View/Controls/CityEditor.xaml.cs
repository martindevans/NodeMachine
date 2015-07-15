using Base_CityGeneration.Elements.Roads.Hyperstreamline;
using Base_CityGeneration.Elements.Roads.Hyperstreamline.Tracing;
using Microsoft.Xna.Framework;
using NodeMachine.Connection;
using NodeMachine.Model;
using NodeMachine.Model.Project;
using NodeMachine.ViewModel.Tabs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace NodeMachine.View.Controls
{
    /// <summary>
    /// Interaction logic for CityEditor.xaml
    /// </summary>
    public partial class CityEditor : BaseYamlEditorControl<City>, ITabName
    {
        public CityEditor(IProjectManager project, IGameConnection connection, City city)
            : base(project, connection, city)
        {
            InitializeComponent();
        }

        protected override ObservableCollection<City> ProjectDataModelCollection
        {
            get
            {
                return ProjectManager.CurrentProject.ProjectData.Cities;
            }
        }

        protected override string ValueName
        {
            get
            {
                return Value.Name;
            }
        }

        protected override string ValueMarkup {
            get
            {
                return Value.Markup;
            }
            set
            {
                Value.Markup = value;
            }
        }

        private async void CheckMarkup(object sender, RoutedEventArgs e)
        {
            await RenderPreview();
        }

        #region rendering

        private NetworkDescriptor _config;
        private NetworkBuilder _networkBuilder;
        private readonly HashSet<Region> _regionsToRender = new HashSet<Region>();
        private float _renderOffset;

        private async Task RenderPreview()
        {
            if (Seed == null || PreviewSizeValue == null || AutoGenerateMinorRoads == null)
                return;
            if (!Seed.Value.HasValue || !PreviewSizeValue.Value.HasValue || !AutoGenerateMinorRoads.IsChecked.HasValue)
                return;

            CompilationOutput.Text = "";
            _regionsToRender.Clear();
            _networkBuilder = null;
            _config = null;

            try
            {
                PreviewCanvas.Children.Clear();

                StringBuilder output = new StringBuilder();

                _config = Deserialize(new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd).Text);
                if (_config == null)
                    output.AppendLine("Deserialization failed");
                else
                {
                    //Generate major roads
                    _networkBuilder = new NetworkBuilder();
                    Random rand = new Random(Seed.Value.Value);
                    _networkBuilder.Build(_config.Major(rand.NextDouble), rand, new Vector2(0, 0), new Vector2(PreviewSizeValue.Value.Value, PreviewSizeValue.Value.Value));
                    _networkBuilder.Reduce();

                    //Extract results
                    var network = _networkBuilder.Result;
                    _regionsToRender.UnionWith(_networkBuilder.Regions());

                    //Render results
                    _renderOffset = -PreviewSizeValue.Value.Value / 2f;
                    RenderNetwork(PreviewCanvas, network, _regionsToRender, _renderOffset);

                    //Generate minor roads
                    if (AutoGenerateMinorRoads.IsChecked.Value)
                    {
                        foreach (var region in _regionsToRender.ToArray())
                        {
                            Region region1 = region;
                            await Task.Factory.StartNew(() => {
                                _networkBuilder.Build(_config.Minor(rand.NextDouble), rand, region1);

                                network = _networkBuilder.Result;
                            });

                            _regionsToRender.Remove(region1);
                            RenderNetwork(PreviewCanvas, network, _regionsToRender, _renderOffset);
                        }

                        _networkBuilder.Reduce();
                        network = _networkBuilder.Result;
                        RenderNetwork(PreviewCanvas, network, _regionsToRender, _renderOffset);
                    }
                }

                CompilationOutput.Text = output.ToString();
            }
            catch (Exception err)
            {
                CompilationOutput.Text = err.ToString();
            }
        }

        private static void RenderNetwork(Canvas canvas, Network network, IEnumerable<Region> regions, float offset)
        {
            canvas.Children.Clear();

            //Draw regions
            foreach (var region in regions)
            {
                int hash;
                unchecked {
                    hash = Math.Abs((17 * 23 + region.Min.GetHashCode()) * 23 + region.Max.GetHashCode());
                }
                RenderPath(canvas, region.Vertices, hash, offset, offset);
            }

            //Draw roads
            var streamlines = network.Vertices.SelectMany(a => a.Edges).Select(a => a.Streamline).Distinct();
            foreach (var streamline in streamlines)
            {
                Polyline p = new Polyline {
                    Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    StrokeThickness = streamline.Width,
                    StrokeEndLineCap = PenLineCap.Flat,
                    StrokeStartLineCap = PenLineCap.Flat
                };

                var v = streamline.First;
                do
                {
                    p.Points.Add(new Point(v.Position.X + offset, v.Position.Y + offset));

                    var e = v.Edges.SingleOrDefault(a => a.A.Equals(v) && a.Streamline == streamline);
                    v = e == null ? null : e.B;

                } while (v != null && !Equals(v, streamline.First));

                canvas.Children.Add(p);
            }
        }

        private static NetworkDescriptor Deserialize(string text)
        {
            return NetworkDescriptor.Deserialize(new StringReader(text));
        }

        private static void RenderPath(Canvas canvas, IEnumerable<Vector2> path, int hash, float offsetX, float offsetY)
        {
            //Create polygon
            Polygon polygon = new Polygon {
                Fill = new SolidColorBrush(new HSLColor((0.618033988749895f * hash) % 1, 1, 0.5f))
            };

            //Add points
            foreach (var point in path)
                polygon.Points.Add(new Point(point.X + offsetX, point.Y + offsetY));

            //Add to canvas
            canvas.Children.Add(polygon);
        }
        #endregion

        private void SendToGame(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OpenHelpUrl(object sender, RoutedEventArgs e)
        {
            Process.Start("https://bitbucket.org/martindevans/base-citygeneration/src/default/Base-CityGeneration/Elements/Roads/Hyperstreamline/Hyperstreamline.md");
        }

        private async void SeedChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!IsInitialized)
                return;

            await RenderPreview();
        }

        private async void PreviewSizeChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!IsInitialized)
                return;

            await RenderPreview();
        }

        private async void OnCanvasClicked(object sender, MouseButtonEventArgs e)
        {
            if (_regionsToRender == null || _regionsToRender.Count == 0)
                return;

            var canvas = (Canvas)sender;

            var p = Mouse.GetPosition(canvas);
            p.X -= _renderOffset;
            p.Y -= _renderOffset;

            var r = _regionsToRender.SingleOrDefault(a => a.PointInPolygon(new Vector2((float)p.X, (float)p.Y)));
            if (r == null)
                return;

            _regionsToRender.Remove(r);

            Random rand = new Random();
            _networkBuilder.Build(_config.Minor(rand.NextDouble), rand, r);
            var network = _networkBuilder.Result;

            RenderNetwork(canvas, network, _regionsToRender, _renderOffset);
        }
    }
}
