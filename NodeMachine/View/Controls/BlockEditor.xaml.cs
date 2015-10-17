using System.Numerics;
using Base_CityGeneration.Elements.Blocks.Spec;
using Base_CityGeneration.Parcels.Parcelling;
using Construct_Gamemode.Map;
using Construct_Gamemode.Map.Block;
using Construct_Gamemode.Map.Models;
using Myre.Collections;
using Newtonsoft.Json;
using NodeMachine.Connection;
using NodeMachine.Model.Project;
using NodeMachine.ViewModel.Tabs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using Block = NodeMachine.Model.Block;
using Point = System.Windows.Point;

namespace NodeMachine.View.Controls
{
    /// <summary>
    /// Interaction logic for BlockEditor.xaml
    /// </summary>
    public partial class BlockEditor : BaseYamlEditorControl<Block>, ITabName
    {
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

        public BlockEditor(IProjectManager manager, IGameConnection connection, Block block)
            : base(manager, connection, block)
        {
            InitializeComponent();
        }

        protected override ObservableCollection<Block> ProjectDataModelCollection
        {
            get
            {
                return ProjectManager.CurrentProject.ProjectData.Blocks;
            }
        }

        protected override void CheckMarkup(object sender, RoutedEventArgs e)
        {
            RenderPreview();
        }

        private void RenderPreview()
        {
            if (Seed == null || !Seed.Value.HasValue)
                return;
            if (CompilationOutput == null)
                return;
            if (PreviewCanvas == null)
                return;

            CompilationOutput.Text = "";
            PreviewCanvas.Children.Clear();

            try
            {
                var spec = Deserialize(new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd).Text);

                Random r = new Random(Seed.Value.Value);

                var parcels = spec.CreateParcels(RootShape(), r.NextDouble, new NamedBoxCollection());

                RenderParcels(parcels);
            }
            catch (Exception err)
            {
                CompilationOutput.Text = err.ToString();
            }
        }

        private Parcel RootShape()
        {
            var shape = new[] {
                new Vector2(-100, 50),
                new Vector2(100, 50),
                new Vector2(100, -50),
                new Vector2(-100, -50)
            };

            return new Parcel(new Parcel.Edge[] {
                new Parcel.Edge { Start = shape[0], End = shape[1], Resources = new [] { "road" } },
                new Parcel.Edge { Start = shape[1], End = shape[2], Resources = new [] { "road" } },
                new Parcel.Edge { Start = shape[2], End = shape[3], Resources = new [] { "road" } },
                new Parcel.Edge { Start = shape[3], End = shape[0], Resources = new [] { "road" } },
            });
        }

        private void RenderParcels(IEnumerable<Parcel> parcels)
        {
            foreach (var parcel in parcels)
            {
                var p = new Polygon {
                    Stroke = Brushes.DarkBlue,
                    StrokeThickness = 2
                };

                foreach (var point in parcel.Points())
                    p.Points.Add(new Point(point.X, point.Y));

                PreviewCanvas.Children.Add(p);
            }
        }

        private static BlockSpec Deserialize(string markup)
        {
            return BlockSpec.Deserialize(new StringReader(markup));
        }

        protected override async void SendToGame(object sender, RoutedEventArgs e)
        {
            if (!await Connection.IsConnected())
                return;

            //Preconditions for subdivision
            if (Seed == null || !Seed.Value.HasValue)
                return;

            var shape = RootShape().Points();
            var bounds = Base_CityGeneration.Datastructures.Rectangle.FromPoints(shape);

            //Send network config to game
            await Connection.Topology.SetRoot(Guid.Parse("CFF595C4-4C67-4CCB-9E5F-AB9AE0F9AF54"), new RemoteRootInit
            {
                Children = new ChildDefinition[] {
                    new ChildDefinition {
                        Prism = new PrismModel(new[] { new Point2(bounds.Left, bounds.Bottom), new Point2(bounds.Left, bounds.Top), new Point2(bounds.Right, bounds.Top), new Point2(bounds.Right, bounds.Bottom) }, 1000f),
                        ChildType = typeof(RemoteBlockContainer).AssemblyQualifiedName,
                        Center = new Point3(0, 0, 0),
                        ChildData = JsonConvert.SerializeObject(new RemoteBlockInit {
                            Script = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd).Text,
                            Seed = Seed.Value.Value,
                            Footprint = shape
                        })
                    }
                }
            }, Seed.Value.Value);
        }

        private void SeedChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            RenderPreview();
        }

        private void OpenHelpUrl(object sender, RoutedEventArgs e)
        {
            Process.Start("https://bitbucket.org/martindevans/base-citygeneration/src/default/Base-CityGeneration/Elements/Blocks/Spec/BlockSpec.md");
        }
    }
}
