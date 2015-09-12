using System;
using System.Collections.Generic;

namespace LexLib
{
    public class Spec
    {
        /*
         * Member Variables
         */

        /* Lexical States. */
        public Dictionary<string, int> states; /* Hashtable taking state indices (Integer) 
			    to state name (String). */

        /* Regular Expression Macros. */
        public Dictionary<string, string> macros;   /* Hashtable taking macro name (String)
				   to corresponding char buffer that
				   holds macro definition. */

        /* NFA Machine. */
        public Nfa nfa_start;       /* Start state of NFA machine. */
        public List<Nfa> nfa_states;    /* List of states, with index
				 corresponding to label. */

        public List<Nfa>[] state_rules; /* An array of Lists of Nfa.
				   The ith Vector represents the lexical state
				   with index i.  The contents of the ith 
				   List are the indices of the NFA start
				   states that can be matched while in
				   the ith lexical state. */


        public int[] state_dtrans;

        /* DFA Machine. */
        public List<Dfa> dfa_states;    /* List of states, with index
				   corresponding to label. */
        public Dictionary<BitSet, Dfa> dfa_sets;    /* Hashtable taking set of NFA states
				   to corresponding DFA state, 
				   if the latter exists. */

        /* Accept States and Corresponding Anchors. */
        public List<Accept> accept_list;
        public int[] anchor_array;

        /* Transition Table. */
        public List<DTrans> dtrans_list;
        public int dtrans_ncols;
        public int[] row_map;
        public int[] col_map;

        /* Special pseudo-characters for beginning-of-line and end-of-file. */
        public const int NUM_PSEUDO = 2;
        public int BOL; // beginning-of-line
        public int EOF; // end-of-file

        /* NFA character class minimization map. */
        public int[] ccls_map;

        /* Regular expression token variables. */
        public int current_token;
        public char lexeme;
        public bool in_quote;
        public bool in_ccl;

        /* Verbose execution flag. */
        public bool verbose;

        /* directives flags. */
        public bool integer_type;
        public bool intwrap_type;
        public bool yyeof;
        public bool count_chars;
        public bool count_lines;
        public bool cup_compatible;
        public bool lex_public;
        public bool ignorecase;

        public String init_code;
        public String class_code;
        public String eof_code;
        public String eof_value_code;

        /* Class, function, type names. */
        public String class_name = "Yylex";
        public String implements_name;
        public String function_name = "yylex";
        public String type_name = "Yytoken";
        public String namespace_name = "YyNameSpace";

        /*
         * Constants
         */
        public const int NONE = 0;
        public const int START = 1;
        public const int END = 2;

        public Spec()
        {

            /* Initialize regular expression token variables. */
            current_token = Gen.EOS;
            lexeme = '\0';
            in_quote = false;
            in_ccl = false;

            /* Initialize hashtable for lexer states. */
            states = new Dictionary<string, int>();
            states["YYINITIAL"] = (int)states.Count;

            /* Initialize hashtable for lexical macros. */
            macros = new Dictionary<string, string>();

            /* Initialize variables for lexer options. */
            integer_type = false;
            intwrap_type = false;
            count_lines = false;
            count_chars = false;
            cup_compatible = false;
            lex_public = false;
            yyeof = false;
            ignorecase = false;

            /* Initialize variables for Lex runtime options. */
            verbose = true;

            nfa_start = null;
            nfa_states = new List<Nfa>();

            dfa_states = new List<Dfa>();

            dfa_sets = new Dictionary<BitSet, Dfa>();   // uses BitSet

            dtrans_list = new List<DTrans>();
            dtrans_ncols = Utility.MAX_SEVEN_BIT + 1;
            row_map = null;
            col_map = null;

            accept_list = null;
            anchor_array = null;

            init_code = null;
            class_code = null;
            eof_code = null;
            eof_value_code = null;

            state_dtrans = null;

            state_rules = null;
        }


        private int unmarked_dfa;

        public void InitUnmarkedDFA()
        {
            unmarked_dfa = 0;
        }

        /*
         * Function: GetNextUnmarkedDFA
         * Description: Returns next unmarked DFA state from spec
         */
        public Dfa GetNextUnmarkedDFA()
        {
            int size;
            Dfa dfa;

            size = dfa_states.Count;
            while (unmarked_dfa < size)
            {
                dfa = (Dfa)dfa_states[unmarked_dfa];

                if (!dfa.IsMarked())
                {
#if OLD_DUMP_DEBUG
      Console.Write("*");

      Console.WriteLine("---------------");
      Console.Write("working on DFA state " 
		    + unmarked_dfa
		    + " = NFA states: ");
      Nfa2Dfa.Print_Set(dfa.GetNFASet());
      Console.WriteLine("");
#endif
                    return dfa;
                }
                unmarked_dfa++;
            }
            return null;
        }
    }
}
