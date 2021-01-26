using System;

namespace Xbim.IO.Step21.Text
{
    /// <summary>
    /// Identifies a span within a source
    /// </summary>
    public struct TextLocation
    {
        /// <summary>
        /// Convenience constructor used in diagnostics
        /// </summary>
        public TextLocation(Uri source, TextSpan span)
        {
            SourceIdentifier = source;
            Span = span;
            Text = null;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public TextLocation(ISourceText text, TextSpan span)
        {
            Text = text;
            SourceIdentifier = text.Source;
            Span = span;
        }

        /// <summary>
        /// If available the source text goes here
        /// </summary>
        public ISourceText? Text { get; }

        /// <summary>
        /// URI of the source
        /// </summary>
        public Uri SourceIdentifier{ get; }

        /// <summary>
        /// Relevant portion in the source data
        /// </summary>
        public TextSpan Span { get; }

        /// <summary>
        /// Debug friendly override
        /// </summary>
        public override string ToString()
        {
            return $"{SourceIdentifier}({StartLine + 1},{StartCharacter + 1}->{EndLine + 1},{EndCharacter + 1}): ";
        }

        /// <summary>
        /// Coordinate of the relevant text within the source
        /// </summary>
        public int StartLine
        {
            get
            {
                if (Text is SourceText sc)
                    return sc.GetLineIndex(Span.Start);
                return -1;
            }
        }

        /// <summary>
        /// Coordinate of the relevant text within the source
        /// </summary>
        public long StartCharacter
        {
            get
            {
                if (Text is SourceText st)
                    return Span.Start - st.Lines[StartLine].Start;
                return -1;
            }
        }

        /// <summary>
        /// Coordinate of the relevant text within the source
        /// </summary>
        public int EndLine
        {
            get
            {
                if (Text is SourceText sc)
                    return sc.GetLineIndex(Span.End);
                return -1;
            }
        }

        /// <summary>
        /// Coordinate of the relevant text within the source
        /// </summary>
        public long EndCharacter
        {
            get
            {
                if (Text is SourceText st)
                    return Span.End - st.Lines[EndLine].Start;
                return -1;
            }
        }
    }
}