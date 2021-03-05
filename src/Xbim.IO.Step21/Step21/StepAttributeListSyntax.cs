using System;
using System.Collections.Generic;

namespace Xbim.IO.Step21
{
    /// <summary>
    /// Argument lists of an entity
    /// E.G.: (ARG1, ARG2, ..., ARGn)
    /// </summary>
    public class StepAttributeListSyntax : SyntaxNode
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly SyntaxToken _openParenthesisToken;
        public readonly SeparatedSyntaxList<SyntaxNode> Attributes;
        private readonly SyntaxToken _closeParenthesisToken;

        /// <summary>
        /// Default constructor
        /// </summary>
        public StepAttributeListSyntax(Uri syntaxTree, SyntaxToken openParenthesisToken, SeparatedSyntaxList<SyntaxNode> arguments, SyntaxToken closeParenthesisToken)
            : base (syntaxTree)
        {
            this._openParenthesisToken = openParenthesisToken;
            this.Attributes = arguments;
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
            foreach (var argument in Attributes)
            {
                yield return argument;
            }
            yield return _closeParenthesisToken;

        }
    }
}