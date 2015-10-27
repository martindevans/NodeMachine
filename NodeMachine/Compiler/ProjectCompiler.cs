using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using EpimetheusPlugins.Scripts;
using HandyCollections.Extensions;
using ICSharpCode.SharpZipLib.Zip;
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

                try
                {

                    //If the output directory does not exist, early exit
                    var dir = _project.ProjectData.CompileOutputDirectory;
                    if (dir == null || !Directory.Exists(dir))
                    {
                        outputMessages.Add(string.Format("{0}: {1}", "DirectoryNotFound", _project.ProjectData.CompileOutputDirectory));
                        return false;
                    }

                    //Unpack the project template into the directory
                    UnpackTemplate(dir);

                    //Accumilate all builders
                    var builders =
                        _project.ProjectData.Buildings.Select<Model.Building, IBuilder>(b => new BuildingBuilder(b, templateNamespace))
                                .Append(_project.ProjectData.Blocks.Select(b => new BlockBuilder(b, templateNamespace)))
                                .Append(_project.ProjectData.Facades.Select(f => new FacadeBuilder(f, templateNamespace)));
                    //todo: Compile City
                    //todo: Compile Floor
                    //todo: Compile Room

                    //Run the builders
                    foreach (var builder in builders)
                        csharpFiles.Add(new KeyValuePair<string, string>(builder.Name() + ".cs", builder.Build(tags)));

                    //Convert all the tags we've discovered into syntax trees of empty interfaces
                    csharpFiles.AddRange(tags.Select(t => new KeyValuePair<string, string>(t + ".cs", TagToCSharpCode(t, templateNamespace))));

                    //Output the files into the project directory
                    foreach (var fileSpec in csharpFiles)
                    {
                        using (var f = File.Create(Path.Combine(dir, "NodeMachinePluginTemplate", fileSpec.Key)))
                        using (var w = new StreamWriter(f))
                        {
                            w.Write(fileSpec.Value);
                        }
                    }

                    //Parse template project XML
                    var projPath = Path.Combine(dir, "NodeMachinePluginTemplate", "NodeMachinePluginTemplate.csproj");
                    var proj = XDocument.Load(projPath);

                    //Find the ItemGroup with Compile elements
                    var compileItemGroup = proj.Root
                                               .Elements(XName.Get("ItemGroup", "http://schemas.microsoft.com/developer/msbuild/2003"))
                                               .Single(e => e.Elements(XName.Get("Compile", "http://schemas.microsoft.com/developer/msbuild/2003")).Any());

                    //Find the ItemGroup with None elements
                    var noneItemGroup = proj.Root
                                            .Elements(XName.Get("ItemGroup", "http://schemas.microsoft.com/developer/msbuild/2003"))
                                            .Single(e => e.Elements(XName.Get("None", "http://schemas.microsoft.com/developer/msbuild/2003")).Any());

                    // Write out the _Config.ini as a None element with a copy command
                    using (var file = File.Create(Path.Combine(dir, Path.Combine("NodeMachinePluginTemplate", "_Config.ini"))))
                    using (var writer = new StreamWriter(file))
                    {
                        writer.WriteLine("id={0}", _project.ProjectData.Guid);
                        writer.WriteLine("load=NodeMachinePluginTemplate.exe");

                        foreach (var metadataValue in _project.ProjectData.Metadata)
                            writer.WriteLine("{0}={1}", metadataValue.Key, metadataValue.Value);
                    }
                    noneItemGroup.Add(new XElement(XName.Get("None", "http://schemas.microsoft.com/developer/msbuild/2003"),
                        new XAttribute("Include", "_Config.ini"),
                        new XElement(XName.Get("CopyToOutputDirectory", "http://schemas.microsoft.com/developer/msbuild/2003"), "PreserveNewest")
                        ));

                    //Add the Csharp files as Compile elements
                    foreach (var fileSpec in csharpFiles)
                    {
                        compileItemGroup.Add(new XElement(
                            XName.Get("Compile", "http://schemas.microsoft.com/developer/msbuild/2003"),
                            new XAttribute("Include", fileSpec.Key)
                            ));
                    }

                    proj.Save(projPath, SaveOptions.None);

                    return true;
                }
                // ReSharper disable once CatchAllClause
                catch (Exception e)
                {

                    outputMessages.Add(e.ToString());
                    return false;
                }
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
                .Replace(" ", "_")
                .Replace("(", "_lp_") //lparen
                .Replace(")", "_rp_") //rparen
                .Replace("]", "_ls_") //lsquare
                .Replace("[", "_rs_") //rsquare
                .Replace("{", "_lb_") //lbrace
                .Replace("}", "_rb_") //rbrace
                ;
        }

        private static string TagToCSharpCode(string iname, string templateNamespace)
        {
            var template =
@"namespace NM_/*TEMPLATED_NAMESPACE*/
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
