using System;
using System.Collections.Generic;

namespace Xbim.IO.Step21
{
    public class StepEntityAssignmentSyntax : SyntaxNode
    {
        public StepEntityAssignmentSyntax(Uri syntaxTree, SyntaxToken identity, StepEntitySyntax entity)
            : base (syntaxTree)
        {
            Identity = identity;
            Entity = entity;
        }

        public override SyntaxKind Kind => SyntaxKind.StepEntityAssignment;

        public SyntaxToken Identity { get; }
        public StepEntitySyntax Entity { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Identity;
            yield return Entity;
        }
    }
}
