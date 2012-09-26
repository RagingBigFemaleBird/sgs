using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Players;

namespace Sanguosha.Core.Cards
{
    public class DeckContainer
    {
        protected class GameAsPlayer : Player
        {
            private static GameAsPlayer instance;

            private GameAsPlayer() { }

            public static GameAsPlayer Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new GameAsPlayer();
                    }
                    return instance;
                }
            }
        }

        public DeckContainer()
        {
            gameDecks = new Dictionary<Player, Dictionary<DeckType, List<Card>>>();
        }

        public List<Card> this[DeckType type]
        {
            get { return this[null, type]; }
            set { this[null, type] = value; }
        }

        public List<Card> this[Player player, DeckType type]
        {
            get
            {
                if (player == null)
                {
                    player = GameAsPlayer.Instance;
                }
                if (!gameDecks.ContainsKey(player))
                {
                    gameDecks[player] = new Dictionary<DeckType, List<Card>>();
                }
                if (!gameDecks[player].ContainsKey(type))
                {
                    gameDecks[player][type] = new List<Card>();
                }
                return gameDecks[player][type];
            }

            set
            {
                if (player == null)
                {
                    player = GameAsPlayer.Instance;
                }
                if (!gameDecks.ContainsKey(player))
                {
                    gameDecks[player] = new Dictionary<DeckType, List<Card>>();
                }
                if (!gameDecks[player].ContainsKey(type))
                {
                    gameDecks[player][type] = value;
                }
            }
        }

        public List<Card> this[DeckPlace place]
        {
            get { return this[place.Player, place.DeckType]; }
            set { this[place.Player, place.DeckType] = value; }
        }

        Dictionary<Player, Dictionary<DeckType, List<Card>>> gameDecks;

        protected Dictionary<Player, Dictionary<DeckType, List<Card>>> GameDecks
        {
            get { return gameDecks; }
            set { gameDecks = value; }
        }
    }
}
