using System;
using System.Collections.Generic;
using System.IO;
using Base_CityGeneration.Elements.Blocks.Spec;
using NodeMachine.Model;

namespace NodeMachine.Compiler
{
    public class BlockBuilder
        : BaseTemplatedScriptBuilder<Block, BlockSpec>
    {
        public BlockBuilder(Block block, string templateNamespace)
            : base(block, "NodeMachine.Compiler.BlockTemplate.cs", templateNamespace)
        {
            throw new NotImplementedException("Write the template referred to above");
        }

        protected override BlockSpec Deserialize(Block input)
        {
            return BlockSpec.Deserialize(new StringReader(input.Markup));
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
