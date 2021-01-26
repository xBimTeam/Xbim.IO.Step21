using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Xbim.IO.Step21.Text;

namespace Xbim.IO.Step21
{
    /// <summary>
    /// Base class for complex syntax tokens
    /// </summary>
    public sealed class SyntaxToken : SyntaxNode
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SyntaxToken(Uri syntaxTree, SyntaxKind kind, long position, string? text, object? value)
            : base(syntaxTree)
        {
            Kind = kind;
            Position = position;
            Text = text ?? string.Empty;
            Value = value;
        }

        /// <summary>
        /// The classification of the node
        /// </summary>
        public override SyntaxKind Kind { get; }

        /// <summary>
        /// Position of token in the data
        /// </summary>
        public long Position { get; }

        /// <summary>
        /// Entire text of the token
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// The value of the token in specific type if meaningful.
        /// </summary>
        public object? Value { get; }

        /// <summary>
        /// The region of data occupied by the token
        /// </summary>
        public override TextSpan Span => new TextSpan(Position, Text?.Length ?? 0);

        /// <summary>
        /// Concrete implementation listing subcomponents of the node
        /// </summary>
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            return Array.Empty<SyntaxNode>();
        }

        /// <summary>
        /// A token is missing if it was inserted by the parser and doesn't appear in source.
        /// </summary>
        public bool IsMissing => Text == null;
    }
}
