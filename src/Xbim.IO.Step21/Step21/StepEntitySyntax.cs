using System;
using System.Collections.Generic;

namespace Xbim.IO.Step21
{
    public class StepEntitySyntax : SyntaxNode
    {
        private readonly SyntaxToken _type;
        private readonly StepArgumentListSyntax _argumentList;

        public StepEntitySyntax(Uri syntaxTree, SyntaxToken type, StepArgumentListSyntax argumentList)
            : base(syntaxTree)
        {
            _type = type;
            _argumentList = argumentList;
        }

        public override SyntaxKind Kind => SyntaxKind.StepEntity;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return _type;
            yield return _argumentList;
        }
    }
}
