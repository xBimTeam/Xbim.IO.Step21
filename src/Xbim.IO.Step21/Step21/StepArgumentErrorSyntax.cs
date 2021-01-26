using System;
using System.Collections.Generic;

namespace Xbim.IO.Step21
{
    /// <summary>
    /// Provides structure of an unexpected sequence in the data
    /// </summary>
    public class StepArgumentErrorSyntax : SyntaxNode
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public StepArgumentErrorSyntax(Uri syntaxTree, SyntaxToken value)
            : base(syntaxTree)
        {
            Value = value;
        }

        /// <summary>
        /// The classification of the node
        /// </summary>
        public override SyntaxKind Kind => SyntaxKind.StepArgumentError;

        /// <summary>
        /// The unexpected token found
        /// </summary>
        public SyntaxToken Value { get; }

        /// <summary>
        /// The unexpected token found is the only component of the syntax.
        /// </summary>
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Value;
        }
    }
}
