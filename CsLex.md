<center>
<h1>CsLex</h1>
*A lexical analyzer generator for C#*

<a href="mailto:brad_merrill@hotmail.com">**Brad Merrill**</a>

Version 1.0, 20-Sep-1999
</center>

## Introduction

A lexical analyzer breaks an input stream of characters into tokens.
Writing lexical analyzers by hand can be a tedious process, so software tools have been developed to ease this task.

Perhaps the best known such utility is the original C-based Lex.
Lex is a lexical analyzer generator for the UNIX operating system,
targeted to the C programming language. 

Lex takes a specially-formatted specification file containing the details of a 
lexical analyzer. This tool then creates a C source file for the associated 
table-driven lexer. 

The CsLex utility is based upon the Lex lexical analyzer generator model.

CsLex takes a specification file similar to that accepted by Lex,
then creates a C# source file for the corresponding lexical analyzer.

CsLex is loosely based on the JLex tool, which was based on the Lex tool.
This was a significant rewrite, so consequently any
errors are solely the responsibility of the most recent author.
See the credits section for more info.

## CsLex Specifications

A CsLex input file is organized into three sections, separated by 
double-percent directives (`%%`). A proper CsLex specification has the 
following format.


	user code
	%%
	CsLex directives
	%%
	regular expression rules

The `%%` directives distinguish sections of the input file and
must be placed at the beginning of 
their line. The remainder of the line containing the `%%` directives may be 
discarded and should not be used to house additional declarations or code. 

The user code section - the first section of the specification file - is 
copied directly into the resulting output file. This area of the specification 
provides space for the implementation of utility classes or return types. 

The CsLex directives section is the second part of the input file. Here, 
macros definitions are given and state names are declared. 

The third section contains the rules of lexical analysis, each of which 
consists of three parts: an optional state list, a regular expression, and an 
action. 

## User Code

User code precedes the first double-percent directive (`%%`). This code is 
copied verbatim into the lexical analyzer source file that CsLex
outputs, at the top of the file. Therefore, if the lexer source
file needs to begin with a package declaration or with the
importation of an external class, the user code section should
begin with the corresponding declaration. This declaration will
then be copied onto the top of the generated source file.

## CsLex Directives

The CsLex directive section begins after the first `%%` and continues until 
the second `%%` delimiter. Each CsLex directive should be
contained on a single line and should begin that line. 

### Reserved names

To avoid scoping conflicts, names beginning with `yy` are
normally reserved for lexical analyzer internal functions and
variables.

### Internal Code to Lexical Analyzer Class

The `%{...%}` directive allows the user to write C# code to be copied 
into the lexical analyzer class. This directive is used as follows.

	%{ 
		... code ...
	%}

To be properly recognized, the `%{` and `%}` should
each be situated at the beginning of a line. The specified C#
code will be then copied into the lexical analyzer class created by CsLex.

	class Yylex 
	{
		... code ... 
	}

This permits the declaration of variables and functions 
internal to the generated lexical analyzer class. 

### Initialization Code for Lexical Analyzer Class

The `%init{ ... %init}` directive allows the user to write C# code to 
be copied into the constructor for the lexical analyzer class.

	%init{
		... code ...
	%init}

The `%init{` and `%init}` directives should be situated at the beginning of a line. The specified C# code will be then copied into the lexical analyzer class constructor.

	class Yylex
	{
		Yylex ()
		{
			... code ...
		}
	}

This directive permits one-time initialization of the lexical
analyzer class from inside its constructor. 

### End-of-File Code for Lexical Analyzer Class

The `%eof{ ... %eof}` directive allows the user to write C# code to be 
copied into the lexical analyzer class for execution after the end-of-file is 
reached.

	%eof{
		... code ...
	%eof}

The `%eof{` and `%eof}` directives should be situated
at the beginning of a line. The specified C# code 
will be executed at most once, and
immediately after the end-of-file is reached for the input file
the lexical analyzer class is processing.  

### Macro Definitions

Macro definitions are given in the CsLex directives section of
the specification. Each macro definition is contained on a
single line and consists of a macro name followed by an equal
sign (=), then by its associated definition. The format can
therefore be summarized as follows.

	name = definition;

Non-newline white space, e.g. blanks and tabs, is optional
between the macro name and the equal sign and between the equal
sign and the macro definition. Each macro definition should be
contained on a single line.  

Macro names should be valid identifiers, e.g. sequences of
letters, digits, and underscores beginning with a letter or
underscore.

Macro definitions should be valid regular expressions, the
details of which are described in another section below.

Macro definitions can contain other macro expansions, in the standard
*{name}* format for macros within regular expressions.

However, the user should note that these expressions are macros - not functions or non-terminals - so mutually recursive constructs using macros are
illegal. Therefore, cycles in macro definitions will have
unpredictable results.

### State Declarations

Lexical states are used to control when certain regular
expressions are matched. These are declared in the CsLex
directives in the following way.

    %state state0,state1,state2

Each declaration of a series of lexical states should be
contained on a single line. Multiple declarations can be
included in the same CsLex specification, so the declaration of
many states can be broken into many declarations over multiple
lines.

State names should be valid identifiers, e.g. sequences of
letters, digits, and underscores beginning with a letter or
underscore.

A single lexical state is implicitly declared by CsLex. This
state is called `YYINITIAL`, and the generated lexer begins
lexical analysis in this state.

Rules of lexical analysis begin with an optional state list. If
a state list is given, the lexical rule is matched only when the
lexical analyzer is in one of the specified states. If a state
list is not given, the lexical rule is matched when the lexical
analyzer is in any state.

If a CsLex specification does not make use of states, by neither
declaring states nor preceding lexical rules with state lists,
the resulting lexer will remain in state `YYINITIAL`
throughout execution. Since lexical rules are not prefaced by
state lists, these rules are matched in all existing states,
including the implicitly declared state
`YYINITIAL`. Therefore, everything works as expected if
states are not used at all.  

States are declared as constant integers within the generated
lexical analyzer class. The constant integer declared for a
declared state has the same name as that state. The user should
be careful to avoid name conflict between state names and
variables declared in the action portion of rules or elsewhere
within the lexical analyzer class. A convenient convention would
be to declare state names in all capitals, as a reminder that
these identifiers effectively become constants.

### Character Counting

Character counting is turned off by default, but can be
activated with the `%char` directive.

	%char

The zero-based character index of the first character in the
matched region of text is then placed in the integer variable
`yychar`.

### Line Counting

Line counting is turned off by default, but can be activated
with the `%line` directive.

    %line

The zero-based line index at the beginning of the matched region
of text is then placed in the integer variable <I>yyline</I>.

### Lexical Analyzer Component Titles

The following directives can be used to change the name of the
generated lexical analyzer class, the namespace, the tokenizing function,
and the token return type.

To change the name of the lexical analyzers namespace
from `YyNameSpace`,
use the `%namespace` directive. 

	%namespace mynamespace

To change the name of the lexical
analyzer class from `Yylex`, use the `%class`
directive.

	%class myclass

To change the name of the tokenizing function from `yylex`,
use the `%function` directive. 

	%function myfunction

To change the name of the return type from the tokenizing
function from **Yytoken**, use the `%type` directive.

	%type mytype

If the default names are not altered using these directives,
the tokenizing function is invoked with a call to

	Yylex.yylex();

which returns the **Yytoken** type.

### Default Token Type

To return an integer type for the return type for the tokenizing function
(and therefore the token type), use the `%intwrap`
directive.

	%intwrap

Under default settings, **Yytoken** is the return type of the
tokenizing function **yylex()**, as in the following code fragment:

	class Yylex
	{ 
		...

		public Yytoken yylex()
		{
		...
		}
	}

The `%intwrap` directive replaces the previous code with a
revised declaration, in which the token type has been changed to
integer boxed Object.

	class Yylex
		{
		...

			public object yylex()
			{
			...
			}
		}

This declaration allows lexical actions to return wrapped
integer codes, as in the following code fragment from a
hypothetical lexical action.

	{
	...
	return ((object) i);
	}

Notice that the effect of `%intwrap` directive can be
equivalently accomplished using the `%type` directive, as
follows.

	%type object

This manually changes the name of the return type of `Yylex.yylex()` to `object`. 


### `YYEOF` on End-of-File

The `%yyeof` directive causes the constant integer
**Yylex.YYEOF** to be declared and returned upon
end-of-file.

	%yyeof

This constant integer is discussed in more detail in a previous
section. Note also that **Yylex.YYEOF** is an **int**, so
the user should make sure that this return value is compatible
with the return type of **yylex()**.

## Character Sets

The default settings support an alphabet of character codes
between 0 and 127 inclusive. If the generated lexical analyzer
receives an input character code that falls outside of these
bounds, the lexer may fail.

The `%full` directive can be used to extend this alphabet
to include all 8-bit values.

	%full

If the `%full` directive is given, CsLex will generate a
lexical analyzer that supports an alphabet of character codes
between 0 and 255 inclusive.

### Character Format To and From File

Under the status quo, CsLex and the lexical analyzer it generates
read from and write to Ascii text files, with byte sized
characters. 

However, to support further extensions on the CsLex
tool, all internal processing of characters is done using the
16-bit C# character type, although the full range of 16-bit
values is not yet supported.

### Specifying the Return Value on End-of-File

The `%eofval` directive specifies the return
value on end-of-file. This directive allows the user to write
C# code to be copied into the lexical analyzer tokenizing
function **yylex()** for execution when the end-of-file
is reached. This code must return a value compatible with the
type of the tokenizing function **yylex()**.

    %eofval{
    ... code ...
    %eofval}

The specified C# code in *code* determines the
return value of **yylex()** when the end-of-file is
reached for the input file the lexical analyzer class is
processing. This will also be the value returned by
**yylex()** each additional time this function is
called after end-of-file is initially reached, so
*code* may be executed more than once. Finally, the
`%eofval{` and `%eofval}` directives should be
situated at the beginning of a line.

An example of usage is given below. Suppose the return value
desired on end-of-file is `new token(sym.EOF)` rather
than the default value `null`. The user adds the following
declaration to the specification file.

	%eofval{
	return new token(sym.EOF);
	%eofval}

The code is then copied into `Yylex.yylex()` into the
appropriate place.

	public Yytoken yylex()
	{
	... 

	return new token(sym.EOF);
	}

The value returned by **yylex()** upon end-of-file and
from that point onward is now `new token(sym.EOF)`.

### Specifying an interface to implement

CsLex allows the user to specify an interface which the
**Yylex** class will implement. By adding the following
declaration to the input file:

	%implements classname

the user specifies that Yylex will implement
**classname**. The generated parser class declaration will
look like:

	class Yylex : classname
	{
	...
	}

### Making the Generated Class Public

The `%public` directive causes the lexical analyzer class
generated by CsLex to be a public class.

	%public

The default behavior adds no access specifier to the generated
class, resulting in the class being visible only from the
current package.

## Regular Expression Rules

The third part of the CsLex specification consists of a series of
rules for breaking the input stream into tokens. These rules
specify regular expressions, then associate these expressions
with actions consisting of C# source code.

The rules have three distinct parts: the optional state list,
the regular expression, and the associated action. This format
is represented as follows.

[ *states* ] *expression* { *action* }

Each part of the rule is discussed in a section below.


If more than one rule matches strings from its input, the
generated lexer resolves conflicts between rules by greedily
choosing the rule that matches the longest string. If more than
one rule matches strings of the same length, the lexer will
choose the rule that is given first in the CsLex
specification. Therefore, rules appearing earlier in the
specification are given a higher priority by the generated
lexer.


The rules given in a CsLex specification should match all
possible input. If the generated lexical analyzer receives input
that does not match any of its rules, an error will be raised.


Therefore, all input should be matched by at least one
rule. This can be guaranteed by placing the following rule at
the bottom of a CsLex specification:

	. { Console.WriteLine("Unmatched input: " + yytext()); }

The dot (.), as described below, will match any input except for
the newline.

### Lexical States

An optional lexical state list preceeds each rule. This list
should be in the following form:

*state0* **{** , *state1*, *state2*, *...* **}**

The outer set of brackets ({}) indicate that multiple states are
optional. The greater than (>) and less than (<) symbols
represent themselves and should surround the state list,
preceding the regular expression. The state list specifies under
which initial states the rule can be matched.

For instance, if **yylex()** is called with the lexer at
state *A*, the lexer will attempt to match the input only
against those rules that have *A* in their state list.

If no state list is specified for a given rule, the rule is
matched against in all lexical states.

### Regular Expressions

Regular expressions should not contain any white space, as white
space is interpreted as the end of the current regular
expression. There is one exception; if (non-newline) white space
characters appear from within double quotes, these characters
are taken to represent themselves. For instance, " " is
interpreted as a blank space.

The alphabet for CsLex is the Ascii character set, meaning
character codes between 0 and 127 inclusive.

The following characters are metacharacters, with special
meanings in CsLex regular expressions.

	? * + | ( ) ^ $ / ; . =   [ ] { } " \

Otherwise, individual characters stand for themselves.


*ef* Consecutive regular expressions represents their concatenation. 

*e*||*f* The vertical bar (|) represents an option
between the regular expressions that surround it, so matches
either expression *e* or *f*.


The following escape sequences are recognized and expanded: 


<TABLE>
  <TR><TH>Sequence</TH><TH>Description</TH></TR>
  <TR>
    <TD>\b</TD>
    <TD>Backspace</TD></TR>
  <TR>
    <TD>\n</TD>
    <TD>newline</TD></TR>
  <TR>
    <TD>\t</TD>
    <TD>Tab</TD></TR>
  <TR>
    <TD>\f</TD>
    <TD>Formfeed</TD></TR>
  <TR>
    <TD>\r</TD>
    <TD>Carriage return</TD></TR>
  <TR>
    <TD>\<i>ddd</i></TD>
    <TD>The character code corresponding to the number formed by three octal 
      digits <i>ddd</i></TD></TR>
  <TR>
    <TD>\x<i>dd</i></TD>
    <TD>The character code corresponding to the number formed by
	two hexadecimal digits <i>dd</i></TD></TR> 
  <TR>
    <TD>\u<i>dddd</i></TD>
    <TD>
	The Unicode character code corresponding to the number
	formed by four 	hexidecimal digits
	<i>dddd</i>. <b>The support of Unicode escape
	sequences of this type is not yet implemented.</b>
    </TD>
  </TR>
  <TR>
    <TD>\^<i>C</i></TD>
    <TD>Control character</TD></TR>
  <TR>
    <TD>\<i>c</i></TD>
    <TD>A backslash followed by any other character *c*
	matches itself
    </TD>
  </TR>
  </TBODY>
</TABLE>

<table>
  <tr>
  <th>Symbol</th>
  <th>Meaning</th>
  </tr>
  <tr>
    <td> $ </td>
    <td> The dollar sign ($) denotes the end of a line. If
	the dollar sign ends a regular expression, the
	expression is matched only at the end of a
	line. </td>
  </tr>
  <tr>
    <td> . </td>
    <td> The dot (.) matches any character except the newline,
	so this expression is equivalent to [^\n]. </td>
  </tr>
  <tr>
    <td> "..." </td>
    <td> Metacharacters lose their meaning within double quotes
	and represent themselves. The sequence <CODE>\"</CODE>
	(which represents the single character <CODE>"</CODE>)
	is the only exception. </td>
  </tr>
  <tr>
    <td> {<i>name</i>} </td>
    <td> Curly braces denote a macro expansion, with <i>name</i>
	the declared name of the associated macro. </td>
  </tr>
  <tr>
    <td> * </td>
    <td> The star (*) represents Kleene closure and matches zero
	or more repetitions of the preceding regular
	expression. </td>
  </tr>
  <tr>
    <td> + </td>
    <td> The plus (+) matches one or more repetitions of the
	preceding regular expression, so <i>e</i>+ is equivalent
	to <i>ee</i>. </td>
  </tr>
  <tr>
    <td> ? </td>
    <td> The question mark (?) matches zero or one repetitions
	of the preceding regular expression. </td>
  </tr>
  <tr>
    <td> (...) </td>
    <td> Parentheses are used for grouping within regular
	expressions. </td>
  </tr>
  <tr valign=top>
    <td> [...] </td>
    <td> Square backets denote a class of characters and match
	any one character enclosed in the backets. If the first
	character following the left bracket ([) is the up arrow
	(^), the set is negated and the expression matches any
	character except those enclosed in the
	backets. Different metacharacter rules hold inside the
	backets, with the following expressions having special
	meanings:

	<TABLE>
	  <TR>
	    <TD>{<i>name</i>}</TD>
	    <TD>Macro expansion</TD>
	  </TR>
	  <TR>
	    <TD><i>a</i> - <i>b</i></TD>
	    <TD>Range of character codes from <i>a</i> to
		<i>b</i> to be included in character set</TD> 
	  </TR>
	  <TR>
	    <TD>"..."</TD>
	    <TD>All metacharacters within double quotes lose
		their special meanings. The sequence
		<CODE>\"</CODE> (which represents the single
		character <CODE>"</CODE>) is the only
		exception.</TD>
	  </TR>
	  <TR>
	    <TD>\</TD>
	    <TD>Metacharacter following backslash(\) loses its
		special meaning</TD>
	  </TR>
	  <tr>
	  </tr>
        </TABLE>

	For example, [a-z] matches any lower-case letter, [^0-9]
	matches anything except a digit, and [0-9a-fA-F] matches
	any hexadecimal digit. Inside character class brackets,
	a metacharacter following a backslash loses its special
	meaning. Therefore, [\-\\] matches a dash or a
	backslash. Likewise ["A-Z"] matches one of the three
	characters A, dash, or Z. Leading and trailing dashes in
	a character class also lose their special meanings, so
	[+-] and [-+] do what you would expect them to (ie,
	match only '+' and '-').
    </td>
  </tr>
</table>
	
### Associated Actions

The action associated with a lexical rule consists of C# code
enclosed inside block-delimiting curly braces. 

	{
	performAction();
	return null;
	}

The C# code is copied, as given, into the
state-driven lexical analyzer method produced by Lex.

All curly braces contained within the block, that are not part of strings
or comments, should be balanced.

#### Actions and Recursion

If a null return value is returned from an action, the lexical
analyzer will loop, searching for the next match from the input
stream and returning the value associated with that match.

The lexical analyzer can be made to recur explicitly with a call
to **yylex()**, as in the following code fragment. 

	{
	...
	return yylex();
	}

This code fragment causes the lexical analyzer to recur,
searching for the next match in the input and returning the
value associated with that match. The same effect can be had,
however, by simply returning null from a given action. This
results in the lexer searching for the next match, without the
additional overhead of recursion.

The preceding code fragment is an example of tail recursion,
since the recursive call comes at the end of the calling
function's execution. The following code fragment is an example
of a recursive call that is not tail recursive. 

	{
	... 
	next = yylex();
	return null;
	}

Recursive actions that are not tail-recursive work in the
expected way, except that variables such as **yyline** and
**yychar** may be changed during recursion.

#### State Transitions

If lexical states are declared in the CsLex directives section,
transitions on these states can be declared within the regular
expression actions. State transitions are made by the following
function call.

	yybegin(state);

The void function **yybegin()** is passed the state name
**state** and effects a transition to this lexical state.

The state **state** must be declared within the CsLex
directives section, or this call will result in a compiler error
in the generated source file. The one exception to this
declaration requirement is state **YYINITIAL**, the lexical
state implicitly declared by CsLex. The generated lexer begins
lexical analysis in state **YYINITIAL** and remains in this
state until a transition is made.

#### Available Lexical Values

The following values, internal to the **Yylex** class, are
available within the action portion of the lexical rules.
<table>
	<tr><th>Method</th><th>Type</th><th>Activation Directive</th><th>Description</th></tr>
	<tr>
		<td><code>yytext()</code></td>
		<td><code>string</code></td>
		<td>always active</td>
		<td>matched portion of the character input stream</td>
	</tr>
</table>
<table>
	<tr>
		<th>Variable</th>
		<th>Type</th>
		<th>Activation Directive</th>
		<th>Description</th>
	</tr>

	<tr>
	<td><code>yychar</code></td>
	<td><code>int</code></td>
	<td><code>%char</code></td>
	<td>Zero-based character index of the first character in the matched portion of the input stream</td>
	</tr>

	<tr>
	<td><code>yyline</code></td>
	<td><code>int</code></td>
	<td><code>%line</code></td>
	<td>Zero-based line number of the start of the matched portion of the input stream</td>
	</tr>
</table>

## Generated Lexical Analyzers

CsLex will take a properly-formed specification and transform it
into a C# source file for the corresponding lexical analyzer.

The generated lexical analayzer resides in the class
**Yylex**. There are two constructors to this class, both
requiring a single argument: the input stream to be
tokenized. The input stream may either be of type
**System.IO.StreamReader** or **System.IO.FileReader**.

The access function to the lexer is `Yylex.yylex()`, which
returns the next token from the input stream. The return type is
**Yytoken** and the function is declared as follows.

	class Yylex
	{
		...
		public Yytoken yylex()
		{
			...
		}
	}

The user must declare the type of **Yytoken** and can
accomplish this conveniently in the first section of the CsLex
specification, the user code section.

For instance, to make
`Yylex.yylex()` return a class containing an integer type (logically boxing it),
the user would enter the following code somewhere
preceding the first `%%`. 

	class Yytoken
	{
		public int Field;

		public Yytoken(int f)
		{
			Field = f;
		}
	}

Then, in the lexical actions, wrapped integers would be
returned, in something like this way.

	{
		return new Yytoken(0);
	}

Likewise, in the user code section, a class could be defined declaring 
constants that correspond to each of the token types.

	class TokenCodes
	{
	...

	public static final STRING = 0;
	public static final INTEGER = 1;

	...
	}

Then, in the lexical actions, these token codes could be
returned. 

	{
	...
	return ((object) STRING);
	}

These are simplified examples; in actual use, one would probably
define a token class containing more information than an integer
code.

These examples begin to illustrate the object-oriented
techniques a user could employ to define an arbitrarily complex
token type to be returned by **Yylex.yylex()**. In
particular, inheritance permits the user to return more than one
token type. If a distinct token type was needed for strings and
integers, the user could make the following declarations. 

	class Yytoken { ... }
	class IntegerToken : Yytoken { ... }
	class StringToken : Yytoken { ... }

Then the user could return both **IntegerToken** and
**StringToken** types from the lexical actions.

The names of the lexical analyzer class, the tokening function,
and its return type each may be altered using the CsLex
directives.

## Credits

CsLex is a derivative of the JLex implementation by Elliot
Joel Berk and C. Scott Ananian.

The design and architecture of CsLex, written in C#, is based on
a melding of the JLex implementation and the Lex functional
specification.  JLex was written for the Java language, and it's
direct antecedent Lex, designed and written for the C language.

CsLex distinguishes itself by incorporating a number of C#
language constructs and services; refer to the design notes for
CsLex for more details on C# specific features incorporated into
the CsLex design. 

### CsLex COPYRIGHT NOTICE, LICENSE AND DISCLAIMER

CsLex is Copyright 2000 by Brad Merrill

Permission to use, copy, modify, and distribute this software
and its documentation for any purpose and without fee is hereby
granted, provided that the above copyright notice appear in all
copies and that both the copyright notice and this permission
notice and warranty disclaimer appear in supporting
documentation, and that the name of the authors or their
employers not be used in advertising or publicity pertaining to
distribution of the software without specific, written prior
permission. 

The authors and their employers disclaim all warranties with
regard to this software, including all implied warranties of
merchantability and fitness. In no event shall the authors or
their employers be liable for any special, indirect or
consequential damages or any damages whatsoever resulting from
loss of use, data or profits, whether in an action of contract,
negligence or other tortious action, arising out of or in
connection with the use or performance of this software.

### JLEX COPYRIGHT NOTICE, LICENSE AND DISCLAIMER

Copyright 1996-2000 by Elliot Joel Berk and C. Scott Ananian 

Permission to use, copy, modify, and distribute this software and its documentation for any purpose and without fee is hereby granted, provided that the above copyright notice appear in all copies and that both the copyright notice and this permission notice and warranty disclaimer appear in supporting documentation, and that the name of the authors or their employers not be used in advertising or publicity pertaining to distribution of the software without specific, written prior permission. 

The authors and their employers disclaim all warranties with regard to this software, including all implied warranties of merchantability and fitness. In no event shall the authors or their employers be liable for any special, indirect or consequential damages or any damages whatsoever resulting from loss of use, data or profits, whether in an action of contract, negligence or other tortious action, arising out of or in connection with the use or performance of this software. 

Java is a trademark of Sun Microsystems, Inc. References to the Java programming language in relation to JLex are not meant to imply that Sun endorses this product. 

</body>
</html>
