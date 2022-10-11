using Xbim.IO.Step21.Text;
using System.Text;

namespace Xbim.IO.Step21
{
    internal sealed class Lexer
    {
        private readonly DiagnosticBag _diagnostics = new();
        private readonly ISourceText _text;

        private StepKind _kind;
        private object? _value;

        public Lexer(ISourceText sourceText)
        {
            _text = sourceText;
        }

        public DiagnosticBag Diagnostics => _diagnostics;

        private char Current => _text.Current;

        private char Lookahead => _text.Lookahead;

        /// <summary>
        /// Attempts to save parsing time by avoiding the settin of value, whenever possible
        /// </summary>
        internal bool IgnoreValues { get; set; }

        public StepToken Lex()
        {
            _text.SetTokenStart();
            _kind = StepKind.BadTokenTrivia;
            _value = null;

            switch (Current)
            {
                case '\0':
                    _kind = StepKind.EndOfFileToken;
                    break;
                case '+':
                    if (IsNumeric(Lookahead))
                    {
                        Progress(); // consume the + and go ahead
                        ReadStepNumber();
                        break;
                    }
                    _kind = StepKind.BadTokenTrivia;
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
                    _kind = StepKind.BadTokenTrivia;
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
                    _kind = StepKind.StepOverride;
                    Progress();
                    break;
                case '$':
                    _kind = StepKind.StepUndefined;
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
                            _kind = StepKind.HashToken;
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
                        _kind = StepKind.BadTokenTrivia;
                        Progress();
                    }
                    break;
                case '(':
                    _kind = StepKind.OpenParenthesisToken;
                    Progress();
                    break;
                case ')':
                    _kind = StepKind.CloseParenthesisToken;
                    Progress();
                    break;
                case ';':
                    _kind = StepKind.SemiColonToken;
                    Progress();
                    break;
                case ',':
                    _kind = StepKind.CommaToken;
                    Progress();
                    break;
                case '=':
                    _kind = StepKind.EqualsToken;
                    Progress();
                    break;
                case '\'':
                    _kind = StepKind.StepString;
                    ReadStepString();
                    break;
                case '"':
                    _kind = StepKind.StepHex;
                    ReadHex();
                    break;
                case ' ':
                case '\t':
                case '\n':
                case '\r':
                    _kind = StepKind.WhitespaceTrivia;
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
            var text = StepFacts.GetText(_kind);
            if (IgnoreValues)
                return new StepToken(_text.Source, _kind, _text.GetBufferStartIndex(), text, null);
            text ??= CurrentBuffer();
            return new StepToken(_text.Source, _kind, _text.GetBufferStartIndex(), text, _value);
        }
    

        private void Progress()
        {
            _text.ProgressChar();
        }

        /// <summary>
        /// The kind set by this method is <see cref="StepKind.StepUndefined"/>, <see cref="StepKind.StepBoolean"/> or <see cref="StepKind.StepEnumeration"/>
        /// This requires checking the CurrentBuffer even if <see cref="IgnoreValues"/> is true.
        /// </summary>
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

            // we cannot ignore the content to determine the kind, even if <see cref="IgnoreValues"/> is true.
            string text = CurrentBuffer();
            switch (text)
            {
                case ".U.":
                    _kind = StepKind.StepUndefined;
                    break;
                case ".T.":
                    _kind = StepKind.StepBoolean;
                    _value = true;
                    break;
                case ".F.":
                    _kind = StepKind.StepBoolean;
                    _value = false;
                    break;
                default:
                    _kind = StepKind.StepEnumeration;
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
                _kind = isFloat ? StepKind.StepFloat : StepKind.StepInteger;
                return;
            }
            string text = CurrentBuffer();

            if (IsInfOrInd)
            {
                switch (text)
                {
                    case "-1.#INF":
                        _kind = StepKind.StepFloat;
                        _value = double.NegativeInfinity;
                        return;
                    case "1.#INF":
                        _kind = StepKind.StepFloat;
                        _value = double.PositiveInfinity;
                        return;
                    case "-1.#IND":
                        _kind = StepKind.StepFloat;
                        _value = double.NaN;
                        return;
                    default:
                        var span = _text.GetTokenSpan();
                        var location = new TextLocation(_text, span);
                        _diagnostics.ReportInvalidNumber(location, text, "Float");
                        _value = double.NaN;
                        _kind = StepKind.StepFloat;
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
                _kind = StepKind.StepFloat;
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
                _kind = StepKind.StepInteger;
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

            _kind = StepKind.MultiLineCommentTrivia;
        }

        private void ReadStepString()
        {
            // Skip the current quote
            Progress();
            var done = false;

            // the regex in xbim is
            // [\']([\001-\046\050-\377]|(\'\')|(\\S\\.))*[\']
            // the last \\S is the solidus for highpoint... it's \S\. and . gets increased by 128 for string value
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
                    case '\\': // backslash
                        if (Lookahead == 'S')
                        {
                            Progress(); // past \
                            Progress(); // past S
                            if (Current == '\0')
                            {
                                var span2 = _text.GetTokenSpan();
                                var location2 = new TextLocation(_text, span2);
                                _diagnostics.ReportUnterminatedString(location2);
                                done = true;
                                break;
                            }
                            Progress(); // past \
                            if (Current == '\0')
                            {
                                var span2 = _text.GetTokenSpan();
                                var location2 = new TextLocation(_text, span2);
                                _diagnostics.ReportUnterminatedString(location2);
                                done = true;
                                break;
                            }
                            Progress(); // past whatever other character follows
                        }
                        else
                            Progress();
                        break;
                    case '\'': // apostrophe
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
            _kind = StepKind.StepString;
            if (IgnoreValues)
                return;
            var t = CurrentBuffer();
            _value = t[1..^1].Replace("\'\'", "\'");
            // todo: string value should convert any \S\. and \X patterns
            // see XbimP21StringDecoder
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
                            _diagnostics.ReportUnterminatedHex(location);
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
            _kind = StepKind.StepHex;
            if (IgnoreValues)
                return;          
            _value = CurrentBuffer()[1..^1];
        }

        private void ReadWhiteSpace()
        {
            while (char.IsWhiteSpace(Current))
                Progress();
            _kind = StepKind.WhitespaceTrivia;
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
                _kind = StepKind.StepIdentityToken;
                return;
            }

            var intBuffer = new StringBuilder();
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
            _kind = StepKind.StepIdentityToken;
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

            _kind = StepFacts.GetKeywordKind(text);
            if (_kind != StepKind.StepIdentifierToken)
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