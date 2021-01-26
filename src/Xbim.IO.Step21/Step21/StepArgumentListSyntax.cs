using System;
using System.Collections.Generic;

namespace Xbim.IO.Step21
{
    /// <summary>
    /// Argument lists of an entity
    /// E.G.: (ARG1, ARG2, ..., ARGn)
    /// </summary>
    public class StepArgumentListSyntax : SyntaxNode
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly SyntaxToken _openParenthesisToken;
        private readonly SeparatedSyntaxList<SyntaxNode> _arguments;
        private readonly SyntaxToken _closeParenthesisToken;

        /// <summary>
        /// Default constructor
        /// </summary>
        public StepArgumentListSyntax(Uri syntaxTree, SyntaxToken openParenthesisToken, SeparatedSyntaxList<SyntaxNode> arguments, SyntaxToken closeParenthesisToken)
            : base (syntaxTree)
        {
            this._openParenthesisToken = openParenthesisToken;
            this._arguments = arguments;
            this._closeParenthesisToken = closeParenthesisToken;
        }

        /// <summary>
        /// The classification of the node
        /// </summary>
        public override SyntaxKind Kind => SyntaxKind.StepArgumentsList;

        /// <summary>
        /// Concrete implementation listing subcomponents of the node
        /// </summary>
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