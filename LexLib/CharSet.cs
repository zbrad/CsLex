using System;
using System.Collections.Generic;

namespace LexLib;

internal class CharSet
{
    /*
     * Member Variables
     */
    private BitSet set;
    private bool compflag;

    /*
     * Function: CharSet
     */
    public CharSet()
    {
        set = new BitSet();
        compflag = false;
    }

    /*
     * Function: complement
     */
    public void complement()
    {
        compflag = true;
    }

    /*
     * Function: add
     */
    public void add(int i)
    {
        set.Set(i, true);
    }

    /*
     * Function: addncase
     * Description: add, ignoring case.
     */
    public void addncase(char c)
    {
        /* Do this in a Unicode-friendly way. */
        /* (note that duplicate adds have no effect) */
        add(c);
        /* Invariant, so the tables don't depend on the build machine's culture. */
        add(Char.ToLowerInvariant(c));
        add(Char.ToUpperInvariant(c));
    }

    /*
     * Function: contains
     */
    public bool contains(int i)
    {
        bool result;

        result = set.Get(i);
        if (compflag)
            return (false == result);
        return result;
    }

    /*
     * Function: mimic
     */
    public void mimic(CharSet s)
    {
        compflag = s.compflag;
        set = new BitSet(s.set);
    }

    public IEnumerator<int> GetEnumerator() { return set.GetEnumerator(); }

    /*
     * Map set using character classes
     */
    public void map(CharSet old, int[] mapping)
    {
        compflag = old.compflag;
        set = new BitSet();

        foreach (int index in old)
        {
            if (index < mapping.Length) // skip unmapped chars
            {
                int pos = mapping[index];
                set.Set(pos, true);
            }
        }
    }

}
