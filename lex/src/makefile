
SRCLIST = Lex.cs \
Input.cs Minimize.cs \
NfaPair.cs Dfa.cs CharSet.cs Utility.cs \
Accept.cs Anchor.cs Error.cs \
Bunch.cs Alloc.cs \
MakeNfa.cs Nfa.cs Nfa2Dfa.cs \
Gen.cs Spec.cs DTrans.cs Emit.cs \
SimplifyNFA.cs BitSet.cs

BIN = ..\bin
TEST = ..\tests

TEST_SRC = $(TEST)\test.txt

.SUFFIXES: .exe .cs .lex .txt

.lex.cs :
	lex $*.lex

.cs.exe:
	csc /o /t:exe $*.cs

#	csc /debug /t:exe /out:$(BIN)\lex.exe /D:DEBUG /D:OLD_DEBUG /D:DESCENT_DEBUG /D:FOODEBUG /D:DO_DEBUG /D:OLD_DUMP_DEBUG /D:NOT_EDBG @<<lex.tmp
#	csc /debug /t:exe /out:$(BIN)\lex.exe /D:DEBUG /D:NOT_EDBG @<<lex.tmp
#	csc /debug /t:exe /out:$(BIN)\lex.exe /D:DEBUG @<<lex.tmp
#	csc /debug /t:exe /out:$(BIN)\lex.exe @<<lex.tmp
#	csc /t:exe /out:$(BIN)\lex.exe @<<lex.tmp

all : $(BIN)\simple.txt $(BIN)\sample.txt

$(BIN)\simple.txt : $(BIN)\simple_lex.exe $(TEST)\simple.lex $(TEST_SRC)
	cd $(BIN)
	simple_lex $(TEST_SRC) > $(BIN)\simple.txt

$(BIN)\sample.txt : $(BIN)\sample_lex.exe $(TEST)\sample.lex $(TEST_SRC)
	cd $(BIN)
	sample_lex $(TEST_SRC) > $(BIN)\sample.txt

$(BIN)\simple_lex.cs : $(TEST)\simple.lex $(BIN)\lex.exe
	cd $(BIN)
	lex $(TEST)\simple.lex

$(BIN)\sample_lex.cs : $(TEST)\sample.lex $(BIN)\lex.exe
	cd $(BIN)
	lex $(TEST)\sample.lex

$(BIN)\simple_lex.exe : $(BIN)\simple_lex.cs

$(BIN)\sample_lex.exe : $(BIN)\sample_lex.cs

$(BIN)\lex.exe : $(SRCLIST) makefile
	csc /o /t:exe /out:$(BIN)\lex.exe @<<lex.tmp
$(SRCLIST)
<<

clean:
	del $(BIN)\*.* /q

