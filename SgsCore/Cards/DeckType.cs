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
    }

    public class DeckType
    {
        static DeckType()
        {
            Dealing = new DeckType("Dealing");
            Discard = new DeckType("Discard");
            Compute = new DeckType("Compute");
            ComputeBackup = new DeckType("ComputeBackup");
            Hand = new DeckType("Hand");
            Equipment = new DeckType("Equipment");
            DelayedTools = new DeckType("DelayedTools");
            JudgeResult = new DeckType("JudgeResult");
            None = new DeckType("None");
        }

        public DeckType(string name)
        {
            this.name = name;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            if (!(obj is DeckType))
            {
                return false;
            }
            DeckType type2 = (DeckType)obj;
            return name == type2.name;
        }

        public static DeckType None;
        public static DeckType Dealing;
        public static DeckType Discard;
        public static DeckType Compute;
        public static DeckType ComputeBackup;
        public static DeckType Hand;
        public static DeckType Equipment;
        public static DeckType DelayedTools;
        public static DeckType JudgeResult;
    }



    public class DeckContainer
    {
        protected class GameAsPlayer : Player
        {
           private static GameAsPlayer instance;

           private GameAsPlayer() {}

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
            gameDecks = new Dictionary<Player, Dictionary<DeckType, List<TerminalCard>>>();
        }

        public List<TerminalCard> this[DeckType type]
        {
            get { return this[null, type]; }
            set { this[null, type] = value; }
        }

        public List<TerminalCard> this[Player player, DeckType type]
        {
            get 
            {
                if (player == null)
                {
                    player = GameAsPlayer.Instance;
                }
                if (!gameDecks.ContainsKey(player))
                {
                    gameDecks[player] = new Dictionary<DeckType, List<TerminalCard>>();
                }
                if (!gameDecks[player].ContainsKey(type))
                {
                    gameDecks[player][type] = new List<TerminalCard>();
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
                    gameDecks[player] = new Dictionary<DeckType, List<TerminalCard>>();
                }
                if (!gameDecks[player].ContainsKey(type))
                {
                    gameDecks[player][type] = value;
                }
            }
        }

        public List<TerminalCard> this[DeckPlace place]
        {
            get { return this[place.Player, place.DeckType]; }
            set { this[place.Player, place.DeckType] = value; }
        }

        Dictionary<Player, Dictionary<DeckType, List<TerminalCard>>> gameDecks;

        protected Dictionary<Player, Dictionary<DeckType, List<TerminalCard>>> GameDecks
        {
            get { return gameDecks; }
            set { gameDecks = value; }
        }
    }
}
