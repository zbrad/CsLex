namespace LexLib
{
    /*
     * Class: Emit
     */
    using System;
    using System.Text;
    using System.IO;
    using System.Collections;

    public class Emit
    {
        /*
         * Member Variables
         */
        private Spec spec;
        private StreamWriter outstream;

        /*
         * Constants: Anchor Types
         */
        private const int START = 1;
        private const int END = 2;
        private const int NONE = 4;

        /*
         * Constants
         */
        private const bool EDBG = true;
        private const bool NOT_EDBG = false;

        /*
         * Function: Emit
         * Description: Constructor.
         */
        public Emit()
        {
            reset();
        }

        /*
         * Function: reset
         * Description: Clears member variables.
         */
        private void reset()
        {
            spec = null;
            outstream = null;
        }

        /*
         * Function: set
         * Description: Initializes member variables.
         */
        private void set(Spec s, StreamWriter o)
        {
#if DEBUG
            Utility.assert(null != s);
            Utility.assert(null != o);
#endif
            spec = s;
            outstream = o;
        }

        /*
         * Function: print_details
         * Description: Debugging output.
         */
        private void print_details()
        {
            int i;
            int j;
            int next;
            int state;
            DTrans dtrans;
            Accept accept;
            bool tr;

            System.Console.WriteLine("---------------------- Transition Table ----------------------");
            for (i = 0; i < spec.row_map.Length; ++i)
            {
                System.Console.Write("State " + i);

                accept = (Accept)spec.accept_list[i];
                if (null == accept)
                {
                    System.Console.WriteLine(" [nonaccepting]");
                }
                else
                {
                    System.Console.WriteLine(" [accepting, line "
                          + accept.line_number
                          + " <"
                          + accept.action
                          + ">]");
                }
                dtrans = (DTrans)spec.dtrans_list[spec.row_map[i]];

                tr = false;
                state = dtrans.GetDTrans(spec.col_map[0]);
                if (DTrans.F != state)
                {
                    tr = true;
                    System.Console.Write("\tgoto " + state.ToString() + " on [");
                }
                for (j = 1; j < spec.dtrans_ncols; j++)
                {
                    next = dtrans.GetDTrans(spec.col_map[j]);
                    if (state == next)
                    {
                        if (DTrans.F != state)
                        {
                            System.Console.Write((char)j);
                        }
                    }
                    else
                    {
                        state = next;
                        if (tr)
                        {
                            System.Console.WriteLine("]");
                            tr = false;
                        }
                        if (DTrans.F != state)
                        {
                            tr = true;
                            System.Console.Write("\tgoto " + state.ToString() +
                                  " on [" + Char.ToString((char)j));
                        }
                    }
                }
                if (tr)
                {
                    System.Console.WriteLine("]");
                }
            }
            System.Console.WriteLine("---------------------- Transition Table ----------------------");
        }

        /*
         * Function: Write
         * Description: High-level access function to module.
         */
        public void Write(Spec spec, StreamWriter o)
        {
            set(spec, o);

#if DEBUG
            Utility.assert(null != spec);
            Utility.assert(null != o);
#endif

#if OLD_DEBUG
  print_details();
#endif

            Header();
            Construct();
            States();
            Helpers();
            Driver();
            Footer();

            outstream.Flush();
            reset();
        }

        /*
         * Function: construct
         * Description: Emits constructor, member variables,
         * and constants.
         */
        private void Construct()
        {
#if DEBUG
            Utility.assert(null != spec);
            Utility.assert(null != outstream);
#endif

            /* Constants */
            outstream.Write(@"
#region constants
    {0}
    const int YY_BUFFER_SIZE = 512;
    const int YY_F = -1;
    const int YY_NO_STATE = -1;
    const int YY_NOT_ACCEPT = 0;
    const int YY_START = 1;
    const int YY_END = 2;
    const int YY_NO_ANCHOR = 4;
    const int YY_BOL = {1};
    const int YY_EOF = {2};
#endregion
",
        (spec.integer_type || spec.yyeof) ? "public const int YYEOF = -1;" : "",
        spec.BOL, spec.EOF);

            /* type declarations */
            outstream.WriteLine(@"
    delegate {0} AcceptMethod();
    AcceptMethod[] accept_dispatch;
",
                spec.type_name);

            /* User specified class code */
            if (null != spec.class_code)
            {
                outstream.Write(spec.class_code);
            }

            /* Member Variables */
            outstream.Write(
                @"
    #region private members
    TextReader yy_reader;
    int yy_buffer_index;
    int yy_buffer_read;
    int yy_buffer_start;
    int yy_buffer_end;
    char[] yy_buffer = new char[YY_BUFFER_SIZE];
    int yychar;
    int yyline;
    bool yy_at_bol = true;
    int yy_lexical_state = YYINITIAL;
    #endregion
");

            /* Function: first constructor (Reader) */
            string spec_access = "internal ";
            if (spec.lex_public)
                spec_access = "public ";

            outstream.Write(@"
    #region constructors

    {0} {1}(TextReader reader) : this()
    {{
        if (reader == null)
            throw new ApplicationException(""Error: Bad input stream initializer."");
        yy_reader = reader;
    }}

    {0} {1}(FileStream instream) : this()
    {{
        if (instream == null)
            throw new ApplicationException(""Error: Bad input stream initializer."");
        yy_reader = new StreamReader(instream);
    }}

    {1}()
    {{
        actionInit();
        userInit();
    }}

    #endregion
",
                spec_access, spec.class_name);

            // action init
            var actioninit = Action_Methods_Init();
            outstream.Write(actioninit);

            // User specified constructor init code
            var userinit = User_Init();
            outstream.Write(userinit); 

            var actions = Action_Methods_Body();
            outstream.Write(actions);
        }

        /*
         * Function: states
         * Description: Emits constants that serve as lexical states,
         * including YYINITIAL.
         */
        private void States()
        {
            foreach (string state in spec.states.Keys)
            {
#if DEBUG
                Utility.assert(null != state);
#endif
                outstream.Write(
                  "private const int " + state + " = " + spec.states[state] + ";\n");
            }

            outstream.Write("private static int[] yy_state_dtrans = new int[] \n"
                    + "  { ");
            for (int index = 0; index < spec.state_dtrans.Length; ++index)
            {
                outstream.Write("  " + spec.state_dtrans[index]);
                if (index < spec.state_dtrans.Length - 1)
                    outstream.Write(",\n");
                else
                    outstream.Write("\n");
            }
            outstream.Write("  };\n");
        }

        /*
         * Function: Helpers
         * Description: Emits helper functions, particularly 
         * error handling and input buffering.
         */
        private void Helpers()
        {
#if DEBUG
            Utility.assert(null != spec);
            Utility.assert(null != outstream);
#endif
            outstream.WriteLine("#region helpers");
            /* Function: yy_do_eof */
            if (spec.eof_code != null)
            {
                outstream.Write("private bool yy_eof_done = false;\n"
                  + "private void yy_do_eof ()\n"
                  + "  {\n"
                  + "  if (!yy_eof_done)\n"
                  + "    {\n"
                  + "    " + spec.eof_code + "\n"
                  + "    }\n"
                  + "  yy_eof_done = true;\n"
                  + "  }\n\n");
            }

            /* Function: yybegin */
            outstream.Write(
              "private void yybegin (int state)\n"
              + "  {\n"
              + "  yy_lexical_state = state;\n"
              + "  }\n\n");

            /* Function: yy_advance */
            outstream.Write(
              "private char yy_advance ()\n"
              + "  {\n"
              + "  int next_read;\n"
              + "  int i;\n"
              + "  int j;\n"
              + "\n"
              + "  if (yy_buffer_index < yy_buffer_read)\n"
              + "    {\n"
              + "    return yy_buffer[yy_buffer_index++];\n"
              + "    }\n"
              + "\n"
              + "  if (0 != yy_buffer_start)\n"
              + "    {\n"
              + "    i = yy_buffer_start;\n"
              + "    j = 0;\n"
              + "    while (i < yy_buffer_read)\n"
              + "      {\n"
              + "      yy_buffer[j] = yy_buffer[i];\n"
              + "      i++;\n"
              + "      j++;\n"
              + "      }\n"
              + "    yy_buffer_end = yy_buffer_end - yy_buffer_start;\n"
              + "    yy_buffer_start = 0;\n"
              + "    yy_buffer_read = j;\n"
              + "    yy_buffer_index = j;\n"
              + "    next_read = yy_reader.Read(yy_buffer,yy_buffer_read,\n"
              + "                  yy_buffer.Length - yy_buffer_read);\n"
              //    + "    if (-1 == next_read)\n"
              + "    if (next_read <= 0)\n"
              + "      {\n"
              + "      return (char) YY_EOF;\n"
              + "      }\n"
              + "    yy_buffer_read = yy_buffer_read + next_read;\n"
              + "    }\n"
              + "  while (yy_buffer_index >= yy_buffer_read)\n"
              + "    {\n"
              + "    if (yy_buffer_index >= yy_buffer.Length)\n"
              + "      {\n"
              + "      yy_buffer = yy_double(yy_buffer);\n"
              + "      }\n"
              + "    next_read = yy_reader.Read(yy_buffer,yy_buffer_read,\n"
              + "                  yy_buffer.Length - yy_buffer_read);\n"
              //    + "    if (-1 == next_read)\n"
              + "    if (next_read <= 0)\n"
              + "      {\n"
              + "      return (char) YY_EOF;\n"
              + "      }\n"
              + "    yy_buffer_read = yy_buffer_read + next_read;\n"
              + "    }\n"
              + "  return yy_buffer[yy_buffer_index++];\n"
              + "  }\n");

            /* Function: yy_move_end */
            outstream.Write(
                "private void yy_move_end ()\n"
              + "  {\n"
              + "  if (yy_buffer_end > yy_buffer_start && \n"
              + "      '\\n' == yy_buffer[yy_buffer_end-1])\n"
              + "    yy_buffer_end--;\n"
              + "  if (yy_buffer_end > yy_buffer_start &&\n"
              + "      '\\r' == yy_buffer[yy_buffer_end-1])\n"
              + "    yy_buffer_end--;\n"
              + "  }\n"
              );

            /* Function: yy_mark_start */
            outstream.Write("private bool yy_last_was_cr=false;\n"
              + "private void yy_mark_start ()\n"
              + "  {\n");
            if (spec.count_lines)
            {
                outstream.Write(
                  "  int i;\n"
                  + "  for (i = yy_buffer_start; i < yy_buffer_index; i++)\n"
                  + "    {\n"
                  + "    if (yy_buffer[i] == '\\n' && !yy_last_was_cr)\n"
                  + "      {\n"
                  + "      yyline++;\n"
                  + "      }\n"
                  + "    if (yy_buffer[i] == '\\r')\n"
                  + "      {\n"
                  + "      yyline++;\n"
                  + "      yy_last_was_cr=true;\n"
                  + "      }\n"
                  + "    else\n"
                  + "      {\n"
                  + "      yy_last_was_cr=false;\n"
                  + "      }\n"
                  + "    }\n"
                  );
            }
            if (spec.count_chars)
            {
                outstream.Write(
                  "  yychar = yychar + yy_buffer_index - yy_buffer_start;\n");
            }
            outstream.Write(
              "  yy_buffer_start = yy_buffer_index;\n"
              + "  }\n");

            /* Function: yy_mark_end */
            outstream.Write(
              "private void yy_mark_end ()\n"
              + "  {\n"
              + "  yy_buffer_end = yy_buffer_index;\n"
              + "  }\n");

            /* Function: yy_to_mark */
            outstream.Write(
              "private void yy_to_mark ()\n"
              + "  {\n"
              + "  yy_buffer_index = yy_buffer_end;\n"
              + "  yy_at_bol = (yy_buffer_end > yy_buffer_start) &&\n"
              + "    (yy_buffer[yy_buffer_end-1] == '\\r' ||\n"
              + "    yy_buffer[yy_buffer_end-1] == '\\n');\n"
              + "  }\n");

            /* Function: yytext */
            outstream.Write(
              "internal string yytext()\n"
              + "  {\n"
              + "  return (new string(yy_buffer,\n"
              + "                yy_buffer_start,\n"
              + "                yy_buffer_end - yy_buffer_start)\n"
              + "         );\n"
              + "  }\n");

            /* Function: yylength */
            outstream.Write(
              "private int yylength ()\n"
              + "  {\n"
              + "  return yy_buffer_end - yy_buffer_start;\n"
              + "  }\n");

            /* Function: yy_double */
            outstream.Write(
              "private char[] yy_double (char[] buf)\n"
              + "  {\n"
              + "  int i;\n"
              + "  char[] newbuf;\n"
              + "  newbuf = new char[2*buf.Length];\n"
              + "  for (i = 0; i < buf.Length; i++)\n"
              + "    {\n"
              + "    newbuf[i] = buf[i];\n"
              + "    }\n"
              + "  return newbuf;\n"
              + "  }\n");

            /* Function: yy_error */
            outstream.Write(
              "private const int YY_E_INTERNAL = 0;\n"
              + "private const int YY_E_MATCH = 1;\n"
              + "private static string[] yy_error_string = new string[]\n"
              + "  {\n"
              + "  \"Error: Internal error.\\n\",\n"
              + "  \"Error: Unmatched input.\\n\"\n"
              + "  };\n");

            outstream.Write(
              "private void yy_error (int code,bool fatal)\n"
              + "  {\n"
              + "  System.Console.Write(yy_error_string[code]);\n"
              + "  if (fatal)\n"
              + "    {\n"
              + "    throw new System.ApplicationException(\"Fatal Error.\\n\");\n"
              + "    }\n"
              + "  }\n");

            //  /* Function: yy_next */
            //  outstream.Write("\tprivate int yy_next (int current,char lookahead) {\n"
            //    + "  return yy_nxt[yy_rmap[current],yy_cmap[lookahead]];\n"
            //    + "\t}\n");

            //  /* Function: yy_accept */
            //  outstream.Write("\tprivate int yy_accept (int current) {\n");
            //    + "  return yy_acpt[current];\n"
            //    + "\t}\n");

            outstream.WriteLine("#endregion");
        }

        /*
         * Function: Header
         * Description: Emits class header.
         */
        private void Header()
        {
#if DEBUG
            Utility.assert(null != spec);
            Utility.assert(null != outstream);
#endif
            outstream.Write("namespace " + spec.namespace_name + "\n{\n");
            outstream.Write(spec.usercode);
            outstream.Write("/* test */\n");

            outstream.Write("\n\n");
            string spec_access = "internal ";
            if (spec.lex_public)
                spec_access = "public ";

            outstream.Write(spec_access + "class " + spec.class_name);
            if (spec.implements_name != null)
            {
                outstream.Write(" : ");
                outstream.Write(spec.implements_name);
            }
            outstream.Write("\n{\n");
        }

        StringBuilder Accept_table()
        {
            int size = spec.accept_list.Count;
            int lastelem = size - 1;
            StringBuilder sb = new StringBuilder(Properties.Settings.Default.MaxStr);

            sb.Append(@"
    static int[] yy_acpt = new int[]
    {");
            for (int elem = 0; elem < size; elem++)
            {
                sb.AppendFormat(@"
    /* {0} */ {1}{2}", elem, getAccept(elem), elem < lastelem ? "," : "");
            }

            sb.Append(@"
    };
");
            return sb;
        }

        string getAccept(int elem)
        {
            string s = "  YY_NOT_ACCEPT"; // default to NOT
            Accept accept = (Accept)spec.accept_list[elem];
            if (accept != null)
            {
                bool is_start = ((spec.anchor_array[elem] & Spec.START) != 0);
                bool is_end = ((spec.anchor_array[elem] & Spec.END) != 0);

                if (is_start && is_end)
                    s = "  YY_START | YY_END";
                else if (is_start)
                    s = "  YY_START";
                else if (is_end)
                    s = "  YY_END";
                else
                    s = "  YY_NO_ANCHOR";
            }

            return s;
        }

        StringBuilder CMap_table()
        {
            //  int size = spec.col_map.Length;
            int size = spec.ccls_map.Length;
            int lastelem = size - 1;
            StringBuilder sb = new StringBuilder();

            sb.Append(@"
    static int[] yy_cmap = new int[]
    {");
            for (int i = 0; i < size; i++)
            {
                if (i%8 == 0)
                    sb.AppendFormat(@"
    /* {0}-{1} */ ",
                i, i + 7);
                sb.Append(spec.col_map[spec.ccls_map[i]]);
                if (i < lastelem)
                    sb.Append(", ");
            }

            sb.Append(@"
    };
");
            return sb;
        }

        StringBuilder RMap_table()
        {
            int size = spec.row_map.Length;
            int lastelem = size - 1;

            StringBuilder sb = new StringBuilder();
            sb.Append(@"
    static int[] yy_rmap = new int[]
    {");

            for (int i = 0; i < size; ++i)
            {
                if (i % 8 == 0)
                    sb.AppendFormat(@"
    /* {0}-{1} */ ", i, i + 7);
                sb.Append(spec.row_map[i]);
                if (i < lastelem)
                    sb.Append(", ");
            }

            sb.Append(@"
    };
");
            return sb;
        }

        StringBuilder YYNXT_table()
        {
            int size = spec.dtrans_list.Count;
            int lastelem = size - 1;
            int lastcol = spec.dtrans_ncols - 1;
            
            StringBuilder sb = new StringBuilder();
            sb.Append(@"
    static int[,] yy_nxt = new int[,]
    {");

            for (int elem = 0; elem < size; elem++)
            {
                DTrans cdt_list = (DTrans)spec.dtrans_list[elem];
#if DEBUG
                Utility.assert(spec.dtrans_ncols <= cdt_list.GetDTransLength());
#endif
                sb.Append(@"
        {");
                for (int i = 0; i < spec.dtrans_ncols; i++)
                {
                    if (i % 8 == 0)
                        sb.AppendFormat(@"
        /* {0}-{1} */ ", i, i + 7);
                    sb.Append(cdt_list.GetDTrans(i));
                    if (i < lastcol)
                        sb.Append(", ");
                }

                sb.AppendFormat(@"
        }}{0}", elem < lastelem ? "," : "");
            }

            sb.Append(@"
    };
");

            return sb;
        }

        /*
         * Function: Table
         * Description: Emits transition table.
         */
        private void Table()
        {
#if DEBUG
            Utility.assert(null != spec);
            Utility.assert(null != outstream);
#endif

            StringBuilder sb = new StringBuilder();

            sb.Append(@"
    #region tables
");
            sb.Append(Accept_table());
            sb.Append(CMap_table());
            sb.Append(RMap_table());
            sb.Append(YYNXT_table());

            sb.Append(@"
    #endregion
");
            outstream.Write(sb);
        }

        string EOF_Test()
        {
            StringBuilder sb = new StringBuilder();
            if (spec.eof_code != null)
                sb.AppendLine("        yy_do_eof();");

            if (spec.integer_type)
                sb.AppendLine("        return YYEOF;");
            else if (spec.eof_value_code != null)
                sb.Append(spec.eof_value_code);
            else
                sb.AppendLine("        return null;");
            return sb.ToString();
        }

        /*
         * Function: Driver
         * Description: 
         */
        void Driver()
        {
#if DEBUG
            Utility.assert(null != spec);
            Utility.assert(null != outstream);
#endif
            Table();

            //string begin_str = "";
            //string state_str = "";
#if NOT_EDBG
    begin_str = "  System.Console.WriteLine(\"Begin\");\n";
    state_str =
      "  System.Console.WriteLine(\"\\n\\nCurrent state: \" + yy_state);\n"
    + "  System.Console.Write(\"Lookahead input: "
    + "   (\" + yy_lookahead + \")\");\n"
    + "  if (yy_lookahead < 32)\n"
    + "    System.Console.WriteLine("
    + "   \"'^\" + System.Convert.ToChar(yy_lookahead+'A'-1).ToString() + \"'\");\n"
    + "  else if (yy_lookahead > 127)\n"
    + "    System.Console.WriteLine("
    + "   \"'^\" + yy_lookahead + \"'\");\n"
    + "  else\n"
    + "    System.Console.WriteLine("
    + "   \"'\" + yy_lookahead.ToString() + \"'\");\n"
    + "  System.Console.WriteLine(\"State = \"+ yy_state);\n"
    + "  System.Console.WriteLine(\"Accepting status = \"+ yy_this_accept);\n"
      + "  System.Console.WriteLine(\"Last accepting state = \"+ yy_last_accept_state);\n"
      + "  System.Console.WriteLine(\"Next state = \"+ yy_next_state);\n"
;
#endif

            var sb = new StringBuilder();
            sb.AppendFormat(@"
    #region driver
    public {0} {1}()
    {{
    char yy_lookahead;
    int yy_anchor = YY_NO_ANCHOR;
    int yy_state = yy_state_dtrans[yy_lexical_state];
    int yy_next_state = YY_NO_STATE;
    int yy_last_accept_state = YY_NO_STATE;
    bool yy_initial = true;
    int yy_this_accept;

    yy_mark_start();
    yy_this_accept = yy_acpt[yy_state];
    if (YY_NOT_ACCEPT != yy_this_accept)
    {{
        yy_last_accept_state = yy_state;
        yy_mark_end();
    }}

    // begin_str

    while (true)
    {{
        if (yy_initial && yy_at_bol)
        {{
            yy_lookahead = (char)YY_BOL;
        }}
        else
        {{
            yy_lookahead = yy_advance();
        }}

        yy_next_state = yy_nxt[yy_rmap[yy_state], yy_cmap[yy_lookahead]];

        // state_str

        if (YY_EOF == yy_lookahead && yy_initial)
        {{
            // EOF_Test()
            {2} 
        }}

        if (YY_F != yy_next_state)
        {{
            yy_state = yy_next_state;
            yy_initial = false;
            yy_this_accept = yy_acpt[yy_state];
            if (YY_NOT_ACCEPT != yy_this_accept)
            {{
                yy_last_accept_state = yy_state;
                yy_mark_end();
            }}
        }}
        else
        {{
            if (YY_NO_STATE == yy_last_accept_state)
            {{
                throw new ApplicationException(""Lexical Error: Unmatched Input."");
            }}
            else
            {{
                yy_anchor = yy_acpt[yy_last_accept_state];
                if (0 != (YY_END & yy_anchor))
                {{
                    yy_move_end();
                }}

                yy_to_mark();
                if (yy_last_accept_state< 0)
                {{
                    if (yy_last_accept_state< {3}) // spec.accept_list.Count
                        yy_error(YY_E_INTERNAL, false);
                }}
                else
                {{
                    AcceptMethod m = accept_dispatch[yy_last_accept_state];
                    if (m != null)
                    {{
                        {4} tmp = m(); // spec.type_name
                        if (tmp != null)
                            return tmp;
                    }}
                }}

                yy_initial = true;
                yy_state = yy_state_dtrans[yy_lexical_state];
                yy_next_state = YY_NO_STATE;
                yy_last_accept_state = YY_NO_STATE;
                yy_mark_start();
                yy_this_accept = yy_acpt[yy_state];
                if (YY_NOT_ACCEPT != yy_this_accept)
                {{
                    yy_last_accept_state = yy_state;
                    yy_mark_end();
                }}
            }}
        }}
    }}
    }}
    #endregion
", getDriverType(), spec.function_name, EOF_Test(), spec.accept_list.Count, spec.type_name);

            outstream.Write(sb);
        }

        string getDriverType()
        {
            string type = spec.type_name;
            if (spec.integer_type)
                type = "int";
            else if (spec.intwrap_type)
                type = "Int32";
            return type;
        }

        /*
         * Function: Actions
         * Description:     
         */
        private string Actions()
        {
            int size = spec.accept_list.Count;
            int bogus_index = -2;
            Accept accept;
            StringBuilder sb = new StringBuilder(Properties.Settings.Default.MaxStr);

#if DEBUG
            Utility.assert(spec.accept_list.Count == spec.anchor_array.Length);
#endif
            string prefix = "";

            for (int elem = 0; elem < size; elem++)
            {
                accept = (Accept)spec.accept_list[elem];
                if (accept != null)
                {
                    sb.Append("        " + prefix + "if (yy_last_accept_state == ");
                    sb.Append(elem);
                    sb.Append(")\n");
                    sb.Append("          { // begin accept action #");
                    sb.Append(elem);
                    sb.Append("\n");
                    sb.Append(accept.action);
                    sb.Append("\n");
                    sb.Append("          } // end accept action #");
                    sb.Append(elem);
                    sb.Append("\n");
                    sb.Append("          else if (yy_last_accept_state == ");
                    sb.Append(bogus_index);
                    sb.Append(")\n");
                    sb.Append("            { /* no work */ }\n");
                    prefix = "else ";
                    bogus_index--;
                }
            }
            return sb.ToString();
        }

        string User_Init()
        {
            var s = string.Format(@"
    #region user init
    void userInit()
    {{
    {0}
    }}
    #endregion
", spec.init_code == null ? "// no user init" : spec.init_code);
            return s;
        }



        private StringBuilder Action_Methods_Init()
        {
            int size = spec.accept_list.Count;
            Accept accept;
            StringBuilder tbl = new StringBuilder();
            tbl.Append(@"
    #region action init
    void actionInit()
    {
");

#if DEBUG
            Utility.assert(spec.accept_list.Count == spec.anchor_array.Length);
#endif
            tbl.Append(@"
    accept_dispatch = new AcceptMethod[]
        {
");
            for (int elem = 0; elem < size; elem++)
            {
                accept = (Accept)spec.accept_list[elem];
                if (accept != null && accept.action != null)
                {
                    tbl.AppendFormat(@"
            new AcceptMethod(this.Accept_{0}),
", elem);
                }
                else
                    tbl.Append(@"
            null,
");
            }

            tbl.Append(@"
        };
    }
    #endregion
");
            return tbl;
        }

        /*
         * Function: Action_Methods_Body
         */
        private StringBuilder Action_Methods_Body()
        {
            int size = spec.accept_list.Count;
            Accept accept;
            StringBuilder sb = new StringBuilder(Properties.Settings.Default.MaxStr);
            sb.Append(@"
    #region action methods
");
#if DEBUG
            Utility.assert(spec.accept_list.Count == spec.anchor_array.Length);
#endif
            for (int elem = 0; elem < size; elem++)
            {
                accept = (Accept)spec.accept_list[elem];
                if (accept != null && accept.action != null)
                {
                    sb.AppendFormat(@"
    {1} Accept_{0}()
    {2}
",
                    elem, spec.type_name, accept.action);
                }
            }

            sb.Append(@"
    #endregion
");
            return sb;
        }


        /*
         * Function: Footer
         * Description:     
         */
        private void Footer()
        {
#if DEBUG
            Utility.assert(null != spec);
            Utility.assert(null != outstream);
#endif
            outstream.Write(@"
    }
}
");
        }
    }
}
