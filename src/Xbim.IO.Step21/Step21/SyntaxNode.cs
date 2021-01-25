using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Xbim.IO.Step21.Text;

namespace Xbim.IO.Step21
{
    /// <summary>
    /// Base abstract class shared by all the nodes in the syntax.
    /// </summary>
    public abstract class SyntaxNode
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        protected SyntaxNode(Uri source)
        {
            Source = source;
        }

        /// <summary>
        /// Source data identifier
        /// </summary>
        public Uri Source { get; }

        /// <summary>
        /// The classification of the node
        /// </summary>
        public abstract SyntaxKind Kind { get; }

        /// <summary>
        /// Allows the localization of the node
        /// </summary>
        public virtual TextSpan Span
        {
            get
            {
                var first = GetChildren().First().Span;
                var last = GetChildren().Last().Span;
                return TextSpan.FromBounds(first.Start, last.End);
            }
        }

        /// <summary>
        /// The broadest span of the node.
        /// </summary>
        public virtual TextSpan FullSpan
        {
            get
            {
                var first = GetChildren().First().FullSpan;
                var last = GetChildren().Last().FullSpan;
                return TextSpan.FromBounds(first.Start, last.End);
            }
        }


        //public ImmutableArray<SyntaxTrivia> LeadingTrivia { get; set; }
        //public ImmutableArray<SyntaxTrivia> TrailingTrivia { get; set; }

        /// <summary>
        /// Provides index of char and line in the souce if available.
        /// </summary>
        public TextLocation GetLocation()
        {
            return new TextLocation(Source, Span);
        }

        /// <summary>
        /// Subcomponents of the node
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<SyntaxNode> GetChildren();

        /// <summary>
        /// Last of the node components
        /// </summary>
        public SyntaxToken GetLastToken()
        {
            if (this is SyntaxToken token)
                return token;

            // A syntax node should always contain at least 1 token.
            return GetChildren().Last().GetLastToken();
        }

        /// <summary>
        /// Hierarchical presentation of the node to a text writer
        /// </summary>
        public void WriteTo(TextWriter writer)
        {
            PrettyPrint(writer, this);
        }

        
        private static void PrettyPrint(TextWriter writer, SyntaxNode node, string indent = "", bool isLast = true)
        {
            var isToConsole = writer == Console.Out;
            var marker = isLast ? "└──" : "├──";

            if (isToConsole)
                Console.ForegroundColor = ConsoleColor.DarkGray;

            writer.Write(indent);
            writer.Write(marker);

            if (isToConsole)
                Console.ForegroundColor = node is SyntaxToken ? ConsoleColor.Blue : ConsoleColor.Cyan;

            writer.Write(node.Kind);

            if (node is SyntaxToken t)
            {
                if (t.Value != null)
                {
                    writer.Write(" ");
                    writer.Write(t.Value);
                }
                else if (t.Kind == SyntaxKind.StepIdentifierToken)
                {
                    writer.Write(" ");
                    writer.Write(t.Text);
                }
            }
            if (isToConsole)
                Console.ResetColor();

            writer.WriteLine();

            indent += isLast ? "   " : "│  ";

            var lastChild = node.GetChildren().LastOrDefault();

            foreach (var child in node.GetChildren())
                PrettyPrint(writer, child, indent, child == lastChild);
        }

        /// <summary>
        /// Hierarchical presentation of the node in string format.
        /// </summary>
        public override string ToString()
        {
            using var writer = new StringWriter();
            WriteTo(writer);
            return writer.ToString();
        }
    }
}