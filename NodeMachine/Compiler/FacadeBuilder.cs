using System;
using System.Collections.Generic;
using System.IO;
using BeautifulBlueprints.Layout;
using BeautifulBlueprints.Serialization;
using NodeMachine.Model;

namespace NodeMachine.Compiler
{
    public class FacadeBuilder
        : BaseTemplatedScriptBuilder<Facade, ILayoutContainer>
    {
        public FacadeBuilder(Facade building, string templateNamespace)
            : base(building, "NodeMachine.Compiler.FacadeTemplate.cst", templateNamespace)
        {
        }

        protected override ILayoutContainer Deserialize(Facade input)
        {
            return Yaml.Deserialize(new StringReader(input.Markup));
        }

        protected override IEnumerable<KeyValuePair<string, string>> Tags()
        {
            return Deserialized;
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
