﻿using Base_CityGeneration.Elements.Building.Internals.Floors.Selection;
using Construct_Gamemode.Map;
using Construct_Gamemode.Map.Building;
using Construct_Gamemode.Map.Models;
using EpimetheusPlugins.Testing.MockScripts;
using Myre.Extensions;
using Newtonsoft.Json;
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

        protected override void CheckMarkup(object sender, RoutedEventArgs e)
        {
            RenderPreview();
        }

        private FloorSelector Deserialize(string markup)
        {
            return FloorSelector.Deserialize(new StringReader(markup));
        }

        private int _seed = 0;  //todo: expose changing seed in UI
        private FloorSelector.Selection Layout(FloorSelector selector)
        {
            Random r = new Random(_seed);
            return selector.Select(r.NextDouble, a => ScriptReferenceFactory.Create(null, Guid.Empty, string.Join(",", a)));
        }

        private readonly ObservableCollection<PreviewRow> _preview = new ObservableCollection<PreviewRow>();

        public IEnumerable<PreviewRow> PreviewRows
        {
            get
            {
                return _preview;
            }
        }

        private void RenderPreview()
        {
            CompilationOutput.Text = "";

            try
            {
                var selector = Deserialize(new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd).Text);
                var selection = Layout(selector);

                var totalHeight = selection.AboveGroundFloors.Select(a => a.Height).Sum();
                var height = totalHeight;

                CreateColumns(selection.Verticals.Select(a => a.Script.Name));
                _preview.Clear();

                foreach (var floor in selection.AboveGroundFloors.Append(selection.BelowGroundFloors))
                {
                    height -= floor.Height;
                    if (Math.Abs(height) < 0.0001)
                        height = 0;

                    bool[] v = selection.Verticals.Select(a => a.Bottom <= floor.Index && a.Top >= floor.Index).ToArray();
                    _preview.Add(new PreviewRow { Height = floor.Height, CumulativeHeight = height, Tags = floor.Script.Name, Verticals = v });
                }
            }
            catch (Exception err)
            {
                CompilationOutput.Text = err.ToString();
            }
        }

        private void CreateColumns(IEnumerable<string> verticals)
        {
            PreviewGrid.Columns.Clear();

            PreviewGrid.Columns.Add(new DataGridTextColumn { Binding = new Binding("Height"), Header = "Height" });
            PreviewGrid.Columns.Add(new DataGridTextColumn { Binding = new Binding("CumulativeHeight"), Header = "Cumulative Height" });
            PreviewGrid.Columns.Add(new DataGridTextColumn { Binding = new Binding("Tags"), Header = "Tags" });

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
        }

        protected override async void SendToGame(object sender, RoutedEventArgs e)
        {
            if (!await Connection.IsConnected())
                return;

            //Layout building
            var solution = Layout(Deserialize(new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd).Text));

            //Create the facade in game
            await Connection.Topology.SetRoot(Guid.Parse("CFF595C4-4C67-4CCB-9E5F-AB9AE0F9AF54"), new RemoteRootInit
            {
                Children = new ChildDefinition[] {
                    new ChildDefinition {
                        Prism = new PrismModel(new[] {new Point2(-10, 10), new Point2(10, 10f), new Point2(10, -10f), new Point2(-10, -10f)}, 1000),
                        ChildType = "Construct_Gamemode.Map.Building.RemoteBuildingContainer",
                        Center = new Point3(1, 1, 1),
                        ChildData = JsonConvert.SerializeObject(new RemoteBuildingInit {
                            AboveGroundFloors = solution.AboveGroundFloors.Select(a => new RemoteFloor { Height = a.Height, Tags = a.Script.Name.Split(',') }).ToArray(),
                            BelowGroundFloors = solution.BelowGroundFloors.Select(a => new RemoteFloor { Height = a.Height, Tags = a.Script.Name.Split(',') }).ToArray()
                        })
                    }
                }
            }, _seed);
        }

        public class PreviewRow
        {
            public float Height { get; set; }

            public float CumulativeHeight { get; set; }

            public string Tags { get; set; }

            public bool[] Verticals { get; set; }
        }
    }
}
