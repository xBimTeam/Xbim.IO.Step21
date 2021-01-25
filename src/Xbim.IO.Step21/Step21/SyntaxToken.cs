using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Xbim.IO.Step21.Text;

namespace Xbim.IO.Step21
{
    public sealed class SyntaxToken : SyntaxNode
    {
        public SyntaxToken(Uri syntaxTree, SyntaxKind kind, long position, string? text, object? value)
            : base(syntaxTree)
        {
            Kind = kind;
            Position = position;
            Text = text ?? string.Empty;
            Value = value;
        }

        public override SyntaxKind Kind { get; }
        public long Position { get; }
        public string Text { get; }
        public object? Value { get; }
        public override TextSpan Span => new TextSpan(Position, Text?.Length ?? 0);
        //public override TextSpan FullSpan
        //{
        //    get
        //    {
        //        var start = LeadingTrivia.Length == 0
        //                        ? Span.Start
        //                        : LeadingTrivia.First().Span.Start;
        //        var end = TrailingTrivia.Length == 0
        //                        ? Span.End
        //                        : TrailingTrivia.Last().Span.End;
        //        return TextSpan.FromBounds(start, end);
        //    }
        //}

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
