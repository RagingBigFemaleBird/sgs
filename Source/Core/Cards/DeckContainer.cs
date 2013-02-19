using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Players;
using System.Diagnostics;

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

        public List<DeckType> GetPlayerPrivateDecks(Player player)
        {
            List<DeckType> list = new List<DeckType>();
            Trace.Assert(player != null);
            if (!GameDecks.Keys.Contains(player)) return list;

            var result = from kvp in GameDecks[player] where kvp.Key is PrivateDeckType select kvp.Key;
            list.AddRange(result);
            return list;
        }

        public List<Card> GetPlayerPrivateCards(Player player)
        {
            var result = new List<Card>();
            foreach (var deckType in GetPlayerPrivateDecks(player))
            {
                result.AddRange(this[player, deckType]);
            }
            return result;
        }
    }
}
