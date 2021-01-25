using Xbim.IO.Step21.Text;
using System;

namespace Xbim.IO.Step21
{
    /// <summary>
    /// Define requirements for accessing the data by the parser
    /// </summary>
    public interface ISourceText
    {
        /// <summary>
        /// Identifier of the data source
        /// </summary>
        Uri Source { get; }

        /// <summary>
        /// The character being parsed
        /// </summary>
        char Current { get; }

        /// <summary>
        /// The value of the upcoming character in the data
        /// </summary>
        char Lookahead { get; }

        /// <summary>
        /// Informs the source that a token start is encountered
        /// </summary>
        void SetTokenStart();

        /// <summary>
        /// Moves the cursor forward by one character
        /// </summary>
        void ProgressChar();

        /// <summary>
        /// A string representation of the token from Start to Current position
        /// </summary>
        /// <returns>A string representation</returns>
        string CurrentBuffer();

        /// <summary>
        /// Pointer to the portion of the source data that defines the current token
        /// </summary>
        /// <returns>the relevant TextSpan</returns>
        TextSpan GetTokenSpan();

        /// <summary>
        /// Pointer to the start of the current token
        /// </summary>
        /// <returns>the long integer of the token start</returns>
        long GetBufferStartIndex();

    }
}