using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Base_CityGeneration.Elements.Building.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NodeMachine.Model;
using Xceed.Wpf.Toolkit.Primitives;

namespace NodeMachine.Compiler
{
    public class BuildingBuilder
        : BaseTemplatedScriptBuilder<Building, BuildingDesigner>
    {
        private readonly Building _building;
        private readonly string _templateNamespace;

        public BuildingBuilder(Building building, string templateNamespace)
            : base(building, "NodeMachine.Compiler.BuildingTemplate.cs", templateNamespace)
        {
            _building = building;
            _templateNamespace = templateNamespace;
        }

        //public SyntaxTree Build(List<string> messages, ISet<string> tags)
        //{
        //    //Construct a building container which has the spec hardcoded
        //    //Container parses spec and puts parsed spec into metadata
        //    var parsed = BuildingDesigner.Deserialize(new StringReader(_building.Markup));

        //    // ReSharper disable once AssignNullToNotNullAttribute
        //    // ^ If this is null it's a programmer error!
        //    // ReSharper disable ExceptionNotDocumented
        //    // ^ GetManifestResourceStream seems to throw every kind of exception under the sun :|
        //    var template = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("NodeMachine.Compiler.BuildingTemplate.cs")).ReadToEnd();
        //    // ReSharper restore ExceptionNotDocumented

        //    //Put markup into the right place to be parsed
        //    template = template.Replace("/*TEMPLATED_BUILDING_SCRIPT*/", ProjectCompiler.EscapeCSharpStringLiteral(_building.Markup));

        //    //Add tags
        //    template = template.Replace("/*TEMPLATED_BUILDING_TAGS*/", ProjectCompiler.BuildTagsList(parsed.Tags, tags));

        //    //Put into correct namespace
        //    template = template.Replace("/*TEMPLATED_NAMESPACE*/", _templateNamespace);

        //    //Create a new class name
        //    template = template.Replace("/*TEMPLATED_BUILDING_NAME*/", ProjectCompiler.ToCSharpName(_building.Name));

        //    //Setup script attribute
        //    template = template.Replace("/*TEMPLATED_BUILDING_GUID*/", parsed.Id.ToString());
        //    template = template.Replace("/*TEMPLATED_BUILDING_DESCRIPTION*/", parsed.Description);

        //    //Sanity check for programmer error (i.e. did we forget to replace any templated values)
        //    if (template.Contains("TEMPLATED"))
        //        throw new NotImplementedException("Programmer Error!");

        //    return CSharpSyntaxTree.ParseText(template);
        //}
        protected override BuildingDesigner Deserialize(Building input)
        {
            return BuildingDesigner.Deserialize(new StringReader(_building.Markup));
        }

        protected override IEnumerable<KeyValuePair<string, string>> Tags()
        {
            return Deserialized.Tags;
        }

        protected override string Markup()
        {
            return Input.Markup;
        }

        protected override string Name()
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
