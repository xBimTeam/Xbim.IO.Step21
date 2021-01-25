using System;

namespace Xbim.IO.Step21
{
    public abstract class StatementSyntax : SyntaxNode
    {
        protected StatementSyntax(Uri syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}