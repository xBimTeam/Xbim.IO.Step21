using System;

namespace Xbim.IO.Step21.Text
{
    public struct TextLocation
    {
        public TextLocation(Uri source, TextSpan span)
        {
            Source = source;
            Span = span;
            Text = null;
        }

        public TextLocation(ISourceText text, TextSpan span)
        {
            Text = text;
            Source = text.Source;
            Span = span;
        }

        public ISourceText? Text { get; }
        public Uri Source{ get; }
        public TextSpan Span { get; }

        public override string ToString()
        {
            return $"{FileName}({StartLine + 1},{StartCharacter + 1}->{EndLine + 1},{EndCharacter + 1}): ";
        }

        public Uri FileName => Source;

        public int StartLine
        {
            get
            {
                if (Text is SourceText sc)
                    return sc.GetLineIndex(Span.Start);
                return -1;
            }
        }

        public long StartCharacter
        {
            get
            {
                if (Text is SourceText st)
                    return Span.Start - st.Lines[StartLine].Start;
                return -1;
            }
        }

        public int EndLine
        {
            get
            {
                if (Text is SourceText sc)
                    return sc.GetLineIndex(Span.End);
                return -1;
            }
        }

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