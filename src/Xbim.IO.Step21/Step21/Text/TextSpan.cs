namespace Xbim.IO.Step21.Text
{
    public struct TextSpan
    {
        public TextSpan(long start, int length)
        {
            Start = start;
            Length = length;
        }

        public long Start { get; }
        public int Length { get; }
        public long End => Start + Length;

        public static TextSpan FromBounds(long start, long end)
        {
            var length = (int)(end - start);
            return new TextSpan(start, length);
        }

        public bool OverlapsWith(TextSpan span)
        {
            return Start < span.End &&
                   End > span.Start;
        }

        public override string ToString() => $"{Start}..{End}";
    }
}