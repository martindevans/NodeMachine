using System.Collections.Generic;

namespace NodeMachine.Compiler
{
    public interface IBuilder
    {
        string Build(ISet<string> tags);
    }
}
