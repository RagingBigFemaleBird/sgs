using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Core.Games
{
    [Serializable]
    public class GameOverException : SgsException { }

    public abstract class Game
    {
        [Serializable]
        class EndOfDealingDeckException : SgsException { }

        static Game()
        {
            games = new Dictionary<Thread,Game>();
        }

        public Game()
        {
            cardSet = new List<TerminalCard>();
            triggers = new Dictionary<GameEvent, SortedList<double, Trigger>>();
            decks = new DeckContainer();
            players = new List<Player>();
        }

        public virtual void Run()
        {
            // Put the whole deck in the dealing deck
            decks[DeckType.Dealing] = cardSet.GetRange(0, cardSet.Count);
            InitTriggers();
            try
            {
                Emit(GameEvent.GameStart, new GameEventArgs() { Game = this });
            }
            catch (GameOverException)
            {

            }
            catch (Exception e)
            {
                Trace.TraceError(e.StackTrace);
            }
        }

        /// <summary>
        /// Initialize triggers at game start time.
        /// </summary>
        protected abstract void InitTriggers();

        public static Game CurrentGame
        {
            get { return games[Thread.CurrentThread]; }
            set { games[Thread.CurrentThread] = value; }
        }

        static Dictionary<Thread, Game> games;
        List<TerminalCard> cardSet;

        public List<TerminalCard> CardSet
        {
            get { return cardSet; }
            set { cardSet = value; }
        }

        Dictionary<GameEvent, SortedList<double, Trigger>> triggers;

        public void RegisterTrigger(GameEvent gameEvent, Trigger trigger)
        {
            if (!triggers.ContainsKey(gameEvent))
            {                
                triggers[gameEvent] = new SortedList<double, Trigger>();
            }
            triggers[gameEvent].Add(trigger.Priority, trigger);
        }

        public void Emit(GameEvent gameEvent, GameEventArgs eventParam)
        {
            SortedList<double, Trigger> triggers = this.triggers[gameEvent];
            var sortedTriggers = triggers.Values.Reverse();
            foreach (var trigger in sortedTriggers)
            {
                if (trigger.Enabled)
                {
                    trigger.Run(gameEvent, eventParam);
                }
            }
        }
        
        DeckContainer decks;

        List<Player> players;

        public List<Player> Players
        {
            get { return players; }
            set { players = value; }
        }

        public struct CardsMovement
        {
            public List<TerminalCard> cards;
            public DeckPlace to;
        }

        public void MoveCards(List<CardsMovement> moves)
        {
            foreach (CardsMovement move in moves)
            {
                // Update card's deck mapping
                foreach (TerminalCard card in move.cards)
                {
                    card.Place = move.to;
                }
            }
        }

        public void MoveCards(CardsMovement move)
        {
            List<CardsMovement> moves = new List<CardsMovement>();
            moves.Add(move);
            MoveCards(moves);
        }

        public void DrawCards(Player player, int num)
        {
            try
            {
                CardsMovement move;
                move.cards = decks[DeckType.Dealing].GetRange(0, num - 1);
                move.to = new DeckPlace(player, DeckType.Hand);
                MoveCards(move);
            }
            catch (ArgumentException)
            {
                throw new EndOfDealingDeckException();
            }            
        }

        Player currentPlayer;

        public Player CurrentPlayer
        {
            get { return currentPlayer; }
            set { currentPlayer = value; }
        }

        TurnPhase currentPhase;

        public TurnPhase CurrentPhase
        {
            get { return currentPhase; }
            set { currentPhase = value; }
        }

        public virtual void Advance()
        {
            var events = new Dictionary<TurnPhase,GameEvent>[]
                         {GameEvent.PhaseBeginEvents, GameEvent.PhaseProceedEvents,
                          GameEvent.PhaseEndEvents, GameEvent.PhaseOutEvents};
            GameEventArgs args = new GameEventArgs() { Game = this, Source = currentPlayer };
            foreach (var gameEvent in events)
            {
                if (gameEvent.ContainsKey(currentPhase))
                {
                    Emit(gameEvent[currentPhase], args);
                }
            }
            
            currentPhase++;
            if ((int)currentPhase >= Enum.GetValues(typeof(TurnPhase)).Length)
            {
                currentPlayer = NextPlayer(currentPlayer);
                currentPhase = 0;
            }
            
        }

        /// <summary>
        /// Get player next to the current one in counter-clock seat map.
        /// </summary>
        /// <param name="currentPlayer"></param>
        /// <returns></returns>
        public virtual Player NextPlayer(Player currentPlayer)
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
