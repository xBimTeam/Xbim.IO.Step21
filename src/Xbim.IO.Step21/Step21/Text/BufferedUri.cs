using Xbim.IO.Step21.Text;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Xbim.IO.Step21.Step21.Text
{
    public class BufferedUri : ISourceText, IDisposable
    {
        [Conditional("DEBUG")]
        private void Warning(int bufferSize)
        {
            if (bufferSize < 16000)
            {
                Debug.WriteLine("Are you sure you want to use such small buffer?");
            }
        }

        public BufferedUri(FileInfo f, int BufferSize = 128000) // default 128K
        {
            if (BufferSize < 2)
                throw new ArgumentOutOfRangeException(nameof(BufferSize), "Argument cannot be smaller than 2");
            Warning(BufferSize);
           
            _disposedValue = false;
            _sourceFile = new Uri(f.FullName);
            _streamReader = new StreamReader(f.FullName, true);
            _buffer = new char[BufferSize];
            _bufferLen = _streamReader.ReadBlock(_buffer);
            _position = 0;
            _start = 0;
            _spanOffset = 0;
            
            _crossingCurrentBuilder = null;
            _current = _bufferLen > 0 ? _buffer[0] : '\0';
            _lookAhead = _bufferLen > 1 ? _buffer[1] : '\0';
        }

        int _bufferLen;

        private int GetNextBuffer()
        {
            _lookAheadProg = false;
            if (_crossingCurrentBuilder == null)
                _crossingCurrentBuilder = new StringBuilder();
            _crossingCurrentBuilder.Append(CurrentBufferSpan());  
            _spanOffset += _bufferLen;
            _bufferLen = _streamReader.ReadBlock(_buffer);
            return _bufferLen;
        }


        char _current;
#pragma warning disable IDE0052 // Remove unread private members
#pragma warning disable IDE0044 // Add readonly modifier
        char _lookAhead; // not used at the moment, keep for future changes.
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0052 // Remove unread private members
        private readonly Uri _sourceFile;
        private readonly char[] _buffer;
        private readonly StreamReader _streamReader;
        private StringBuilder? _crossingCurrentBuilder;

        /// <summary>
        /// position of <see cref="Current"/> char
        /// </summary>
        private long _position;
        /// <summary>
        /// Start of token under analysis
        /// </summary>
        private long _start;

        /// <summary>
        /// location of the beginning of the current <see cref="_buffer"/> in the <see cref="_sourceFile"/>
        /// </summary>
        private long _spanOffset;

        // private bool _crossingBuffers;

        // todo: rename once working
        public string FileName => _sourceFile.ToString();

        // because we can move the _currspan in the evaluation of Lookahead this could become slow...
        // therefore we cache current char, it's also used a lot in the lexer.
        public char Current => _current;

        public char Lookahead
        {
            get
            {
                var avail = _bufferLen - PositionInBuffer;
                if (avail <= 1)
                {
                    avail = GetNextBuffer();
                    _lookAheadProg = true;
                }
                return avail > 1 ? _buffer[PositionInBuffer + 1] : '\0';
            }
        }

        /// <summary>
        /// Allocates a new string in memory, copying data from the span already loaded.
        /// </summary>
        /// <returns>A string representation of the token from Start to Current position</returns>
        public string CurrentBuffer()
        {
            if (_crossingCurrentBuilder is null)
                return CurrentBufferSpan();
            else
                return _crossingCurrentBuilder.ToString() + CurrentBufferSpan();
        }


        /// we append the lenght from <see cref="_start" /> to the end of the span
        private string CurrentBufferSpan()
        {
            // because start could be in the buffer before current start needs to be capped at 0 minimum
             
            var start = Math.Max(StartInBuffer, 0);
            var l = PositionInBuffer - start;
            return new string(_buffer, start, l);
        }

        public long GetBufferStartIndex() => _start;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TextSpan GetTokenSpan()
        {
            var length = (int)(_position - _start);
            return new TextSpan(_start, length);
        }

        bool _lookAheadProg = false;

        public void ProgressChar()
        {
            _position++;
            // if the span is finished, then get another chunk from the file.
            //
            var avail = _bufferLen - PositionInBuffer;
            if (avail <= 0)
                avail = GetNextBuffer();
            else if (_lookAheadProg && _crossingCurrentBuilder != null)
            {
                // if we have a _crossingCurrentBuilder and it was created reading the lookahead then
                // we have to append current, this allows to read Lookahead and current multiple times with no risk
                // but if we did not have that possibility it could be simplified.
                //
                _crossingCurrentBuilder.Append(_current);
                _lookAheadProg = false;
            }
            _current = avail > 0 ? _buffer[PositionInBuffer] : '\0';
        }

        private int PositionInBuffer => (int)(_position - _spanOffset);
        private int StartInBuffer => (int)(_start - _spanOffset);

        public Uri Source { get => _sourceFile; }

        public void SetTokenStart()
        {
            _start = _position;
            if (_crossingCurrentBuilder != null)
            {
                // System.Diagnostics.Debug.WriteLine(StartInBuffer);
                if (StartInBuffer >= 0)
                {
                    _crossingCurrentBuilder = null;
                }
                else
                {
                    // is this ever the case?
                }
            }
        }

        // even "ref structs" can become disposable
        // https://tooslowexception.com/disposable-ref-structs-in-c-8-0/

        private bool _disposedValue;

        void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    // _buffer ok
                    // _memSpan ok
                    // _sourceFile ok
                    if (!(_streamReader is null))
                        _streamReader.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                // _buffer = null;
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Class1()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
        }

    }
}
