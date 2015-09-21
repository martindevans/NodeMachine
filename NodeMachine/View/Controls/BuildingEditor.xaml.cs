using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Base_CityGeneration.Elements.Building.Design;
using Construct_Gamemode.Map;
using Construct_Gamemode.Map.Building;
using Construct_Gamemode.Map.Models;
using EpimetheusPlugins.Testing.MockScripts;
using Myre.Collections;
using Newtonsoft.Json;
using NodeMachine.Annotations;
using NodeMachine.Connection;
using NodeMachine.Model;
using NodeMachine.Model.Project;
using NodeMachine.ViewModel.Tabs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace NodeMachine.View.Controls
{
    /// <summary>
    /// Interaction logic for BuildingEditor.xaml
    /// </summary>
    public partial class BuildingEditor : BaseYamlEditorControl<Building>, ITabName
    {
        public BuildingEditor(IProjectManager manager, IGameConnection connection, Building building)
            : base(manager, connection, building)
        {
            InitializeComponent();
        }

        protected override ObservableCollection<Building> ProjectDataModelCollection
        {
            get
            {
                return ProjectManager.CurrentProject.ProjectData.Buildings;
            }
        }

        protected override string ValueName
        {
            get
            {
                return Value.Name;
            }
        }

        protected override string ValueMarkup
        {
            get
            {
                return Value.Markup;
            }
            set
            {
                Value.Markup = value;
            }
        }

        protected override async void CheckMarkup(object sender, RoutedEventArgs e)
        {
            await RenderPreview();
        }

        private BuildingDesigner Deserialize(string markup)
        {
            return BuildingDesigner.Deserialize(new StringReader(markup));
        }

        private Vector2[] Footprint()
        {
            return new Vector2[] {
                new Vector2(-30, -30),
                new Vector2(-30, 30f),
                new Vector2(30, 30f),
                new Vector2(30, -30f)
            };
        }

        private Design Layout(BuildingDesigner designer, int seed)
        {
            Random r = new Random(seed);
            var m = new NamedBoxCollection();
            Func<string[], EpimetheusPlugins.Scripts.ScriptReference> s = a => ScriptReferenceFactory.Create(null, Guid.Empty, string.Join(",", a));

            var lot = Footprint();

            return designer
                .Internals(r.NextDouble, m, s)
                .Externals(r.NextDouble, m, s, new BuildingSideInfo[] {
                    new BuildingSideInfo(lot[0], lot[1], new BuildingSideInfo.NeighbourInfo[0]),
                    new BuildingSideInfo(lot[1], lot[2], new BuildingSideInfo.NeighbourInfo[0]),
                    new BuildingSideInfo(lot[2], lot[3], new BuildingSideInfo.NeighbourInfo[0]),
                    new BuildingSideInfo(lot[3], lot[0], new BuildingSideInfo.NeighbourInfo[0]),
                });
        }

        private readonly ObservableCollection<PreviewRow> _preview = new ObservableCollection<PreviewRow>();

        public IEnumerable<PreviewRow> PreviewRows
        {
            get
            {
                return _preview;
            }
        }

        private async Task RenderPreview()
        {
            if (!Seed.Value.HasValue)
                return;

            CompilationOutput.Text = "";

            try
            {
                var selector = Deserialize(new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd).Text);
                var design = Layout(selector, Seed.Value.Value);

                var totalHeight = design.Floors.Where(a => a.Index >= 0).Select(a => a.Height).Sum();
                var height = totalHeight;

                _preview.Clear();
                CreateColumns(design.Verticals.Select(a => a.Script.Name));

                foreach (var floor in design.Floors.OrderByDescending(a => a.Index))
                {
                    height -= floor.Height;
                    if (Math.Abs(height) < 0.0001)
                        height = 0;

                    FloorSelection floor1 = floor;
                    var f = design.Walls.GroupBy(a => a.BottomIndex).Select(a => a.First())
                                  .SelectMany(a => a.Facades)
                                  .Single(a => a.Bottom <= floor1.Index && a.Top >= floor1.Index);

                    bool[] v = design.Verticals.Select(a => a.Bottom <= floor.Index && a.Top >= floor.Index).ToArray();
                    _preview.Add(new PreviewRow(floor.Height, height, floor.Script.Name, v, f == null ? "" : string.Format("{0} {1}-{2}", string.IsNullOrWhiteSpace(f.Script.Name) ? "Null" : f.Script.Name, f.Bottom, f.Top)));
                }
            }
            catch (Exception err)
            {
                _preview.Clear();

                CompilationOutput.Text = err.ToString();
            }
        }

        private void CreateColumns(IEnumerable<string> verticals)
        {
            using (Dispatcher.DisableProcessing())
            {
                Dispatcher.Invoke(() => {
                    PreviewGrid.Columns.Clear();

                    PreviewGrid.Columns.Add(new DataGridTextColumn { Binding = new Binding("Height"), Header = "Height" });
                    PreviewGrid.Columns.Add(new DataGridTextColumn { Binding = new Binding("CumulativeHeight"), Header = "Cumulative Height" });
                    PreviewGrid.Columns.Add(new DataGridTextColumn { Binding = new Binding("Tags"), Header = "Tags" });
                    PreviewGrid.Columns.Add(new DataGridTextColumn { Binding = new Binding("Facade"), Header = "Facade" });

                    int i = 0;
                    foreach (var name in verticals)
                    {
                        PreviewGrid.Columns.Add(new DataGridCheckBoxColumn {
                            Binding = new Binding(string.Format("Verticals[{0}]", i)),
                            CanUserSort = false,
                            Header = name
                        });

                        i++;
                    }
                });
            }
        }

        protected override async void SendToGame(object sender, RoutedEventArgs e)
        {
            if (!Seed.Value.HasValue)
                return;
            if (!await Connection.IsConnected())
                return;

            var script = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd).Text;
            var lot = Footprint();

            //Create the building in game
            await Connection.Topology.SetRoot(Guid.Parse("CFF595C4-4C67-4CCB-9E5F-AB9AE0F9AF54"), new RemoteRootInit
            {
                Children = new ChildDefinition[] {
                    new ChildDefinition {
                        Prism = new PrismModel(new[] { (Point2)lot[0], (Point2)lot[1], (Point2)lot[2], (Point2)lot[3] }, 1000),
                        ChildType = "Construct_Gamemode.Map.Building.RemoteBuildingContainer",
                        Center = new Point3(1, 1, 1),
                        ChildData = JsonConvert.SerializeObject(new RemoteBuildingInit {
                            Seed = Seed.Value.Value,
                            Script = script
                        })
                    }
                }
            }, Seed.Value.Value);
        }

        public class PreviewRow
        {
            public float Height { [UsedImplicitly]get; private set; }

            public float CumulativeHeight { [UsedImplicitly]get; private set; }

            public string Tags { [UsedImplicitly]get; private set; }

            public bool[] Verticals { [UsedImplicitly]get; private set; }

            public string Facade { [UsedImplicitly]get; private set; }

            public PreviewRow(float height, float cumulativeHeight, string tags, bool[] verticals, string facade)
            {
                Height = height;
                CumulativeHeight = cumulativeHeight;
                Tags = tags;
                Verticals = verticals;
                Facade = facade;
            }
        }

        private async void SeedChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!IsInitialized)
                return;

            await RenderPreview();
        }
    }
}
