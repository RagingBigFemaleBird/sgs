using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SgsCore
{
    public class Game
    {
        public enum TurnPhase
        {
            BeforeTurnStart,
            TurnStart,
            Judgement,
            Dealing,
            Playing,
            Discarding,
            TurnFinish,
            AfterTurnFinish
        }

        public struct Place
        {
            public Player player;
            public DeckType pile;
        }

        public static Game()
        {
            games = new Dictionary<Thread,Game>();
        }

        public Game()
        {
            cardSet = new List<Card>();
            triggers = new Dictionary<GameEvent, SortedList<int, Trigger>>();
            decks = new Dictionary<DeckType, List<Card>>();
            foreach (var deck in Enum.GetValues(typeof(DeckType)).Cast<DeckType>())
            {
                decks[deck] = new List<Card>();
            }
        }

        public void Run()
        {
        }

        protected abstract void PopulateCards();
        protected abstract void InstallInitalTriggers();

        public static Game CurrentGame
        {
            get { return games[Thread.CurrentThread]; }
            set { games[Thread.CurrentThread] = value; }
        }

        protected static Dictionary<Thread, Game> games;
        protected List<Card> cardSet;

        public List<Card> CardSet
        {
            get { return cardSet; }
            set { cardSet = value; }
        }

        protected Dictionary<GameEvent, SortedList<int, Trigger>> triggers;

        public void RegisterTrigger(GameEvent gameEvent, Trigger trigger)
        {
            if (triggers[gameEvent] == null)
            {
                triggers[gameEvent] = new SortedList<int, Trigger>();
            }
            triggers[gameEvent].Add(trigger.Priority, trigger);
        }

        public void Emit(GameEvent gameEvent, Object eventParam)
        {
            SortedList<int, Trigger> triggers = this.triggers[gameEvent];
            foreach (var trigger in triggers.Values)
            {
                trigger.Run(gameEvent, eventParam);
            }
        }
        
        protected Dictionary<DeckType, List<Card>> decks;

        protected List<Player> players;

        internal List<Player> Players
        {
            get { return players; }
            set { players = value; }
        }

        public struct CardsMovement
        {
            public List<Card> cards;
            public Place from, to;
        }

        public void MoveCards(List<CardsMovement> move)
        {

        }

        public void MoveCards(CardsMovement move)
        {
            List<CardsMovement> moves = new List<CardsMovement>();
            moves.Add(move);
            MoveCards(moves);
        }

        public void DrawCards(Player player, int num)
        {

        }

        /// <summary>
        /// Get player next to the current one in counter-clock seat map.
        /// </summary>
        /// <param name="currentPlayer"></param>
        /// <returns></returns>
        public Player NextPlayer(Player currentPlayer)
        {
            int numPlayers = players.Count;
            int i;
            for (i = 0; i < numPlayers; i++)
            {
                if (players[i] == currentPlayer)
                {
                    break;
                }
            }

            // The next player to the last player is the first player.
            if (i == numPlayers - 1)
            {
                return players[0];
            }
            else if (i >= numPlayers)
            {
                return null;
            }
            else 
            {
                return players[i + 1];
            }
        }
    }
}
