using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using Base_CityGeneration.Elements.Blocks.Spec;
using Base_CityGeneration.Parcels.Parcelling;
using Microsoft.Xna.Framework;
using NodeMachine.Connection;
using NodeMachine.Model.Project;
using NodeMachine.ViewModel.Tabs;
using SharpYaml;
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

                var parcels = spec.CreateParcels(RootShape(), r.NextDouble);

                RenderParcels(parcels);
            }
            catch (Exception err)
            {
                CompilationOutput.Text = err.ToString();
            }
        }

        private Parcel RootShape()
        {
            return new Parcel(new Parcel.Edge[] {
                new Parcel.Edge { Start = new Vector2(-100, 50), End = new Vector2(100, 50), Resources = new [] { "road" } },
                new Parcel.Edge { Start = new Vector2(100, 50), End = new Vector2(100, -50), Resources = new [] { "road" } },
                new Parcel.Edge { Start = new Vector2(100, -50), End = new Vector2(-100, -50), Resources = new [] { "road" } },
                new Parcel.Edge { Start = new Vector2(-100, -50), End = new Vector2(-100, 50), Resources = new [] { "road" } },
            });
        }

        private void RenderParcels(IEnumerable<Parcel> parcels)
        {
            foreach (var parcel in parcels)
            {
                var p = new Polygon()
                {
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

        protected override void SendToGame(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
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
