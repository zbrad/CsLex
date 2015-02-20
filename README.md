# CsLex Project

This project was first created in 1999, when C# and the .NET Framework were in their infancy.  I've resurrected the original code, fixed up some typos in the [CsLex Manual](http://github.com/zbrad/CsLex.md "CsLex Manual"), released it on GitHub.


## Plans

Since the parser was created before generics and many of the newer language features, I'm planning to update it to use a Visual Studio solution build (MSBuild), review/update the code to use modern data structures, and ensure that full unicode parsing can be correctly supported.

## Building

This project was created before Visual Studio had C# support, so editing was done with Emacs (with an early version of a C#-mode), and building was using nmake.  

	cd lex\src
	nmake 

