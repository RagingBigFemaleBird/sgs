using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.Cards
{
    class CardPlace
    {
        private DeckPlace deck;

        public DeckPlace Deck
        {
            get { return deck; }
            set { deck = value; }
        }

        private int position;

        public int Position
        {
            get { return position; }
            set { position = value; }
        }
    }
}
