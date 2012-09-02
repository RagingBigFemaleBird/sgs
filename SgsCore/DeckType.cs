using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SgsCore
{
    public class DeckType
    {
        public static DeckType()
        {
            Dealing = new DeckType();
            Discard = new DeckType();
            Compute = new DeckType();
            ComputeBackup = new DeckType();
            Hand = new DeckType();
            Equipment = new DeckType();
            DelayedTools = new DeckType();
        }

        public static DeckType Dealing;
        public static DeckType Discard;
        public static DeckType Compute;
        public static DeckType ComputeBackup;
        public static DeckType Hand;
        public static DeckType Equipment;
        public static DeckType DelayedTools;
    }
}
