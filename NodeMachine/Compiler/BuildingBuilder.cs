using System;
using System.Collections.Generic;
using System.IO;
using Base_CityGeneration.Elements.Building.Design;
using NodeMachine.Model;

namespace NodeMachine.Compiler
{
    public class BuildingBuilder
        : BaseTemplatedScriptBuilder<Building, BuildingDesigner>
    {
        public BuildingBuilder(Building building, string templateNamespace)
            : base(building, "NodeMachine.Compiler.BuildingTemplate.cs", templateNamespace)
        {
        }

        protected override BuildingDesigner Deserialize(Building input)
        {
            return BuildingDesigner.Deserialize(new StringReader(input.Markup));
        }

        protected override IEnumerable<KeyValuePair<string, string>> Tags()
        {
            return Deserialized.Tags;
        }

        protected override string Markup()
        {
            return Input.Markup;
        }

        public override string Name()
        {
            return Input.Name;
        }

        protected override Guid Id()
        {
            return Deserialized.Id;
        }

        protected override string Description()
        {
            return Deserialized.Description;
        }
    }
}
