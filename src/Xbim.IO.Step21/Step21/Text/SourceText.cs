using System;
using System.Collections.Immutable;
using System.IO;
using System.Text;

namespace Xbim.IO.Step21.Text
{
    /// <summary>
    /// Simplistic implementation of the <see cref="ISourceText"/> interface.
    /// </summary>
    public sealed class SourceText : ISourceText
    {
        private readonly string _text;

        /// <summary>
        /// Construction from text when an original URI source is available
        /// </summary>
        /// <param name="text">The content of the data</param>
        /// <param name="source">Identifier of the source</param>
        private SourceText(string text, Uri source)
        {
            _text = text;
            Source = source;
            Lines = ParseLines(this, text);
        }

        /// <summary>
        /// Construction from bare text builds an artificial URI for the source
        /// </summary>
        /// <param name="text">the data to evaluate</param>
        public static SourceText From(string text)
        {
            var dataUri = "data:," + text;
            var uri = new Uri(dataUri);
            return From(text, uri);
        }

        /// <summary>
        /// Helper method to direct parsing of files through memory allocation. This is not efficient for large files.
        /// </summary>
        /// <param name="fileInfo">File to get the data from.</param>
        public static SourceText From(FileInfo fileInfo)
        {
            var text = File.ReadAllText(fileInfo.FullName);
            var uri = new Uri(fileInfo.FullName);
            return new SourceText(text, uri);
        }

        /// <summary>
        /// Construction from text when an original URI source is available
        /// </summary>
        /// <param name="text">The content of the data</param>
        /// <param name="source">Identifier of the source</param>
        public static SourceText From(string text, Uri source)
        {
            return new SourceText(text, source);
        }

        private static ImmutableArray<TextLine> ParseLines(SourceText sourceText, string text)
        {
            var result = ImmutableArray.CreateBuilder<TextLine>();

            var position = 0;
            var lineStart = 0;

            while (position < text.Length)
            {
                var lineBreakWidth = GetLineBreakWidth(text, position);

                if (lineBreakWidth == 0)
                {
                    position++;
                }
                else
                {
                    AddLine(result, sourceText, position, lineStart, lineBreakWidth);

                    position += lineBreakWidth;
                    lineStart = position;
                }
            }

            if (position >= lineStart)
                AddLine(result, sourceText, position, lineStart, 0);

            return result.ToImmutable();
        }

        private static void AddLine(ImmutableArray<TextLine>.Builder result, SourceText sourceText, int position, int lineStart, int lineBreakWidth)
        {
            var lineLength = position - lineStart;
            var lineLengthIncludingLineBreak = lineLength + lineBreakWidth;
            var line = new TextLine(sourceText, lineStart, lineLength, lineLengthIncludingLineBreak);
            result.Add(line);
        }

        private static int GetLineBreakWidth(string text, int position)
        {
            var c = text[position];
            var l = position + 1 >= text.Length ? '\0' : text[position + 1];

            if (c == '\r' && l == '\n')
                return 2;

            if (c == '\r' || c == '\n')
                return 1;

            return 0;
        }

        internal ImmutableArray<TextLine> Lines { get; }

        private char this[int index] => _text[index];

        /// <summary>
        /// Identifier of the data source
        /// </summary>
        public Uri Source { get; }

        /// <summary>
        /// The character being parsed
        /// </summary>
        public char Current => Peek(0);

        /// <summary>
        /// The value of the upcoming character in the data
        /// </summary>
        public char Lookahead => Peek(1);


        private int _position;
        private int _start;

        private char Peek(int offset)
        {
            var index = _position + offset;

            if (index >= _text.Length)
                return '\0';

            return GetChar(index);
        }

        internal int GetLineIndex(long position)
        {
            var lower = 0;
            var upper = Lines.Length - 1;

            while (lower <= upper)
            {
                var index = lower + (upper - lower) / 2;
                var start = Lines[index].Start;

                if (position == start)
                    return index;

                if (start > position)
                {
                    upper = index - 1;
                }
                else
                {
                    lower = index + 1;
                }
            }

            return lower - 1;
        }

        /// <summary>
        /// Overrides ToString() to return the entire content.
        /// </summary>
        public override string ToString() => _text;

        private string ToString(long start, long length) => _text.Substring((int)start, (int)length);

        internal string ToString(TextSpan span) => ToString(span.Start, span.Length);

        private char GetChar(int index)
        {
            return this[index];
        }

        /// <summary>
        /// Informs the source that a token start is encountered
        /// </summary>
        public void SetTokenStart()
        {
            _start = _position;
        }

        /// <summary>
        /// Moves the cursor forward by one character
        /// </summary>
        public void ProgressChar()
        {
            _position++;
        }

        /// <summary>
        /// A string representation of the token from Start to Current position
        /// </summary>
        public string CurrentBuffer()
        {
            var length = _position - _start;
            var text = ToString(_start, length);
            return text;
        }

        /// <summary>
        /// Pointer to the portion of the source data that defines the current token
        /// </summary>        
        public TextSpan GetTokenSpan()
        {
            var length = _position - _start;
            return new TextSpan(_start, length);
        }

        /// <summary>
        /// Pointer to the start of the current token
        /// </summary>
        public long GetBufferStartIndex() => _start;
        
    }
}