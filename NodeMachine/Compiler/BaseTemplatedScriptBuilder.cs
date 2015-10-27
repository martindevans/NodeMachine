using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

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

        public abstract string Name();

        protected abstract Guid Id();

        protected abstract string Description();

        public string Build(ISet<string> tags)
        {
            var template = 

                //Put markup into the right place to be parsed
                _template.Replace("/*TEMPLATED_SCRIPT*/", ProjectCompiler.EscapeCSharpStringLiteral(Markup()))

                //Add tags
                .Replace("/*TEMPLATED_TAGS*/", ProjectCompiler.BuildTagsList(Tags(), tags))

                //Put into correct namespace
                .Replace("/*TEMPLATED_NAMESPACE*/", _templateNamespace)

                //Create a new class name
                .Replace("/*TEMPLATED_NAME*/", ProjectCompiler.ToCSharpName(Name()))

                //Setup script attribute
                .Replace("/*TEMPLATED_GUID*/", Id().ToString())
                .Replace("/*TEMPLATED_DESCRIPTION*/", Description());

            //Sanity check for programmer error (i.e. did we forget to replace any templated values)
            if (template.Contains("TEMPLATED_"))
                throw new NotImplementedException(string.Format("Programmer Error in {0}", GetType().Name));

            return template;
        }
    }
}
