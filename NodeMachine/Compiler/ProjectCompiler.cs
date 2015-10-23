using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using EpimetheusPlugins.Scripts;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.CodeAnalysis;
using NodeMachine.Extensions;
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
            var csharpFiles = new List<KeyValuePair<string, string>>();

            return await Task.Run(() => {

                //If the output directory does not exist, early exit
                var dir = _project.ProjectData.CompileOutputDirectory;
                if (dir == null || !Directory.Exists(dir))
                {
                    outputMessages.Add(string.Format("{0}: {1}", "DirectoryNotFound", _project.ProjectData.CompileOutputDirectory));
                    return false;
                }

                //Unpack the project template into the directory
                UnpackTemplate(dir);

                foreach (var building in _project.ProjectData.Buildings)
                {
                    var b = new BuildingBuilder(building, templateNamespace);
                    csharpFiles.Add(new KeyValuePair<string, string>(b.Name() + ".cs", b.Build(tags)));
                }
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
                csharpFiles.AddRange(tags.Select(t => new KeyValuePair<string, string>(t, TagToCSharpCode(t, templateNamespace))));

                //Output the files into the project directory
                foreach (var fileSpec in csharpFiles)
                {
                    using (var f = File.Create(Path.Combine(dir, "NodeMachinePluginTemplate", fileSpec.Key)))
                    using (var w = new StreamWriter(f))
                    {
                        w.Write(fileSpec.Value);
                    }
                }

                //Fill in template with file includes
                var projPath = Path.Combine(dir, "NodeMachinePluginTemplate", "NodeMachinePluginTemplate.csproj");
                var proj = XDocument.Load(projPath);

                var compileItemGroup = proj.Root
                                           .Elements(XName.Get("ItemGroup", "http://schemas.microsoft.com/developer/msbuild/2003"))
                                           .Single(e => e.Elements(XName.Get("Compile", "http://schemas.microsoft.com/developer/msbuild/2003")).Any());

                foreach (var fileSpec in csharpFiles)
                {
                    compileItemGroup.Add(new XElement(
                        XName.Get("Compile", "http://schemas.microsoft.com/developer/msbuild/2003"),
                        new XAttribute("Include", fileSpec.Key)
                    ));
                }

                proj.Save(projPath, SaveOptions.None);

                throw new NotImplementedException("Add _Config.ini as a CopyToOutputDirectory file");

                return true;
            });
        }

        private static void UnpackTemplate(string dir)
        {
            var fs = new FileSystem();
            using (ZipInputStream s = new ZipInputStream(fs.File.OpenRead(Path.Combine("Compiler" ,"NodeMachinePluginTemplate.zip"))))
                s.UnpackToDirectory(dir, fs);
        }

        internal static string ToCSharpName(string str)
        {
            return str
                .Replace(" ", "_");
        }

        private static string TagToCSharpCode(string iname, string templateNamespace)
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

            return template;
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
