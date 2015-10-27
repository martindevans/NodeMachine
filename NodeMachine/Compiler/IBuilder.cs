using System.Collections.Generic;

namespace NodeMachine.Compiler
{
    public interface IBuilder
    {
        string Name();
        
        string Build(ISet<string> tags);
    }
}
