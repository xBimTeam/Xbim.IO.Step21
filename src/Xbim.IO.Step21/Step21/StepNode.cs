using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Xbim.IO.Step21.Text;

namespace Xbim.IO.Step21
{
    /// <summary>
    /// Base abstract class shared by all the nodes in the syntax.
    /// </summary>
    public abstract class StepNode
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        protected StepNode(Uri source)
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
        public abstract StepKind Kind { get; }

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
        public abstract IEnumerable<StepNode> GetChildren();

        /// <summary>
        /// Last of the node components
        /// </summary>
        public StepToken GetLastToken()
        {
            if (this is StepToken token)
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

        
        private static void PrettyPrint(TextWriter writer, StepNode node, string indent = "", bool isLast = true)
        {
            var isToConsole = writer == Console.Out;
            var marker = isLast ? "└──" : "├──";

            if (isToConsole)
                Console.ForegroundColor = ConsoleColor.DarkGray;

            writer.Write(indent);
            writer.Write(marker);

            if (isToConsole)
                Console.ForegroundColor = node is StepToken ? ConsoleColor.Blue : ConsoleColor.Cyan;

            writer.Write(node.Kind);

            if (node is StepToken t)
            {
                if (t.Value != null)
                {
                    writer.Write(" ");
                    writer.Write(t.Value);
                }
                else if (t.Kind == StepKind.StepIdentifierToken)
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

        /// <summary>
        /// Writes the Node as Part21 string to the <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">Destination writer</param>
        public virtual void WritePart21(StreamWriter writer)
        {
            if (this is StepToken t)
            {
                if (t.Kind == StepKind.StepString)
                    writer.Write(FixTextEncoding(t.Text));
                else
                    writer.Write(t.Text);
            }
            foreach (var child in this.GetChildren())
                child.WritePart21(writer);

        }

        private static string FixTextEncoding(string text)
        {
            // input text starts and end with '
            if (text.Length < 2)
                return "''";
            text = text[1..^1];
            var d = new XbimP21StringDecoder(text);
            return $"'{ToPart21(d.Unescape())}'";
        }

        /// <summary>
        /// Static helper to convert string to Express Part21
        /// </summary>
        /// <param name="source">The source unicode string</param>
        /// <returns>The encoded version according to Part21 specifications</returns>
        public static string ToPart21(string source)
        {
            if (string.IsNullOrEmpty(source)) return "";
            var state = WriteState.Normal;
            var sb = new StringBuilder(source.Length * 2);


            for (var i = 0; i < source.Length; i++)
            {
                int c;
                try
                {
                    c = char.ConvertToUtf32(source, i);
                }
                catch (Exception)
                {
                    c = '?';
                }
                if (c > 0xFFFF)
                {
                    state = SetMode(WriteState.FourBytes, state, sb);
                    sb.AppendFormat(@"{0:X8}", c);
                    i++; // to skip the next surrogate
                }
                else if (c > 0xFF)
                {
                    state = SetMode(WriteState.TwoBytes, state, sb);
                    sb.AppendFormat(@"{0:X4}", c);
                }
                else
                {
                    state = SetMode(WriteState.Normal, state, sb);
                    // boundaries according to specs from http://www.buildingsmart-tech.org/downloads/accompanying-documents/guidelines/IFC2x%20Model%20Implementation%20Guide%20V2-0b.pdf
                    if (c > 126 || c < 32)
                        sb.AppendFormat(@"\X\{0:X2}", c);
                    //needs un-escaping as this is converting SIZE: 2'x2'x3/4" to SIZE: 2''x2''x3/4" and Manufacturer's to Manufacturer''s 
                    else if ((char)c == '\'')
                        sb.Append("''");
                    else if ((char)c == '\\')
                        sb.Append("\\\\");
                    else
                        sb.Append((char)c);
                }
            }
            SetMode(WriteState.Normal, state, sb);
            return sb.ToString();
        }

        private enum WriteState
        {
            Normal,
            TwoBytes,
            FourBytes
        }

        private static WriteState SetMode(WriteState newState, WriteState fromState, StringBuilder sb)
        {
            if (newState == fromState)
                return newState;
            if (fromState != WriteState.Normal)
            {
                // needs to close the old state
                sb.Append(@"\X0\");
            }
            if (newState == WriteState.TwoBytes)
            {
                sb.Append(@"\X2\");
            }
            else if (newState == WriteState.FourBytes)
            {
                sb.Append(@"\X4\");
            }
            return newState;
        }

    }
}