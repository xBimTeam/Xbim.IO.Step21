namespace Xbim.IO.Step21.Text
{
    /// <summary>
    /// Allows the localization of a token within an <see cref="ISourceText"/>.
    /// </summary>
    public struct TextSpan
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="start">Starting index</param>
        /// <param name="length">Lenght</param>
        public TextSpan(long start, int length)
        {
            Start = start;
            Length = length;
        }

        /// <summary>
        /// Starting index of the token
        /// </summary>
        public long Start { get; }

        /// <summary>
        /// Token lenght
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Identifies the span end
        /// </summary>
        public long End => Start + Length;


        internal static TextSpan FromBounds(long start, long end)
        {
            var length = (int)(end - start);
            return new TextSpan(start, length);
        }

        /// <summary>
        /// Convenience method for testing span overlaps
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public bool OverlapsWith(TextSpan span)
        {
            return Start < span.End &&
                   End > span.Start;
        }

        /// <summary>
        /// Overrides ToString for easier debugging and presentation purposes.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{Start}..{End}";
    }
}