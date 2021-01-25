using Xbim.IO.Step21.Text;
using System.Text;

namespace Xbim.IO.Step21
{
    internal sealed class Lexer
    {
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
        private readonly ISourceText _text;

        private SyntaxKind _kind;
        private object? _value;

        public Lexer(ISourceText sourceText)
        {
            _text = sourceText;
        }

        public DiagnosticBag Diagnostics => _diagnostics;

        private char Current => _text.Current;

        private char Lookahead => _text.Lookahead;

        public bool IgnoreValues { get; internal set; }

        public SyntaxToken Lex()
        {
            _text.SetTokenStart();
            _kind = SyntaxKind.BadTokenTrivia;
            _value = null;

            switch (Current)
            {
                case '\0':
                    _kind = SyntaxKind.EndOfFileToken;
                    break;
                case '+':
                    if (IsNumeric(Lookahead))
                    {
                        Progress(); // consume the + and go ahead
                        ReadStepNumber();
                        break;
                    }
                    _kind = SyntaxKind.BadTokenTrivia;
                    Progress();
                    break;
                case '-':
                    if (IsNumeric(Lookahead))
                    {
                        Progress(); // consume the - and go ahead
                        ReadStepNumber();
                        break;
                    }
                    Progress(); 
                    _kind = SyntaxKind.BadTokenTrivia;
                    break;
                case '.':
                    if (char.IsDigit(Lookahead)) // not using IsNumeric() because we dont accept another . after the current .
                    {
                        ReadStepNumber();
                        break;
                    }
                    else
                    {
                        ReadStepEnumStyle();
                        break;
                    }                    
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    ReadStepNumber();
                    break;
                case '*':
                    _kind = SyntaxKind.StepOverride;
                    Progress();
                    break;
                case '$':
                    _kind = SyntaxKind.StepUndefined;
                    Progress();
                    break;
                case '#':
                    switch (Lookahead)
                    {
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            ReadStepIdentity();
                            break;
                        default:
                            Progress();
                            _kind = SyntaxKind.HashToken;
                            break;
                    }
                    break;
                case '/':
                    if (Lookahead == '*')
                    {
                        ReadMultiLineComment();
                    }
                    else
                    {
                        _kind = SyntaxKind.BadTokenTrivia;
                        Progress();
                    }
                    break;
                case '(':
                    _kind = SyntaxKind.OpenParenthesisToken;
                    Progress();
                    break;
                case ')':
                    _kind = SyntaxKind.CloseParenthesisToken;
                    Progress();
                    break;
                case ';':
                    _kind = SyntaxKind.SemiColonToken;
                    Progress();
                    break;
                case ',':
                    _kind = SyntaxKind.CommaToken;
                    Progress();
                    break;
                case '=':
                    _kind = SyntaxKind.EqualsToken;
                    Progress();
                    break;
                case '\'':
                    ReadStepString();
                    break;
                case '"':
                    ReadHex();
                    break;
                case ' ':
                case '\t':
                case '\n':
                case '\r':
                    ReadWhiteSpace();
                    break;
                case '_':
                    ReadIdentifierOrKeyword();
                    break;
                default:
                    if (char.IsLetter(Current))
                    {
                        ReadIdentifierOrKeyword();
                    }
                    else if (char.IsWhiteSpace(Current))
                    {
                        ReadWhiteSpace();
                    }
                    else
                    {
                        var span = _text.GetTokenSpan();
                        var location = new TextLocation(_text, span);
                        _diagnostics.ReportBadCharacter(location, Current);
                        Progress();
                    }
                    break;
            }
            if (IgnoreValues)
                return new SyntaxToken(_text.Source, _kind, _text.GetBufferStartIndex(), "", null);
            var text = SyntaxFacts.GetText(_kind);
            if (text == null)
            {
                text = CurrentBuffer();
            }
            return new SyntaxToken(_text.Source, _kind, _text.GetBufferStartIndex(), text, _value);
        }
    

        private void Progress()
        {
            _text.ProgressChar();
        }

        private void ReadStepEnumStyle()
        {
            Progress(); // skip initial "."           
            var done = false;
            while (!done)
            {
                switch (Current)
                {
                    case '\0':
                    case '\r':
                    case '\n':
                        var span = _text.GetTokenSpan();
                        var location = new TextLocation(_text, span);
                        _diagnostics.ReportUnterminatedEnumeration(location);
                        done = true;
                        break;
                    case '.':
                        Progress();
                        done = true;                      
                        break;
                    default:
                        Progress();
                        break;
                }
            }

            // we cannot ignore the content to determine the return,
            // even if <see cref="IgnoreValues"/> is true.
            string text = CurrentBuffer();
            switch (text)
            {
                case ".U.":
                    _kind = SyntaxKind.StepUndefined;
                    break;
                case ".T.":
                    _kind = SyntaxKind.StepBoolean;
                    _value = true;
                    break;
                case ".F.":
                    _kind = SyntaxKind.StepBoolean;
                    _value = false;
                    break;
                default:
                    _kind = SyntaxKind.StepEnumeration;
                    _value = text;
                    break;
            }
        }

        private void ReadStepNumber()
        {
            // potential leading [+-] are consumend before this moment.
            
            bool isFloat = false;
            while (char.IsDigit(Current))
                Progress(); // consumed all the int options
            if (Current == '.')
            {
                Progress();
                isFloat = true;
            }
            while (char.IsDigit(Current))
                Progress();
            if (Current == 'e' || Current == 'E') // in case we are parsing an exp.
            {
                Progress();
                if (Current == '+' || Current == '-') // exp sign
                    Progress();
                while (char.IsDigit(Current)) // get the remaining digits
                    Progress();
            }

            bool IsInfOrInd = false;
            // all standard digits are consumed, deal with IFCCARTESIANPOINT((-1.#INF,-1.#IND,1.#INF));
            if (Current == '#')
            {
                IsInfOrInd = true;
                Progress();
                if (Current == 'I')
                {
                    Progress();
                    if (Current == 'N')
                    {
                        Progress();
                        if (Current == 'F' || Current == 'D')
                        {
                            Progress();
                        }
                    }
                }
            }
            if (IgnoreValues)
            {
                _kind = isFloat ? SyntaxKind.StepFloat : SyntaxKind.StepInteger;
                return;
            }
            string text = CurrentBuffer();

            if (IsInfOrInd)
            {
                switch (text)
                {
                    case "-1.#INF":
                        _kind = SyntaxKind.StepFloat;
                        _value = double.NegativeInfinity;
                        return;
                    case "1.#INF":
                        _kind = SyntaxKind.StepFloat;
                        _value = double.PositiveInfinity;
                        return;
                    case "-1.#IND":
                        _kind = SyntaxKind.StepFloat;
                        _value = double.NaN;
                        return;
                    default:
                        var span = _text.GetTokenSpan();
                        var location = new TextLocation(_text, span);
                        _diagnostics.ReportInvalidNumber(location, text, "Float");
                        _value = double.NaN;
                        _kind = SyntaxKind.StepFloat;
                        return;
                }
            }

            if (isFloat)
            {
                if (!double.TryParse(text, out var value))
                {
                    var span = _text.GetTokenSpan();
                    var location = new TextLocation(_text, span);
                    _diagnostics.ReportInvalidNumber(location, text, "Float");
                }
                _value = value;
                _kind = SyntaxKind.StepFloat;
            }
            else
            {
                if (!int.TryParse(text, out var value))
                {
                    var span = _text.GetTokenSpan();
                    var location = new TextLocation(_text, span);
                    _diagnostics.ReportInvalidNumber(location, text, "Int");
                }
                _value = value;
                _kind = SyntaxKind.StepInteger;
            }
        }

        private bool IsNumeric(char tocheck)
        {
            return tocheck switch
            {
                '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or '.' => true,
                _ => false,
            };
        }

        private void ReadMultiLineComment()
        {
            Progress();
            Progress();
            var done = false;

            while (!done)
            {
                switch (Current)
                {
                    case '\0':
                        var span = _text.GetTokenSpan(); // BUG, I think this could be not right, it should be lenght (was 2)
                        var location = new TextLocation(_text, span);
                        _diagnostics.ReportUnterminatedMultiLineComment(location);
                        done = true;
                        break;
                    case '*':
                        if (Lookahead == '/')
                        {
                            Progress();
                            done = true;
                        }
                        Progress();
                        break;
                    default:
                        Progress();
                        break;
                }
            }

            _kind = SyntaxKind.MultiLineCommentTrivia;
        }

        private void ReadStepString()
        {
            // Skip the current quote
            Progress();
            var done = false;

            // the regex in xbim is
            // [\']([\001-\046\050-\377]|(\'\')|(\\S\\.))*[\']
            // the last \\S... is weird, we'll just get everyting until \'
            while (!done)
            {
                switch (Current)
                {
                    case '\0':
                        var span = _text.GetTokenSpan();
                        var location = new TextLocation(_text, span);
                        _diagnostics.ReportUnterminatedString(location);
                        done = true;
                        break;
                    case '\'':
                        if (Lookahead == '\'')
                        {
                            
                            Progress();
                            Progress();
                        }
                        else
                        {
                            Progress();
                            done = true;
                        }
                        break;
                    default:
                        
                        Progress();
                        break;
                }
            }
            _kind = SyntaxKind.StepString;
            if (IgnoreValues)
                return;
            var t = CurrentBuffer();
            _value = t[1..^1].Replace("\'\'", "\'");
        }

        private void ReadHex()
        {
            // Skip the current quote
            Progress();

            var done = false;

            // the regex in xbim is
            // [\"][0-9A-Fa-f]+[\"]
            // we'll probably ignore the + and use a * approach instead
            while (!done)
            {
                switch (Current)
                {
                    case '\0':
                        {
                            var span = _text.GetTokenSpan(); 
                            var location = new TextLocation(_text, span);
                            _diagnostics.ReportUnterminatedString(location);
                        }
                        done = true;
                        break;
                    case '"':
                        Progress();
                        done = true;
                        break;
                    case '0': case '1': case '2': case '3': case '4':
                    case '5': case '6': case '7': case '8': case '9':
                    case 'A': case 'B': case 'C': case 'D': case 'E': case 'F':
                    case 'a': case 'b': case 'c': case 'd': case 'e': case 'f':
                        Progress();
                        break;
                    default:
                        {
                            var span = _text.GetTokenSpan(); // BUG? this was 1
                            var location = new TextLocation(_text, span);
                            _diagnostics.ReportBadCharacter(location, Current);
                        }
                        done = true;
                        break;
                }
            }
            _kind = SyntaxKind.StepHex;
            if (IgnoreValues)
                return;          
            _value = CurrentBuffer()[1..^1];
        }

        private void ReadWhiteSpace()
        {
            while (char.IsWhiteSpace(Current))
                Progress();
            _kind = SyntaxKind.WhitespaceTrivia;
        }

        /// <summary>
        /// Reads the identity in a ulong format - convert to int as appropriate
        /// </summary>
        private void ReadStepIdentity()
        {
            Progress(); // skip the hash

            if (IgnoreValues)
            {
                while (char.IsDigit(Current))
                {
                    Progress();
                }
                _kind = SyntaxKind.StepIdentityToken;
                return;
            }

            StringBuilder intBuffer = new StringBuilder();
            while (char.IsDigit(Current))
            {
                intBuffer.Append(Current);
                Progress();
            }
            if (!ulong.TryParse(intBuffer.ToString(), out var ulongValue))
            {
                string text = CurrentBuffer();
                var span = _text.GetTokenSpan();
                var location = new TextLocation(_text, span);
                _diagnostics.ReportInvalidStepIdentity(location, text);
            }
            _value = ulongValue;
            _kind = SyntaxKind.StepIdentityToken;
        }

        private void ReadIdentifierOrKeyword()
        {
            var sb = new StringBuilder();
            var done = false;
            while (!done)
            {
                while (char.IsLetterOrDigit(Current) || Current == '_')
                {
                    sb.Append(Current);
                    Progress();

                }
                done = true;
                if (Current == '-')
                {
                    var currVal = sb.ToString();
                    if (currVal.StartsWith("ISO") || currVal.StartsWith("END")) // special keywords with - character
                    {
                        sb.Append(Current);
                        Progress();
                        done = false;
                    }
                }
            }

            string text = CurrentBuffer();

            _kind = SyntaxFacts.GetKeywordKind(text);
            if (_kind != SyntaxKind.StepIdentifierToken)
            {
                // for all keyords we expect a ;
                if (Current == ';')
                    Progress();
                else
                {
                    var span = _text.GetTokenSpan();
                    var location = new TextLocation(_text, span);
                    _diagnostics.ReportInvalidKeyword(location, text);
                }
            }
        }
        private string CurrentBuffer()
        {
            return _text.CurrentBuffer();
        }
    }
}