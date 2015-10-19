using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace NodeMachine.Compiler
{
    public interface IBuilder
    {
        SyntaxTree Build(List<string> messages, ISet<string> tags);
    }
}
