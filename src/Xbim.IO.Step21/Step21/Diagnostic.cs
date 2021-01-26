using Xbim.IO.Step21.Text;

namespace Xbim.IO.Step21
{
    /// <summary>
    /// Diagnostic messagging from the parser
    /// </summary>
    public sealed class Diagnostic
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public Diagnostic(TextLocation location, string message)
        {
            Location = location;
            Message = message;
        }

        /// <summary>
        /// The location reference of the message
        /// </summary>
        public TextLocation Location { get; }

        /// <summary>
        /// Message
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Debug friendly override.
        /// </summary>
        public override string ToString() => Message;
    }
}