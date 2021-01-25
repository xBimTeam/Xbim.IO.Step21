using System.Collections;
using System.Collections.Generic;
using Xbim.IO.Step21.Text;

namespace Xbim.IO.Step21
{
    internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private readonly List<Diagnostic> _diagnostics = new List<Diagnostic>();

        public DiagnosticBag()
        {

        }

        public DiagnosticBag(
            IEnumerable<Diagnostic> d1,
            IEnumerable<Diagnostic> d2
            )
        {
            _diagnostics.AddRange(d1);
            _diagnostics.AddRange(d2);
        }

        public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void AddRange(IEnumerable<Diagnostic> diagnostics)
        {
            _diagnostics.AddRange(diagnostics);
        }

        private void Report(TextLocation location, string message)
        {
            var diagnostic = new Diagnostic(location, message);
            _diagnostics.Add(diagnostic);
        }

        internal void ReportInvalidStepIdentity(TextLocation location, string text)
        {
            var message = $"The identity {text} isn't valid, likely too long.";
            Report(location, message);
        }

        public void ReportInvalidKeyword(TextLocation location, string text)
        {
            var message = $"The keyword {text} isn't terminated as expected with a ';'.";
            Report(location, message);
        }


        public void ReportInvalidNumber(TextLocation location, string text, string type)
        {
            var message = $"The number {text} isn't valid {type}.";
            Report(location, message);
        }

        public void ReportBadCharacter(TextLocation location, char character)
        {
            var message = $"Bad character input: '{character}'.";
            Report(location, message);
        }

        public void ReportUnterminatedString(TextLocation location)
        {
            var message = "Unterminated string literal.";
            Report(location, message);
        }

        public void ReportUnterminatedEnumeration(TextLocation location)
        {
            var message = "Unterminated enumeration literal.";
            Report(location, message);
        }

        public void ReportUnterminatedMultiLineComment(TextLocation location)
        {
            var message = "Unterminated multi-line comment.";
            Report(location, message);
        }

        public void ReportUnexpectedToken(TextLocation location, SyntaxKind actualKind, SyntaxKind expectedKind)
        {
            var message = $"Unexpected token <{actualKind}>, expected <{expectedKind}>.";
            Report(location, message);
        }

        public void ReportUnexpectedToken(TextLocation location, SyntaxKind actualKind, string expectedKind)
        {
            var message = $"Unexpected token <{actualKind}>, expected <{expectedKind}>.";
            Report(location, message);
        }       
    }
}
