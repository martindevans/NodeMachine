using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace NodeMachine.Compiler
{
    public interface IBuilder
    {
        SyntaxTree Build(ISet<string> tags);
    }
}
