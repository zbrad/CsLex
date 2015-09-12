namespace LexLib
{
    public class Anchor
    {
        /*
         * Member Variables
         */
        public Accept accept;
        public int anchor;

        /*
         * Function: Anchor
         */
        Anchor()
        {
            accept = null;
            anchor = Spec.NONE;
        }
    }
}
