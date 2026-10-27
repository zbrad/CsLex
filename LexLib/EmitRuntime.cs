namespace LexLib;

/*
 * %runtime: emit a lexer class that derives from CsLexLib.CsLex<T>. The base
 * class holds the input buffer and the driver loop, so the generated class
 * carries only the tables, the actions, and the names spec actions use
 * (yytext(), yyline, yybegin, ...). Behaviour must match the standalone form;
 * LexTests/RuntimeModeTests checks that.
 */
internal partial class Emit
{
    string RuntimeBase() => "CsLexLib.CsLex<" + getDriverType() + ">";

    private void RuntimeBody()
    {
        string token = getDriverType();
        string accept_method = "CsLexLib.AcceptMethod<" + token + ">";
        string access = spec.lex_public ? "public" : "internal";

        if (spec.integer_type || spec.yyeof)
            outstream.Write("\n    public const int YYEOF = -1;\n");

        /* User specified class code */
        if (null != spec.class_code)
            outstream.Write(spec.class_code);

        outstream.Write(string.Format(@"
    #region constructors
    {0} {1}(System.IO.TextReader reader) : base(reader) {{ }}
    {0} {1}(System.IO.FileStream instream) : base(instream) {{ }}
    #endregion

    {2}[] accept_dispatch;
", access, spec.class_name, accept_method));

        /* Called by the base constructor. */
        outstream.Write(Action_Methods_Init("protected override void ActionInit()", accept_method));
        outstream.Write(User_Init("protected override void UserInit()"));
        outstream.Write(Action_Methods_Body());

        States();

        outstream.Write(string.Format(@"
    #region names used by spec actions
    internal string yytext() => Text;
    int yylength() => Length;
    void yybegin(int state) => Begin(state);
    int yyline => Line;
    int yychar => Char;
    public {0} {1}() => Lex();
    #endregion
", token, spec.function_name));

        if (spec.eof_code != null)
            outstream.Write(Eof_Helper());
        outstream.Write(string.Format(@"
    protected override {0} OnEof()
    {{
{1}    }}
", token, EOF_Test()));

        Table();

        outstream.Write(string.Format(@"
    #region tables for the runtime
    protected override {0}[] AcceptDispatch => accept_dispatch;
    protected override int[] AcceptTable => yy_acpt;
    protected override int[] ColumnMap => yy_cmap;
    protected override int[] RowMap => yy_rmap;
    protected override int[,] TransitionTable => yy_nxt;
    protected override int[] StatesTable => yy_state_dtrans;
    #endregion
", accept_method));
    }
}
