using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Base_CityGeneration.Elements.Building.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NodeMachine.Model;

namespace NodeMachine.Compiler
{
    public class BuildingBuilder
        : IBuilder
    {
        private readonly Building _building;
        private readonly CompileSettings _settings;
        private readonly string _templateNamespace;

        public BuildingBuilder(Building building, CompileSettings settings, string templateNamespace)
        {
            _building = building;
            _settings = settings;
            _templateNamespace = templateNamespace;
        }

        public SyntaxTree Build(List<string> messages, ISet<string> tags)
        {
            //Construct a building container which has the spec hardcoded
            //Container parses spec and puts parsed spec into metadata
            var parsed = BuildingDesigner.Deserialize(new StringReader(_building.Markup));

            // ReSharper disable once AssignNullToNotNullAttribute
            // ^ If this is null it's a programmer error!
            // ReSharper disable ExceptionNotDocumented
            // ^ GetManifestResourceStream seems to throw every kind of exception under the sun :|
            var template = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("NodeMachine.Compiler.BuildingTemplate.cs")).ReadToEnd();
            // ReSharper restore ExceptionNotDocumented

            //Put markup into the right place to be parsed
            template = template.Replace("/*TEMPLATED_BUILDING_SCRIPT*/", EscapeCSharpStringLiteral(_building.Markup));

            //Add tags
            template = template.Replace("/*TEMPLATED_BUILDING_TAGS*/", BuildTagsList(parsed.Tags, tags));

            //Put into correct namespace
            template = template.Replace("/*TEMPLATED_NAMESPACE*/", _templateNamespace);

            //Create a new class name
            template = template.Replace("/*TEMPLATED_BUILDING_NAME*/", ProjectCompiler.ToCSharpName(_building.Name));

            //Setup script attribute
            template = template.Replace("/*TEMPLATED_BUILDING_GUID*/", parsed.Id.ToString());
            template = template.Replace("/*TEMPLATED_BUILDING_DESCRIPTION*/", parsed.Description);

            //Sanity check for programmer error (i.e. did we forget to replace any templated values)
            if (template.Contains("TEMPLATED"))
                throw new NotImplementedException();

            return CSharpSyntaxTree.ParseText(template);
        }

        private static string BuildTagsList(IEnumerable<KeyValuePair<string, string>> tagsValues, ISet<string> outputTagNames)
        {
            if (outputTagNames == null || outputTagNames.Count == 0)
                return "";

            var interfaces = new List<string>();

            foreach (var interfaceName in tagsValues.Distinct().SelectMany(InterfaceNameForTag))
            {
                outputTagNames.Add(interfaceName);
                interfaces.Add(interfaceName);
            }

            return ", " + string.Join(",", interfaces);
        }

        private static string EscapeCSharpStringLiteral(string str)
        {
            //Convert into a verbatim C# string
            return str.Replace("\"", "\"\"");
        }

        private static IEnumerable<string> InterfaceNameForTag(KeyValuePair<string, string> tag)
        {
            //Allows us to search for Key = Value
            yield return string.Format("ITag_Key_{0}_Value_{1}", tag.Key, tag.Value);

            //Allows us to search for HasKey(Key)
            yield return string.Format("ITag_Key_{0}", tag.Key);

            //Allows us to search for HasValue(Value)
            yield return string.Format("ITag_Value_{0}", tag.Value);
        }
    }
}
