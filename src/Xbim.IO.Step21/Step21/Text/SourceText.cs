using System;
using System.Collections.Immutable;
using System.IO;
using System.Text;

namespace Xbim.IO.Step21.Text
{
    public sealed class SourceText : ISourceText
    {
        private readonly string _text;

        private SourceText(string text, Uri fileName)
        {
            _text = text;
            Source = fileName;
            Lines = ParseLines(this, text);
        }

        public static SourceText From(string text)
        {
            var dataUri = "data:," + text;
            var uri = new Uri(dataUri);
            return From(text, uri);
        }

        public static SourceText From(FileInfo fileInfo)
        {
            var text = File.ReadAllText(fileInfo.FullName);
            var uri = new Uri(fileInfo.FullName);
            return new SourceText(text, uri);
        }

        public static SourceText From(string text, Uri fileName)
        {
            return new SourceText(text, fileName);
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

        public char this[int index] => _text[index];

        // public int Length => _text.Length;

        public Uri Source { get; }

        public char Current => Peek(0);

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

        public int GetLineIndex(long position)
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

        public override string ToString() => _text;

        // used for currentbuffer
        private string ToString(long start, long length) => _text.Substring((int)start, (int)length);

        public string ToString(TextSpan span) => ToString(span.Start, span.Length);

        private char GetChar(int index)
        {
            return this[index];
        }

        public void SetTokenStart()
        {
            _start = _position;
        }

        public void ProgressChar()
        {
            _position++;
        }

        public string CurrentBuffer()
        {
            var length = _position - _start;
            var text = ToString(_start, length);
            return text;
        }

        public TextSpan GetTokenSpan()
        {
            var length = _position - _start;
            return new TextSpan(_start, length);
        }

        public long GetBufferStartIndex() => _start;
        
    }
}