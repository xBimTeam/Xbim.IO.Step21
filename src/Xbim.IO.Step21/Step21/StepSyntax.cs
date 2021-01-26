using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Xbim.IO.Step21
{
    /// <summary>
    /// representation syntax for an entire STEP file
    /// </summary>
    public sealed partial class StepSyntax : SyntaxNode
    {
        /// <summary>
        /// Default contructor
        /// </summary>
        public StepSyntax(Uri syntaxTree,
            ImmutableArray<SyntaxNode> headers,
            ImmutableArray<SyntaxNode> members,
            SyntaxToken endOfFileToken)
            : base(syntaxTree)
        {
            Headers = headers;
            Members = members;
            EndOfFileToken = endOfFileToken;
        }

        /// <summary>
        /// The classification of the node
        /// </summary>
        public override SyntaxKind Kind => SyntaxKind.StepFileKeyword;

        /// <summary>
        /// All members 
        /// </summary>
        public ImmutableArray<SyntaxNode> Headers { get; }

        /// <summary>
        /// All members in the body section
        /// </summary>
        public ImmutableArray<SyntaxNode> Members { get; }

        /// <summary>
        /// The token at the end of the file
        /// </summary>
        public SyntaxToken EndOfFileToken { get; }

        /// <summary>
        /// Concrete implementation listing subcomponents of the node
        /// </summary>
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            foreach (var m in Members)
                yield return m;
            yield return EndOfFileToken;
        }
    }
}