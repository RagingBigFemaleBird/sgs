using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SgsCore
{
    public class Card
    {
        static Card()
        {
            UnknownCard = new Card();
        }

        Card()
        {
            suit = SuitType.None;
            rank = 0;
        }

        public enum SuitType
        {
            None = 0x00,
            Black = 0x10,
            Club = 0x11,
            Spade = 0x12,
            Red = 0x20,
            Heart = 0x21,
            Diamond = 0x22             
        }

        protected Game.Place place;

        protected Game.Place Place
        {
            get { return place; }
            set { place = value; }
        }
        protected SuitType suit;

        public SuitType Suit
        {
            get { return suit; }
            set { suit = value; }
        }
        protected int rank;

        public int Rank
        {
            get { return rank; }
            set { rank = value; }
        }
        protected static Card UnknownCard;
    }
}
