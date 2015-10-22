using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
                    references: References().Select(a => MetadataReference.CreateFromFile(a)),
                    options: new CSharpCompilationOptions(
                        outputKind: OutputKind.DynamicallyLinkedLibrary,
                        optimizationLevel: OptimizationLevel.Release,
                        platform: Platform.X86
                    )
                );

                //Write compile result into memory
                using (var msDll = new MemoryStream())
                using (var msPdb = new MemoryStream())
                {
                    var result = compilation.Emit(msDll, msPdb);

                    //If we fail, write out errors
                    if (!result.Success)
                    {
                        foreach (var diagnostic in result.Diagnostics.Where(a => a.IsWarningAsError || a.Severity == DiagnosticSeverity.Error))
                            outputMessages.Add(string.Format("{0}: {1}", diagnostic.Id, diagnostic.GetMessage()));
                        return false;
                    }

                    //If we succeeded, write out results to dll
                    using (var file = File.Create(Path.Combine(dir, string.Format("{0}.dll", templateNamespace))))
                        msDll.CopyTo(file);
                    using (var file = File.Create(Path.Combine(dir, string.Format("{0}.pdb", templateNamespace))))
                        msPdb.CopyTo(file);

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

        private static IEnumerable<string> References()
        {
            //System
            yield return typeof(object).Assembly.Location;
            yield return typeof(Enumerable).Assembly.Location;

            //Base-CityGeneration
            yield return typeof(BuildingDesigner).Assembly.Location;
            yield return typeof(Cassowary.CassowaryException).Assembly.Location;
            yield return typeof(EpimetheusPlugins.Names).Assembly.Location;
            yield return typeof(HandyCollections.RecentlyUsedQueue<>).Assembly.Location;
            yield return typeof(ICSharpCode.SharpZipLib.SharpZipBaseException).Assembly.Location;
            yield return typeof(Mercurial.AddCommand).Assembly.Location;
            yield return typeof(Myre.ActionDisposable).Assembly.Location;
            yield return typeof(Myre.Debugging.CommandAttribute).Assembly.Location;
            yield return typeof(Myre.Entities.BehaviourData).Assembly.Location;
            yield return typeof(Myre.Graphics.Camera).Assembly.Location;
            yield return typeof(Newtonsoft.Json.ConstructorHandling).Assembly.Location;
            yield return typeof(Ninject.ActivationException).Assembly.Location;
            yield return typeof(NLog.GlobalDiagnosticsContext).Assembly.Location;
            yield return typeof(Placeholder.Configuration).Assembly.Location;
            yield return typeof(Placeholder.AI.Control.BehaviourTree.BaseStateHandlingNode).Assembly.Location;
            yield return typeof(Placeholder.AI.Pathfinding.BasePathEnumerable<>).Assembly.Location;
            yield return typeof(Placeholder.Audio2.Names).Assembly.Location;
            yield return typeof(Placeholder.ConstructiveSolidGeometry.Configuration).Assembly.Location;
            yield return typeof(Placeholder.Entities.Names).Assembly.Location;
            yield return typeof(Placeholder.Networking.Names).Assembly.Location;
            yield return typeof(Placeholder.Serialization.Configuration).Assembly.Location;
            yield return typeof(Poly2Tri.P2T).Assembly.Location;
            yield return typeof(SharpYaml.IParser).Assembly.Location;
            yield return typeof(SupersonicSound.FmodException).Assembly.Location;
            yield return typeof(SwizzleMyVectors.Matrix4x4Extensions).Assembly.Location;

            ////Base-CityGeneration (Transitive Closure)
            //var allCitygenReferences = new HashSet<Assembly>();
            //var processingStack = new Stack<Assembly>();
            //processingStack.Push(typeof(BuildingDesigner).Assembly);

            ////Process until stack is empty
            //while (processingStack.Count > 0)
            //{
            //    var a = processingStack.Pop();

            //    if (allCitygenReferences.Add(a))
            //    {
            //        yield return a.Location;

            //        foreach (var referencedAssembly in a.GetReferencedAssemblies())
            //        {
            //            //processingStack.Push(referencedAssembly.
            //        }
            //    }
            //}
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
