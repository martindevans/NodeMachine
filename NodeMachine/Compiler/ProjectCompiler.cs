using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Base_CityGeneration.Elements.Building.Design;
using EpimetheusPlugins.Scripts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NodeMachine.Model.Project;

namespace NodeMachine.Compiler
{
    public class ProjectCompiler
    {
        private readonly IProject _project;

        public ProjectCompiler(IProject project)
        {
            _project = project;
        }

        public async Task<bool> Compile(List<string> outputMessages)
        {
            var templateNamespace = ToCSharpName(_project.ProjectData.Name);

            var tags = new HashSet<string>();
            var trees = new List<SyntaxTree>();

            return await Task.Run(() => {

                //If the output directory does not exist, early exit
                var dir = _project.ProjectData.CompileOutputDirectory;
                if (dir == null || !Directory.Exists(dir))
                {
                    outputMessages.Add(string.Format("{0}: {1}", "DirectoryNotFound", _project.ProjectData.CompileOutputDirectory));
                    return false;
                }

                foreach (var building in _project.ProjectData.Buildings)
                    trees.Add(new BuildingBuilder(building, templateNamespace).Build(tags));
                //foreach (var block in _project.ProjectData.Blocks)
                //    trees.Add(new BlockBuilder(block, templateNamespace).Build(tags));
                //foreach (var city in _project.ProjectData.Cities)
                //    trees.Add(new CityBuilder(city, templateNamespace).Build(tags));
                //foreach (var facade in _project.ProjectData.Facades)
                //    trees.Add(new FacadeBuilder(facade, templateNamespace).Build(tags));
                //foreach (var floor in _project.ProjectData.Floors)
                //    trees.Add(new FloorBuilder(floor, templateNamespace).Build(tags));
                //foreach (var room in _project.ProjectData.Rooms)
                //    trees.Add(new RoomBuilder(room, templateNamespace).Build(tags));

                //Convert all the tags we've discovered into syntax trees of empty interfaces
                trees.AddRange(tags.Select(t => TagToSyntaxTree(t, templateNamespace)));

                //Build ourselves a suitable compiler
                var compilation = CSharpCompilation.Create(
                    _project.ProjectData.Name,
                    syntaxTrees: trees,
                    references: MetadataReferences(),
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                );

                //Write compile result into memory
                using (var ms = new MemoryStream())
                {
                    var result = compilation.Emit(ms);

                    //If we fail, write out errors
                    if (!result.Success)
                    {
                        foreach (var diagnostic in result.Diagnostics.Where(a => a.IsWarningAsError || a.Severity == DiagnosticSeverity.Error))
                            outputMessages.Add(string.Format("{0}: {1}", diagnostic.Id, diagnostic.GetMessage()));
                        return false;
                    }

                    //If we succeeded, write out results to dll
                    using (var file = File.Create(Path.Combine(dir, string.Format("{0}.dll", templateNamespace))))
                        ms.CopyTo(file);

                    //Also write out the _Config.ini
                    using (var file = File.Create(Path.Combine(dir, "_Config.ini")))
                    using (var writer = new StreamWriter(file))
                    {
                        writer.WriteLine("id={0}", _project.ProjectData.Guid);
                        writer.WriteLine("load={0}.dll", templateNamespace);

                        foreach (var metadataValue in _project.ProjectData.Metadata)
                            writer.WriteLine("{0}={1}", metadataValue.Key, metadataValue.Value);
                    }

                    return true;
                }
            });
        }

        internal static string ToCSharpName(string str)
        {
            return str
                .Replace(" ", "_");
        }

        private static IEnumerable<MetadataReference> MetadataReferences()
        {
            var references = new MetadataReference[] {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(BuildingDesigner).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(EpimetheusPlugins.Names).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(SharpYaml.IParser).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Cassowary.CassowaryException).Assembly.Location),
            };
            return references;
        }

        private static SyntaxTree TagToSyntaxTree(string iname, string templateNamespace)
        {
            var template =
@"namespace /*TEMPLATED_NAMESPACE*/
{
    public interface /*TEMPLATED_INTERFACE_NAME*/
    {
    }
}";

            template = template
                .Replace("/*TEMPLATED_NAMESPACE*/", templateNamespace)
                .Replace("/*TEMPLATED_INTERFACE_NAME*/", iname);

            return CSharpSyntaxTree.ParseText(template);
        }

        internal static string BuildTagsList(IEnumerable<KeyValuePair<string, string>> tagsValues, ISet<string> outputTagNames)
        {
            if (tagsValues == null || !tagsValues.Any())
                return "";

            var interfaces = new List<string>();

            foreach (var interfaceName in tagsValues.Distinct().SelectMany(InterfaceNamesForTag))
            {
                outputTagNames.Add(interfaceName);
                interfaces.Add(interfaceName);
            }

            return ", " + string.Join(",", interfaces);
        }

        internal static string EscapeCSharpStringLiteral(string str)
        {
            //Convert into a verbatim C# string
            return str.Replace("\"", "\"\"");
        }

        private static IEnumerable<string> InterfaceNamesForTag(KeyValuePair<string, string> tag)
        {
            var k = TaggingExtensions.CanonicalizeTagString(tag.Key);
            var v = TaggingExtensions.CanonicalizeTagString(tag.Value);

            //Allows us to search for Key = Value
            yield return string.Format("ITag_Key_{0}_Value_{1}", k, v);

            //Allows us to search for HasKey(Key)
            yield return string.Format("ITag_Key_{0}", k);

            //Allows us to search for HasValue(Value)
            yield return string.Format("ITag_Value_{0}", v);
        }
    }
}
