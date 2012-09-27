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
using Sanguosha.Core.UI;

namespace Sanguosha.Core.Games
{
    [Serializable]
    public class GameOverException : SgsException { }

    public struct CardsMovement
    {
        public List<Card> cards;
        public DeckPlace to;
    }

    public enum DamageElement
    {
        None,
        Fire,
        Lightning,
    }

    public abstract class Game
    {
        [Serializable]
        class EndOfDealingDeckException : SgsException { }

        [Serializable]
        class GameAlreadyStartedException : SgsException { }

        static Game()
        {
            games = new Dictionary<Thread,Game>();
        }

        public Game()
        {
            cardSet = new List<Card>();
            cardSet.Add(new Card() { Type = "Sanguosha.Core.Cards.Battle.HuoGong", Rank = 9, Suit = SuitType.Heart });
            cardSet.Add(new Card() { Type = "Sanguosha.Core.Cards.Battle.HuoGong", Rank = 10, Suit = SuitType.Club });
            cardSet.Add(new Card() { Type = "SHA", Rank = 1, Suit = SuitType.Spade });
            cardSet.Add(new Card() { Type = "SHA", Rank = 2, Suit = SuitType.Heart });
            cardSet.Add(new Card() { Type = "SHA", Rank = 3, Suit = SuitType.Heart });
            cardSet.Add(new Card() { Type = "SHA", Rank = 4, Suit = SuitType.Heart });
            cardSet.Add(new Card() { Type = "SHA", Rank = 5, Suit = SuitType.Heart });
            cardSet.Add(new Card() { Type = "SHA", Rank = 6, Suit = SuitType.Heart });
            cardSet.Add(new Card() { Type = "SHA", Rank = 7, Suit = SuitType.Heart });
            cardSet.Add(new Card() { Type = "SHA", Rank = 8, Suit = SuitType.Heart });
            cardSet.Add(new Card() { Type = "SHA", Rank = 9, Suit = SuitType.Heart });
            cardSet.Add(new Card() { Type = "SHA", Rank = 10, Suit = SuitType.Heart });
            cardSet.Add(new Card() { Type = "SHA", Rank = 11, Suit = SuitType.Heart });
            cardSet.Add(new Card() { Type = "SHA", Rank = 12, Suit = SuitType.Heart });
            cardSet.Add(new Card() { Type = "SHA", Rank = 13, Suit = SuitType.Heart });
            cardSet.Add(new Card() { Type = "SHA", Rank = 14, Suit = SuitType.Heart });
            cardSet.Add(new Card() { Type = "SHA", Rank = 15, Suit = SuitType.Heart });
            cardSet.Add(new Card() { Type = "SHA", Rank = 16, Suit = SuitType.Heart });
            cardSet.Add(new Card() { Type = "SHA", Rank = 17, Suit = SuitType.Heart });
            cardSet.Add(new Card() { Type = "SHA", Rank = 18, Suit = SuitType.Heart });
            cardSet.Add(new Card() { Type = "SHA", Rank = 19, Suit = SuitType.Heart });
            cardSet.Add(new Card() { Type = "SHA", Rank = 20, Suit = SuitType.Heart });
            triggers = new Dictionary<GameEvent, SortedList<double, Trigger>>();
            decks = new DeckContainer();
            players = new List<Player>();
            cardHandlers = new Dictionary<string, CardHandler>();
            uiProxies = new Dictionary<Player, IUiProxy>();
        }



        public virtual void Run()
        {
            if (games.ContainsKey(Thread.CurrentThread))
            {
                throw new GameAlreadyStartedException();
            }
            else
            {
                games.Add(Thread.CurrentThread, this);
            }
            // Put the whole deck in the dealing deck
            decks[DeckType.Dealing] = cardSet.GetRange(0, cardSet.Count);
            foreach (Card card in cardSet)
            {
                card.Place = new DeckPlace(null, DeckType.Dealing);
            }

            InitTriggers();
//            try
//            {
                Emit(GameEvent.GameStart, new GameEventArgs() { Game = this });
/*            }
            catch (GameOverException)
            {

            }
            catch (Exception e)
            {
                Trace.TraceError(e.StackTrace);
            }
  */      }

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
        List<Card> cardSet;

        public List<Card> CardSet
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
            if (!this.triggers.ContainsKey(gameEvent)) return;
            SortedList<double, Trigger> triggers = this.triggers[gameEvent];
            if (triggers == null) return;
            var sortedTriggers = triggers.Values.Reverse();
            foreach (var trigger in sortedTriggers)
            {
                if (trigger.Enabled)
                {
                    trigger.Run(gameEvent, eventParam);
                }
            }
        }

        private Dictionary<Player, IUiProxy> uiProxies;

        public Dictionary<Player, IUiProxy> UiProxies
        {
            get { return uiProxies; }
            set { uiProxies = value; }
        }

        Dictionary<string, CardHandler> cardHandlers;

        public Dictionary<string, CardHandler> CardHandlers
        {
            get { return cardHandlers; }
            set { cardHandlers = value; }
        }

        DeckContainer decks;

        public DeckContainer Decks
        {
            get { return decks; }
            set { decks = value; }
        }

        List<Player> players;

        public List<Player> Players
        {
            get { return players; }
            set { players = value; }
        }

        public void MoveCards(List<CardsMovement> moves)
        {
            foreach (CardsMovement move in moves)
            {
                List<Card> cards = new List<Card>(move.cards);
                // Update card's deck mapping
                foreach (Card card in cards)
                {
                    decks[card.Place].Remove(card);
                    decks[move.to].Add(card);
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

        public Card DrawCard()
        {
            var drawDeck = decks[DeckType.Dealing];
            if (drawDeck.Count == 0)
            {
                Emit(GameEvent.Shuffle, new GameEventArgs() { Game = this });
            }
            if (drawDeck.Count == 0)
            {
                throw new GameOverException();
            }
            Card card = drawDeck.First();
            drawDeck.RemoveAt(0);
            return card;
        }

        public void DrawCards(Player player, int num)
        {
            try
            {
                List<Card> cardsDrawn = new List<Card>();
                for (int i = 0; i < num; i++)
                {
                    cardsDrawn.Add(DrawCard());
                }
                CardsMovement move;
                move.cards = cardsDrawn;
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
                         { GameEvent.PhaseBeginEvents, GameEvent.PhaseProceedEvents,
                           GameEvent.PhaseEndEvents, GameEvent.PhaseOutEvents };
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

        /// <summary>
        /// 造成伤害
        /// </summary>
        /// <param name="source">伤害来源</param>
        /// <param name="dest">伤害目标</param>
        /// <param name="magnitude">伤害点数</param>
        /// <param name="elemental">伤害属性</param>
        /// <param name="cards">造成伤害的牌</param>
        public void DoDamage(Player source, Player dest, int magnitude, DamageElement elemental, List<Card> cards)
        {
            GameEventArgs args = new GameEventArgs() { Source = source, Target = dest, Cards = cards, IntArg = magnitude, IntArg2 = (int)(elemental) };

            Game.CurrentGame.Emit(GameEvent.DamageSourceConfirmed, args);
            Game.CurrentGame.Emit(GameEvent.DamageElementConfirmed, args);
            Game.CurrentGame.Emit(GameEvent.BeforeDamageComputing, args);
            Game.CurrentGame.Emit(GameEvent.DamageComputingStarted, args);
            Game.CurrentGame.Emit(GameEvent.DamageCaused, args);
            Game.CurrentGame.Emit(GameEvent.DamageInflicted, args);
            Game.CurrentGame.Emit(GameEvent.BeforeHealthChanged, args);

            args.Target.Health -= args.IntArg;
            Trace.TraceInformation("Player {0} Lose {1} hp, @ {2} hp", args.Target.Id, args.IntArg, args.Target.Health);

            Game.CurrentGame.Emit(GameEvent.AfterHealthChanged, args);
            Game.CurrentGame.Emit(GameEvent.AfterDamageCaused, args);
            Game.CurrentGame.Emit(GameEvent.AfterDamageInflicted, args);
            Game.CurrentGame.Emit(GameEvent.DamageComputingFinished, args);

        }
    }
}
