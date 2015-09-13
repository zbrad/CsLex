using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsLexLib;
using System.IO;

namespace CsLexLibTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var s = new StreamReader(args[0]);
            MyLex yy = new MyLex(s);
            MyToken t;
            while ((t = yy.Lex()) != null)
                Console.WriteLine(t);
        }
    }

    class Actions
    {
        MyLex lex;
        public AcceptMethod<MyToken>[] Dispatch { get; private set; }


        public Actions(MyLex lex)
        {
            this.lex = lex;
            actionInit();
        }
        #region action methods

        void actionInit()
        {
            Dispatch = new AcceptMethod<MyToken>[]
    {

            null,

            null,

            new AcceptMethod<MyToken>(this.Accept_2),

            new AcceptMethod<MyToken>(this.Accept_3),

            new AcceptMethod<MyToken>(this.Accept_4),

            new AcceptMethod<MyToken>(this.Accept_5),

            new AcceptMethod<MyToken>(this.Accept_6),

            new AcceptMethod<MyToken>(this.Accept_7),

            new AcceptMethod<MyToken>(this.Accept_8),

            new AcceptMethod<MyToken>(this.Accept_9),

            new AcceptMethod<MyToken>(this.Accept_10),

            null,

            new AcceptMethod<MyToken>(this.Accept_12),

            new AcceptMethod<MyToken>(this.Accept_13),

            null,

            new AcceptMethod<MyToken>(this.Accept_15),

            null,

            new AcceptMethod<MyToken>(this.Accept_17),

            null,

            null,

    };

        }

        MyToken Accept_2()
        { /* this is newline */
            Console.WriteLine("Parsed Newline.");
            return null;
        }

        MyToken Accept_3()
        { /* this is whitespace */
            Console.WriteLine("Parsed Whitespace = [" + lex.Text + "]");
            return null;
        }

        MyToken Accept_4()
        { /* this is the '*' char */
            return (new MyToken(2, lex.Text, lex.Line, lex.Char, lex.Char + 1));
        }

        MyToken Accept_5()
        { /* this is the '/' char */
            return (new MyToken(1, lex.Text, lex.Line, lex.Char, lex.Char + 1));
        }

        MyToken Accept_6()
        {
            lex.IllegalChar();
            return null;
        }

        MyToken Accept_7()
        { /* comment begin (initial) */
            lex.Begin(MyLex.STATE_COMMENT);
            Console.WriteLine("Comment_Begin = [" + lex.Text + "]");
            return null;
        }

        MyToken Accept_8()
        {
            Console.WriteLine("Comment_Text = [" + lex.Text + "]");
            /* comment text here */
            return null;
        }

        MyToken Accept_9()
        {
            /* comment end */
            Console.WriteLine("Comment_End = [" + lex.Text + "]");
            lex.Begin(MyLex.STATE_YYINITIAL);
            return null;
        }

        MyToken Accept_10()
        {
            /* comment begin (non-initial) */
            return null;
        }

        MyToken Accept_12()
        { /* this is newline */
            Console.WriteLine("Parsed Newline.");
            return null;
        }

        MyToken Accept_13()
        {
            Console.WriteLine("Comment_Text = [" + lex.Text + "]");
            /* comment text here */
            return null;
        }

        MyToken Accept_15()
        {
            Console.WriteLine("Comment_Text = [" + lex.Text + "]");
            /* comment text here */
            return null;
        }

        MyToken Accept_17()
        {
            Console.WriteLine("Comment_Text = [" + lex.Text + "]");
            /* comment text here */
            return null;
        }

        #endregion


    }

    class MyLex : CsLex<MyToken>
    {
        public MyLex(FileStream f) : base(f) { }

        public MyLex(StreamReader sr) : base(sr) { }

        #region states

        public static readonly int STATE_COMMENT = 1;

        //TODO: see if we can change to use enum
        public enum States
        {
            YYINITIAL,
            COMMENT
        }

        #endregion

        #region tables

        static int[] yy_state_dtrans = new int[]
  {   0,
  8
  };

        static int[] yy_acpt = new int[]
        {
    /* 0 */   YY_NOT_ACCEPT,
    /* 1 */   YY_NO_ANCHOR,
    /* 2 */   YY_NO_ANCHOR,
    /* 3 */   YY_NO_ANCHOR,
    /* 4 */   YY_NO_ANCHOR,
    /* 5 */   YY_NO_ANCHOR,
    /* 6 */   YY_NO_ANCHOR,
    /* 7 */   YY_NO_ANCHOR,
    /* 8 */   YY_NO_ANCHOR,
    /* 9 */   YY_NO_ANCHOR,
    /* 10 */   YY_NO_ANCHOR,
    /* 11 */   YY_NOT_ACCEPT,
    /* 12 */   YY_NO_ANCHOR,
    /* 13 */   YY_NO_ANCHOR,
    /* 14 */   YY_NOT_ACCEPT,
    /* 15 */   YY_NO_ANCHOR,
    /* 16 */   YY_NOT_ACCEPT,
    /* 17 */   YY_NO_ANCHOR,
    /* 18 */   YY_NOT_ACCEPT,
    /* 19 */   YY_NOT_ACCEPT
        };

        static int[] yy_cmap = new int[]
        {
    /* 0-7 */ 6, 6, 6, 6, 6, 6, 6, 6, 
    /* 8-15 */ 3, 3, 2, 6, 6, 1, 6, 6, 
    /* 16-23 */ 6, 6, 6, 6, 6, 6, 6, 6, 
    /* 24-31 */ 6, 6, 6, 6, 6, 6, 6, 6, 
    /* 32-39 */ 3, 6, 6, 6, 7, 6, 6, 6, 
    /* 40-47 */ 6, 6, 4, 6, 6, 6, 6, 5, 
    /* 48-55 */ 6, 6, 6, 6, 6, 6, 6, 6, 
    /* 56-63 */ 6, 6, 6, 6, 6, 6, 6, 6, 
    /* 64-71 */ 6, 6, 6, 6, 6, 6, 6, 6, 
    /* 72-79 */ 6, 6, 6, 6, 6, 6, 6, 6, 
    /* 80-87 */ 6, 6, 6, 6, 6, 6, 6, 6, 
    /* 88-95 */ 6, 6, 6, 6, 6, 6, 6, 6, 
    /* 96-103 */ 6, 6, 6, 6, 6, 6, 6, 6, 
    /* 104-111 */ 6, 6, 6, 6, 6, 6, 6, 6, 
    /* 112-119 */ 6, 6, 6, 6, 6, 6, 6, 6, 
    /* 120-127 */ 6, 6, 6, 6, 6, 6, 6, 6, 
    /* 128-135 */ 0, 0
        };

        static int[] yy_rmap = new int[]
        {
    /* 0-7 */ 0, 1, 2, 2, 1, 3, 1, 1, 
    /* 8-15 */ 4, 1, 1, 5, 1, 6, 7, 8, 
    /* 16-23 */ 9, 10, 11, 12
        };

        static int[,] yy_nxt = new int[,]
        {
        {
        /* 0-7 */ 1, 11, 2, 3, 4, 5, 6, 6
        },
        {
        /* 0-7 */ -1, -1, -1, -1, -1, -1, -1, -1
        },
        {
        /* 0-7 */ -1, -1, 3, 3, -1, -1, -1, -1
        },
        {
        /* 0-7 */ -1, -1, -1, -1, 7, -1, -1, -1
        },
        {
        /* 0-7 */ 1, 13, 13, 13, 14, 16, 13, -1
        },
        {
        /* 0-7 */ -1, -1, 12, -1, -1, -1, -1, -1
        },
        {
        /* 0-7 */ -1, 13, 13, 13, 18, 19, 13, -1
        },
        {
        /* 0-7 */ -1, 13, 13, 13, 15, 9, 13, -1
        },
        {
        /* 0-7 */ -1, 13, 13, 13, 15, 19, 13, -1
        },
        {
        /* 0-7 */ -1, 13, 13, 13, 10, 17, 13, -1
        },
        {
        /* 0-7 */ -1, 13, 13, 13, 18, 17, 13, -1
        },
        {
        /* 0-7 */ -1, 13, 13, 13, 15, -1, 13, -1
        },
        {
        /* 0-7 */ -1, 13, 13, 13, -1, 17, 13, -1
        }
        };

        #endregion


        protected override AcceptMethod<MyToken>[] AcceptDispatch { get { return actions.Dispatch; } }

        protected override int[] AcceptTable { get { return yy_acpt; } }


        protected override int[] ColumnMap { get { return yy_cmap; } }

        protected override int[] RowMap { get { return yy_rmap; } }

        protected override int[] StatesTable { get { return yy_state_dtrans; } }


        protected override int[,] TransitionTable { get { return yy_nxt; } }

        Actions actions;
        protected override void ActionInit()
        {
            actions =  new Actions(this);
        }


    }

    class MyToken
    {
        public int Index { get; set; }
        public string Text { get; set; }
        public int Line { get; set; }
        public int Begin { get; set; }
        public int End { get; set; }

        internal MyToken(int index, string text, int line, int begin, int end)
        {
            Index = index;
            Text = text;
            Line = line;
            Begin = begin;
            End = end;
        }
        public override String ToString()
        {
            return "Token #" + Index + ": " + Text + " (line " + Line + ")";
        }
    }
}
