using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace NodeMachine.Compiler
{
    public abstract class BaseTemplatedScriptBuilder<TIn, TOut>
        : IBuilder
    {
        private readonly TIn _input;
        protected TIn Input
        {
            get { return _input; }
        }

        private readonly string _templateNamespace;
        private readonly string _template;

        private TOut _deserialized;
        protected TOut Deserialized
        {
            get
            {
                if (_deserialized == null)
                    _deserialized = Deserialize(_input);
                return _deserialized;
            }
        }

        protected BaseTemplatedScriptBuilder(TIn input, string template, string templateNamespace)
        {
            _input = input;
            _templateNamespace = templateNamespace;

            // ReSharper disable once AssignNullToNotNullAttribute
            // ^ If this is null it's a programmer error!
            // ReSharper disable ExceptionNotDocumented
            // ^ GetManifestResourceStream seems to throw every kind of exception under the sun :|
            _template = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(template)).ReadToEnd();
            // ReSharper restore ExceptionNotDocumented
        }

        protected abstract TOut Deserialize(TIn input);

        protected abstract IEnumerable<KeyValuePair<string, string>> Tags();

        protected abstract string Markup();

        protected abstract string Name();

        protected abstract Guid Id();

        protected abstract string Description();

        public SyntaxTree Build(ISet<string> tags)
        {
            var template = 

                //Put markup into the right place to be parsed
                _template.Replace("/*TEMPLATED_BUILDING_SCRIPT*/", ProjectCompiler.EscapeCSharpStringLiteral(Markup()))

                //Add tags
                .Replace("/*TEMPLATED_BUILDING_TAGS*/", ProjectCompiler.BuildTagsList(Tags(), tags))

                //Put into correct namespace
                .Replace("/*TEMPLATED_NAMESPACE*/", _templateNamespace)

                //Create a new class name
                .Replace("/*TEMPLATED_BUILDING_NAME*/", ProjectCompiler.ToCSharpName(Name()))

                //Setup script attribute
                .Replace("/*TEMPLATED_BUILDING_GUID*/", Id().ToString())
                .Replace("/*TEMPLATED_BUILDING_DESCRIPTION*/", Description());

            //Sanity check for programmer error (i.e. did we forget to replace any templated values)
            if (template.Contains("TEMPLATED"))
                throw new NotImplementedException("Programmer Error!");

            return CSharpSyntaxTree.ParseText(template);
        }
    }
}
