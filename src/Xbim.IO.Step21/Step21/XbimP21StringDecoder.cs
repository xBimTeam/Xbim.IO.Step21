using System;
using System.Text;

namespace Xbim.IO.Step21
{
    /// <summary>
    /// Decodes a Part21 string to its unicode string equivalent.
    /// </summary>
    public class XbimP21StringDecoder
    {
        // Documentation on this can be found at 
        // http://www.buildingsmart-tech.org/implementation/get-started/string-encoding/string-encoding-decoding-summary
        // and in the stringconverter.java class thereby posted.
        //
        // available patterns are:
        //  ''                  -> '
        //  \\                  -> \
        //  \S\<1 char>         -> add 0x80 to the char
        //  \P[A-I]\            -> Use codepage ISO-8859-x where x is 1 for A, 2 for B, ... 9 for I. (eg '\PB\' sets ISO-8859-2)
        //  \X\<2 chars>        -> 1 byte hexadecimal unicode
        //  \X2\<4 chars>\X0\   -> 2 byte Unicode sequence (can be repeated until termination with \X0\)
        //  \X4\<8 chars>\X0\   -> 4 byte Unicode sequence (can be repeated until termination with \X0\)
        
        // A list of available code pages for .NET is available at
        // http://msdn.microsoft.com/en-us/library/system.text.encodinginfo.codepage.aspx
        // Unit tests have been performed with data from http://www.i18nguy.com/unicode/supplementary-test.html#utf8
        //
        private const string _singleApostrophToken = @"''";
        private const string _singleBackslashToken = @"\\";
        private const string _codeTableToken = @"\P";
        private const string _upperAsciiToken = @"\S\";
        private const string _hex8Token = @"\X\";
        private const string _hex16Token = @"\X2\";
        private const string _hex32Token = @"\X4\";
        private const string _longHexEndToken = @"\X0\";
        private const byte _upperAsciiShift = 0x80;
        private int _iCurChar;
        private readonly string _p21;
        private readonly StringBuilder _builder;
        private bool _eof;
        Encoding? _oneByteDecoder;
        Encoding OneByteDecoder
        {
            get
            {
                if (_oneByteDecoder is null)
                {
                    CodepageInitialize();
                }
                if (_oneByteDecoder is null)
                    throw new Exception("Uninitialised byte decoder.");
                return _oneByteDecoder;
            }
        }


        /// <summary>
        /// default constructor
        /// </summary>
        public XbimP21StringDecoder(string value, int codePageOverride = -1)
        {
            _p21 = value;
            CodepageInitialize(codePageOverride);
            _builder = new StringBuilder();
        }

        /// <summary>
        /// Performs the actual decoding process
        /// </summary>
        /// <returns>Unescaped value in a unicode string</returns>
        public string Unescape()
        {
            while (!_eof)
            {
                if (At(_singleApostrophToken))
                    ReplaceApostrophes();
                else if (At(_singleBackslashToken))
                    ReplaceBackSlashes();
                else if (At(_codeTableToken))
                    ParseCodeTable();
                else if (At(_upperAsciiToken))
                    ParseUpperAscii();
                else if (At(_hex8Token))
                    ParseHex8();
                else if (At(_hex16Token))
                    ParseTerminatedHex(4);
                else if (At(_hex32Token))
                    ParseTerminatedHex(8);
                else
                    CopyCharacter();
            }
            return _builder.ToString();
        }

        private void ParseCodeTable()
        {
            var CodePageIds = "ABCDEFGHI";
            MovePast(_codeTableToken);
            if (_eof || !HasLength(2)) throw new XbimP21EofException();
            var CodePageChar = CurrentChar();
            var iAddress = CodePageIds.IndexOf(CodePageChar);
            if (iAddress == -1)
                throw new XbimP21InvalidCharacterException(String.Format("Invalid codepage character '{0}'", CodePageChar));
            MoveNext();
            if (CurrentChar() != '\\')
                throw new XbimP21InvalidCharacterException(String.Format("Invalid codepage termination '{0}'", CurrentChar()));
            iAddress++;
            Move(1); // past the last backslash
            _oneByteDecoder = Encoding.GetEncoding("iso-8859-" + iAddress.ToString());
        }

        private void ReplaceBackSlashes()
        {
            MovePast(_singleBackslashToken);
            _builder.Append('\\');
        }

        private void ReplaceApostrophes()
        {
            MovePast(_singleApostrophToken);
            _builder.Append("'");
        }

        private void ParseUpperAscii()
        {
            MovePast(_upperAsciiToken);
            if (_eof) throw new XbimP21EofException();
            var val = (byte)(CurrentChar() + _upperAsciiShift);
            var upperAscii = new byte[] { val };
            _builder.Append(OneByteDecoder.GetChars(upperAscii));
            MoveNext();
        }

        private void ParseHex8()
        {
            MovePast(_hex8Token);
            if (_eof || !HasLength(2)) throw new XbimP21EofException();
            var byteval = GetHexLength(2);
            _builder.Append(OneByteDecoder.GetChars(byteval));
        }

        private byte[] GetHexLength(int StringLenght)
        {
            StringLenght /= 2;
            var ret = new byte[StringLenght];
            for (var i = 0; i < StringLenght; i++)
            {
                var hex = _p21.Substring(_iCurChar, 2);
                try
                {
                    ret[i] = Convert.ToByte(hex, 16);
                    Move(2);
                }
                catch (Exception)
                {
                    throw new XbimP21InvalidCharacterException(String.Format("Invalid hexadecimal representation '{0}'", hex));
                }
            }
            return ret;
        }

        private void ParseTerminatedHex(int stringLenght)
        {
            Move(4); // move past token
            
            // prepare decoder
            var EncodingName = "unicodeFFFE";
            if (stringLenght == 8)
                EncodingName = "utf-32BE";
            var enc = Encoding.GetEncoding(EncodingName);

            // multiple (including none) sequences of stringLenght characters could follow until the termination
            while (!At(_longHexEndToken))
            {
                if (_eof || !HasLength(stringLenght + _longHexEndToken.Length))  
                    throw new XbimP21EofException();
                var byteval = GetHexLength(stringLenght);
                _builder.Append(enc.GetChars(byteval, 0, stringLenght / 2));
            }
            MovePast(_longHexEndToken);
        }

        private void CopyCharacter()
        {
            _builder.Append(CurrentChar());
            MoveNext();
        }

        private char CurrentChar()
        {
            return _p21[_iCurChar];
        }

        private void CodepageInitialize(int codePageOverride = -1)
        {
            if (codePageOverride == -1)
               _oneByteDecoder = Encoding.GetEncoding("iso-8859-1");
            else
               _oneByteDecoder = Encoding.GetEncoding(codePageOverride);
            _eof = (_p21.Length == 0);
            _iCurChar = 0;
        }

        private bool At(string token)
        {
            return HasLength(token) &&
                   _p21.Substring(_iCurChar, token.Length).Equals(token);
        }

        private bool HasLength(string token)
        {
            return HasLength(token.Length);
        }

        private bool HasLength(int length)
        {
            return _iCurChar + length <= _p21.Length;
        }

        private void MoveNext()
        {
            Move(1);
        }

        private void MovePast(string token)
        {
            Move(token.Length);
        }

        private void Move(int length)
        {
            if (_eof) return;
            _iCurChar += length;
            _eof = (_iCurChar >= _p21.Length);
        }
    }

    /// <summary>
    /// EOF exception 
    /// </summary>
    public class XbimP21EofException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public XbimP21EofException()
            : base("Unexpected end of buffer.")
        {
        }
    }

    /// <summary>
    /// XbimP21InvalidCharacter exception 
    /// </summary>
    public class XbimP21InvalidCharacterException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public XbimP21InvalidCharacterException(string message)
            : base(message)
        {
        }
    }
}