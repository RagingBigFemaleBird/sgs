using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SgsCore
{
    class Player
    {
        public enum Gender
        {
            None = 0,
            Male = 1,
            Female = 2,
            Both = Male & Female
        }

        protected int id;
        protected Gender gender;        
        protected int maxHealth;
        protected int health;
        protected Dictionary<DeckType, List<Card>> decks;
    }
}
