using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CsLexLib
{

    public delegate T AcceptMethod<T>();

    public abstract class CsLex<T>
    {
        #region constants
        public const int YY_BUFFER_SIZE = 512;
        public const int YY_F = -1;
        public const int YY_NO_STATE = -1;
        public const int YY_NOT_ACCEPT = 0;
        public const int YY_START = 1;
        public const int YY_END = 2;
        public const int YY_NO_ANCHOR = 4;
        public const int YY_BOL = 128;
        public const int YY_EOF = 129;
        public const int YY_E_INTERNAL = 0;
        public const int YY_E_MATCH = 1;
        #endregion

        public static readonly string[] yy_error_string = new string[]
          {
            "Error: Internal error.",
            "Error: Unmatched input."
          };

        public static readonly int STATE_YYINITIAL = 0;
  
        #region lex members
        public TextReader Reader { get; private set; }
        public int Index { get; private set; }
        public int Read { get; private set; }
        public int Start { get; private set; }
        public int End { get; private set; }
        public char[] Buffer { get; private set; }
        public int Char { get; private set; }
        public int Line { get; private set; }
        public bool IsAtBol { get; private set; }
        public int LexicalState { get; private set; }
        public bool WasLastCr { get; private set; }
        public string Text { get { return new string(Buffer, Start, End - Start); } }
        public int Length { get { return End - Start; } }
        #endregion

        #region constructors

        public CsLex(TextReader reader) : this()
        {
            if (reader == null)
                throw new ApplicationException("Error: Bad input stream initializer.");
            Reader = reader;
        }

        public CsLex(FileStream instream) : this()
        {
            if (instream == null)
                throw new ApplicationException("Error: Bad input stream initializer.");
            Reader = new StreamReader(instream);
        }

        protected virtual void UserInit() { }

        CsLex()
        {
            this.Buffer = new char[YY_BUFFER_SIZE];
            this.IsAtBol = true;
            this.LexicalState = STATE_YYINITIAL;

            ActionInit();
            UserInit();
        }

        #endregion

        #region helpers
        public void Begin(int state)
        {
            LexicalState = state;
        }

        public char Advance()
        {
            int next_read;
            int i;
            int j;

            if (Index < Read)
            {
                return Buffer[Index++];
            }

            if (0 != Start)
            {
                i = Start;
                j = 0;
                while (i < Read)
                {
                    Buffer[j] = Buffer[i];
                    i++;
                    j++;
                }

                End = End - Start;
                Start = 0;
                Read = j;
                Index = j;
                next_read = Reader.Read(Buffer, Read, Buffer.Length - Read);
                if (next_read <= 0)
                {
                    return (char)YY_EOF;
                }

                Read = Read + next_read;
            }

            while (Index >= Read)
            {
                if (Index >= Buffer.Length)
                {
                    Buffer = Double(Buffer);
                }

                next_read = Reader.Read(Buffer, Read,
                              Buffer.Length - Read);
                if (next_read <= 0)
                {
                    return (char)YY_EOF;
                }

                Read = Read + next_read;
            }

            return Buffer[Index++];
        }
        public void MoveEnd()
        {
            if (End > Start &&
                '\n' == Buffer[End - 1])
                End--;
            if (End > Start &&
                '\r' == Buffer[End - 1])
                End--;
        }

        public void MarkStart()
        {
            int i;
            for (i = Start; i < Index; i++)
            {
                if (Buffer[i] == '\n' && !WasLastCr)
                {
                    Line++;
                }

                if (Buffer[i] == '\r')
                {
                    Line++;
                    WasLastCr = true;
                }
                else
                {
                    WasLastCr = false;
                }
            }

            Char = Char + Index - Start;
            Start = Index;
        }

        public void MarkEnd()
        {
            End = Index;
        }

        public void ToMark()
        {
            Index = End;
            IsAtBol = (End > Start) &&
              (Buffer[End - 1] == '\r' ||
              Buffer[End - 1] == '\n');
        }

        public char[] Double(char[] buf)
        {
            int i;
            char[] newbuf;
            newbuf = new char[2 * buf.Length];
            for (i = 0; i < buf.Length; i++)
            {
                newbuf[i] = buf[i];
            }
            return newbuf;
        }

        public void Error(int code, bool fatal)
        {
            Console.Write(yy_error_string[code]);
            if (fatal)
            {
                throw new ApplicationException("Fatal Error.");
            }
        }
        #endregion

        #region abstract table defs
        protected abstract AcceptMethod<T>[] AcceptDispatch { get; }
        protected abstract int[] AcceptTable { get; }
        protected abstract int[] ColumnMap { get; }
        protected abstract int[] RowMap { get; }
        protected abstract int[,] TransitionTable { get; }
        protected abstract int[] StatesTable { get; }
        #endregion

        #region abstract methods
        protected abstract void ActionInit();

        #endregion

        protected virtual T OnEof()
        {
            return default(T);
        }

        #region driver
        public T Lex()
        {
            char yy_lookahead;
            int yy_anchor = YY_NO_ANCHOR;
            int yy_state = StatesTable[LexicalState];
            int yy_next_state = YY_NO_STATE;
            int yy_last_accept_state = YY_NO_STATE;
            bool yy_initial = true;
            int yy_this_accept;

            MarkStart();
            yy_this_accept = AcceptTable[yy_state];
            if (YY_NOT_ACCEPT != yy_this_accept)
            {
                yy_last_accept_state = yy_state;
                MarkEnd();
            }

            // begin_str

            while (true)
            {
                if (yy_initial && IsAtBol)
                {
                    yy_lookahead = (char)YY_BOL;
                }
                else
                {
                    yy_lookahead = Advance();
                }

                yy_next_state = TransitionTable[RowMap[yy_state], ColumnMap[yy_lookahead]];

                // state_str

                if (YY_EOF == yy_lookahead && yy_initial)
                {
                    return OnEof();
                }

                if (YY_F != yy_next_state)
                {
                    yy_state = yy_next_state;
                    yy_initial = false;
                    yy_this_accept = AcceptTable[yy_state];
                    if (YY_NOT_ACCEPT != yy_this_accept)
                    {
                        yy_last_accept_state = yy_state;
                        MarkEnd();
                    }
                }
                else
                {
                    if (YY_NO_STATE == yy_last_accept_state)
                    {
                        throw new ApplicationException("Lexical Error: Unmatched Input.");
                    }
                    else
                    {
                        yy_anchor = AcceptTable[yy_last_accept_state];
                        if (0 != (YY_END & yy_anchor))
                        {
                            MoveEnd();
                        }

                        ToMark();
                        if (yy_last_accept_state < 0 || yy_last_accept_state >= AcceptTable.Length)
                        { 
                                Error(YY_E_INTERNAL, false);
                        }

                        var m = AcceptDispatch[yy_last_accept_state];
                        if (m != null)
                        {
                            T tmp = m();
                            if (default(T) == null)
                            {
                                if (tmp != null)
                                    return tmp;
                            }
                            else
                                return tmp;
                        }

                        yy_initial = true;
                        yy_state = StatesTable[LexicalState];
                        yy_next_state = YY_NO_STATE;
                        yy_last_accept_state = YY_NO_STATE;
                        MarkStart();
                        yy_this_accept = AcceptTable[yy_state];
                        if (YY_NOT_ACCEPT != yy_this_accept)
                        {
                            yy_last_accept_state = yy_state;
                            MarkEnd();
                        }
                    }
                }
            }
        }
        #endregion

        public void IllegalChar()
        {
            StringBuilder sb = new StringBuilder("Illegal character: <");
            var s = this.Text;
            for (int i = 0; i < s.Length; i++)
                if (s[i] >= 32)
                    sb.Append(this.Text[i]);
                else
                {
                    sb.Append("^");
                    sb.Append(Convert.ToChar(s[i] + 'A' - 1));
                }
            sb.Append(">");
            Console.WriteLine(sb.ToString());
        }
    }
}
