using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Players;

namespace Sanguosha.Core.Cards
{
    public struct DeckPlace
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
    }
}
