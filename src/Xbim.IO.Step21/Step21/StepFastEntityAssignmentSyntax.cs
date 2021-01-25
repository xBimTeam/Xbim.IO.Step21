using System;
using System.Collections.Generic;

namespace Xbim.IO.Step21
{
    public class StepFastEntityAssignmentSyntax : SyntaxNode
    {       
        public StepFastEntityAssignmentSyntax(Uri syntaxTree, SyntaxToken identity, SyntaxToken type, SyntaxToken? firstString)
            : base(syntaxTree)
        {
            Identity = identity;
            Type = type;
            FirstString = firstString;
        }

        public override SyntaxKind Kind => SyntaxKind.StepEntityAssignmentFast;

        public SyntaxToken Identity { get; }
        public SyntaxToken Type { get; }
        public SyntaxToken? FirstString { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Identity;
            yield return Type;
            if (FirstString != null)
                yield return FirstString;
        }
    }
}
