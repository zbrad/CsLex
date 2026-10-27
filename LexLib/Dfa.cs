using System;
using System.Collections.Generic;

namespace LexLib
{
    public class Dfa
    {
        /*
         * Member Variables
         */
        bool mark;
        Accept accept;
        int anchor;
        List<Nfa> nfa_set;
        BitSet nfa_bit;
        int label;

        public List<Nfa> GetNFASet() { return nfa_set; }
        public void SetNFASet(List<Nfa> a) { nfa_set = a; }
        public BitSet GetNFABit() { return nfa_bit; }
        public void SetNFABit(BitSet b) { nfa_bit = b; }
        public int GetLabel() { return label; }
        public void SetLabel(int i) { label = i; }
        public Accept GetAccept() { return accept; }
        public void SetAccept(Accept a) { accept = a; }
        public int GetAnchor() { return anchor; }
        public void SetAnchor(int a) { anchor = a; }

        /*
         * Function: Dfa
         */
        public Dfa(int l)
        {
            mark = false;

            accept = null;
            anchor = Spec.NONE;

            nfa_set = null;
            nfa_bit = null;

            label = l;
        }

        public void dump()
        {
#if DUMMY
  Log.Out.WriteLine("[Dfa begin dump]");
  Log.Out.WriteLine("group="+Int32.ToString(group));
  Log.Out.WriteLine("mark="+Boolean.ToString(mark));
  if (accept == null)
    Log.Out.WriteLine("accept=null");
  else
    accept.dump();
  Log.Out.WriteLine("anchor="+Int32.ToString(anchor));
  if (nfa_set == null)
    Log.Out.WriteLine("nfa_set=null");
  else
    {
    int n1 = nfa_set.Count;
    for (int i = 0; i < n1; i++)
      {
      Object o2 = nfa_set[i];
      Log.Out.Write("i="+Int32.ToString(i)+" elem=");
      if (o2 == null)
	Log.Out.WriteLine("null");
      else
	{
	Nfa elem = (CNfa) o2;
	elem.dump();
	}
      }
    }
  if (nfa_bit == null)
    Log.Out.WriteLine("nfa_bit=null");
  else
    {
    Log.Out.Write("nfa_bit("+Int32.ToString(nfa_bit.GetLength())+")=");
    for (int i = 0; i < nfa_bit.GetLength(); i++)
      if (nfa_bit.Get(i))
	Log.Out.Write("1");
      else
	Log.Out.Write("0");
    Log.Out.WriteLine("");
    }
  Log.Out.WriteLine("[Dfa end dump]");
#endif
        }

        public bool IsMarked() { return mark; }
        public void SetMarked() { mark = true; }
        public void ClearMarked() { mark = false; }

    }
}
