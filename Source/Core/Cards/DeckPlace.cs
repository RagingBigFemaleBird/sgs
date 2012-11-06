using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Players;

namespace Sanguosha.Core.Cards
{
    public class DeckPlace
    {
        public DeckPlace(Player player, DeckType deckType)
        {
            this.player = player;
            this.deckType = deckType;
        }

        private Player player;

        public Player Player
        {
            get { return player; }
            set { player = value; }
        }

        private DeckType deckType;

        public DeckType DeckType
        {
            get { return deckType; }
            set { deckType = value; }
        }

        public override string ToString()
        {
            return "Player " + player.Id + ", " + deckType.ToString();
        }

        public override bool Equals(object obj)
        {
            DeckPlace dp = obj as DeckPlace;
            if (dp == null)
                return false;
            return player == dp.player && deckType == dp.deckType;
        }

        public override int GetHashCode()
        {
            return ((player == null) ? 0 : player.GetHashCode()) + ((deckType == null) ? 0 : deckType.GetHashCode());
        }

        public static bool operator ==(DeckPlace a, DeckPlace b)
        {
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(DeckPlace a, DeckPlace b)
        {
            return !(a == b);
        }
    }
}
