using System;
using System.Collections.Generic;

namespace Xbim.IO.Step21
{
    public class StepArgumentErrorSyntax : SyntaxNode
    {
        public StepArgumentErrorSyntax(Uri syntaxTree, SyntaxToken value)
            : base(syntaxTree)
        {
            Value = value;
        }
    
        public override SyntaxKind Kind => SyntaxKind.StepArgumentError;

        public SyntaxToken Value { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Value;
        }
    }
}
