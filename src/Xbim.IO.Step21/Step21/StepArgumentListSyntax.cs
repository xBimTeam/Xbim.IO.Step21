using System;
using System.Collections.Generic;

namespace Xbim.IO.Step21
{
    public class StepArgumentListSyntax : SyntaxNode
    {
        private readonly SyntaxToken _openParenthesisToken;
        private readonly SeparatedSyntaxList<SyntaxNode> _arguments;
        private readonly SyntaxToken _closeParenthesisToken;
        public StepArgumentListSyntax(Uri syntaxTree, SyntaxToken openParenthesisToken, SeparatedSyntaxList<SyntaxNode> arguments, SyntaxToken closeParenthesisToken)
            : base (syntaxTree)
        {
            this._openParenthesisToken = openParenthesisToken;
            this._arguments = arguments;
            this._closeParenthesisToken = closeParenthesisToken;
        }

        public override SyntaxKind Kind => SyntaxKind.StepArgumentsList;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return _openParenthesisToken;
            foreach (var argument in _arguments)
            {
                yield return argument;
            }
            yield return _closeParenthesisToken;

        }
    }
}