using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;


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

    public abstract class Game : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event 
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

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
            triggers = new Dictionary<GameEvent, SortedList<double, Trigger>>();
            decks = new DeckContainer();
            players = new List<Player>();
            cardHandlers = new Dictionary<string, CardHandler>();
            uiProxies = new Dictionary<Player, IUiProxy>();
        }

        public void LoadExpansion(Expansion expansion)
        {
            cardSet.AddRange(expansion.CardSet);
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
            int id = 0;
            foreach (var g in Game.CurrentGame.CardSet)
            {
                if (id < 8 && g.Type is Core.Heroes.HeroCardHandler)
                {
                    Core.Heroes.HeroCardHandler h = (Core.Heroes.HeroCardHandler)g.Type;
                    Trace.TraceInformation("Assign {0} to player {1}", h.Hero.Name, id);
                    Game.CurrentGame.Players[id].Hero = h.Hero;
                    Game.CurrentGame.Players[id].Allegiance = h.Hero.Allegiance;
                    Game.CurrentGame.Players[id].MaxHealth = Game.CurrentGame.Players[id].Health = h.Hero.DefaultHp;
                    if (id == 0)
                    {
                        Game.CurrentGame.Players[id].Role = Role.Ruler;
                    }
                    else if (id == 1)
                    {
                        Game.CurrentGame.Players[id].Role = Role.Defector;
                    }
                    else if (id < 4)
                    {
                        Game.CurrentGame.Players[id].Role = Role.Loyalist;
                    }
                    else
                    {
                        Game.CurrentGame.Players[id].Role = Role.Rebel;
                    }
                    id++;
                }
                //todo: put this card somewhere else
            }
            // Put the whole deck in the dealing deck
            decks[DeckType.Dealing] = cardSet.GetRange(0, cardSet.Count);
            foreach (Card card in cardSet)
            {
                card.Place = new DeckPlace(null, DeckType.Dealing);
            }

            InitTriggers();
            try
            {
                Emit(GameEvent.GameStart, new GameEventArgs() { Game = this });
            }
            catch (GameOverException)
            {

            }
            /*catch (Exception e)
            {
                Trace.TraceError(e.StackTrace);
            }*/
        }

        /// <summary>
        /// Initialize triggers at game start time.
        /// </summary>
        protected abstract void InitTriggers();

        public static Game CurrentGame
        {
            get { return games[Thread.CurrentThread]; }
            set 
            {
                games[Thread.CurrentThread] = value;                 
            }
        }

        /// <summary>
        /// Mapping from a thread to the game it hosts.
        /// </summary>
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

        /// <summary>
        /// Emit a game event to invoke associated triggers.
        /// </summary>
        /// <param name="gameEvent">Game event to be emitted.</param>
        /// <param name="eventParam">Additional helper for triggers listening on this game event.</param>
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

        private IUiProxy globalProxy;

        public IUiProxy GlobalProxy
        {
            get { return globalProxy; }
            set { globalProxy = value; }
        }

        /// <summary>
        /// Card usage handler for a given card's type name.
        /// </summary>
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

        public void MoveCards(List<CardsMovement> moves, List<UI.IGameLog> logs)
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

        public void MoveCards(CardsMovement move, UI.IGameLog log)
        {
            List<CardsMovement> moves = new List<CardsMovement>();
            moves.Add(move);
            List<UI.IGameLog> logs = new List<IGameLog>();
            MoveCards(moves, logs);
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
                MoveCards(move, new UI.CardUseLog() { Source = player, Targets = null, Skill = null, Cards = null });
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
            set 
            {
                if (currentPlayer == value) return;
                currentPlayer = value;
                OnPropertyChanged("CurrentPlayer");
            }
        }

        TurnPhase currentPhase;

        public TurnPhase CurrentPhase
        {
            get { return currentPhase; }
            set 
            {
                if (currentPhase == value) return;
                currentPhase = value;
                OnPropertyChanged("CurrentPhase");
            }
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
            
            CurrentPhase++;
            if ((int)CurrentPhase >= Enum.GetValues(typeof(TurnPhase)).Length)
            {
                // todo: fix this.
                foreach (string key in CurrentPlayer.AutoResetAttributes)
                {
                    CurrentPlayer[key] = 0;
                }
                CurrentPlayer = NextPlayer(currentPlayer);
                CurrentPhase = TurnPhase.BeforeStart;
            }
            
        }

        /// <summary>
        /// Get player next to the a player in counter-clock seat map.
        /// </summary>
        /// <param name="p">Player</param>
        /// <returns></returns>
        public virtual Player NextPlayer(Player p)
        {
            int numPlayers = players.Count;
            int i;
            for (i = 0; i < numPlayers; i++)
            {
                if (players[i] == p)
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
        /// Get player previous to a player in counter-clock seat map
        /// </summary>
        /// <param name="p">Player</param>
        /// <returns></returns>
        public virtual Player PreviousPlayer(Player p)
        {
            int numPlayers = players.Count;
            int i;
            for (i = 0; i < numPlayers; i++)
            {
                if (players[i] == p)
                {
                    break;
                }
            }

            // The previous player to the first player is the last player
            if (i == 0)
            {
                return players[numPlayers - 1];
            }
            else if (i >= numPlayers)
            {
                return null;
            }
            else
            {
                return players[i - 1];
            }
        }

        public virtual int DistanceTo(Player from, Player to)
        {
            int distRight = from[PlayerAttribute.RangeMinus], distLeft = from[PlayerAttribute.RangeMinus];
            Player p = from;
            while (p != to)
            {
                p = NextPlayer(p);
                distRight++;
            }
            distRight += to[PlayerAttribute.RangePlus];
            p = from;
            while (p != to)
            {
                p = PreviousPlayer(p);
                distLeft++;
            }
            distLeft += to[PlayerAttribute.RangePlus];
            return distRight > distLeft ? distLeft : distRight;
        }

        /// <summary>
        /// 造成伤害
        /// </summary>
        /// <param name="source">伤害来源</param>
        /// <param name="dest">伤害目标</param>
        /// <param name="magnitude">伤害点数</param>
        /// <param name="elemental">伤害属性</param>
        /// <param name="cards">造成伤害的牌</param>
        public void DoDamage(Player source, Player dest, int magnitude, DamageElement elemental, ICard card)
        {
            GameEventArgs args = new GameEventArgs() { Source = source, Targets = new List<Player>(), Card = card, IntArg = -magnitude, IntArg2 = (int)(elemental) };
            args.Targets.Add(dest);

            try
            {
                Game.CurrentGame.Emit(GameEvent.DamageSourceConfirmed, args);
                Game.CurrentGame.Emit(GameEvent.DamageElementConfirmed, args);
                Game.CurrentGame.Emit(GameEvent.BeforeDamageComputing, args);
                Game.CurrentGame.Emit(GameEvent.DamageComputingStarted, args);
                Game.CurrentGame.Emit(GameEvent.DamageCaused, args);
                Game.CurrentGame.Emit(GameEvent.DamageInflicted, args);
                Game.CurrentGame.Emit(GameEvent.BeforeHealthChanged, args);
            }
            catch (TriggerResultException e)
            {
                if (e.Status == TriggerResult.End)
                {
                    Trace.TraceInformation("Damage Aborted");
                    return;
                }
                Trace.Assert(false);
            }

            Trace.Assert(args.Targets.Count == 1);
            args.Targets[0].Health += args.IntArg;
            Trace.TraceInformation("Player {0} Lose {1} hp, @ {2} hp", args.Targets[0].Id, -args.IntArg, args.Targets[0].Health);

            Game.CurrentGame.Emit(GameEvent.AfterHealthChanged, args);
            Game.CurrentGame.Emit(GameEvent.AfterDamageCaused, args);
            Game.CurrentGame.Emit(GameEvent.AfterDamageInflicted, args);
            Game.CurrentGame.Emit(GameEvent.DamageComputingFinished, args);

        }

        public Card Judge(Player player)
        {
            Card c = Game.CurrentGame.DrawCard();
            GameEventArgs args = new GameEventArgs();
            args.Source = player;
            args.Card = c;
            Game.CurrentGame.Emit(GameEvent.PlayerJudge, args);
            Trace.Assert(args.Source == player);
            c = (Card)args.Card;
            Trace.Assert(c != null);
            return c;
        }

        public void RecoverHealth(Player source, Player target, int magnitude)
        {
            if (target.Health >= target.MaxHealth)
            {
                return;
            }
            GameEventArgs args = new GameEventArgs() { Source = source, Targets = new List<Player>(), IntArg = magnitude, IntArg2 = 0 };
            args.Targets.Add(target);

            Game.CurrentGame.Emit(GameEvent.BeforeHealthChanged, args);

            Trace.Assert(args.Targets.Count == 1);
            args.Targets[0].Health += args.IntArg;
            Trace.TraceInformation("Player {0} gain {1} hp, @ {2} hp", args.Targets[0].Id, args.IntArg, args.Targets[0].Health);

            Game.CurrentGame.Emit(GameEvent.AfterHealthChanged, args);
        }

        public void LoseHealth(Player source, int magnitude)
        {
            GameEventArgs args = new GameEventArgs() { Source = source, Targets = new List<Player>(), IntArg = -magnitude, IntArg2 = 0 };
            args.Targets.Add(source);

            Game.CurrentGame.Emit(GameEvent.BeforeHealthChanged, args);

            Trace.Assert(args.Targets.Count == 1);
            args.Targets[0].Health += args.IntArg;
            Trace.TraceInformation("Player {0} lose {1} hp, @ {2} hp", args.Targets[0].Id, args.IntArg, args.Targets[0].Health);

            Game.CurrentGame.Emit(GameEvent.AfterHealthChanged, args);
        }

        /// <summary>
        /// 处理玩家使用卡牌事件。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="c"></param>
        public void PlayerUsedCard(Player source, ICard c)
        {
            try
            {
                GameEventArgs arg = new GameEventArgs();
                arg.Source = source;
                arg.Targets = null;
                arg.Card = c;

                Emit(GameEvent.PlayerUsedCard, arg);
            }
            catch (TriggerResultException)
            {
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// 处理玩家打出卡牌事件。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="c"></param>
        public void PlayerPlayedCard(Player source, ICard c)
        {
            try
            {
                GameEventArgs arg = new GameEventArgs();
                arg.Source = source;
                arg.Targets = null;
                arg.Card = c;

                Emit(GameEvent.PlayerPlayedCard, arg);
            }
            catch (TriggerResultException)
            {
                throw new NotImplementedException();
            }
        }

        public bool HandleCardUse(Player p, ISkill skill, List<Card> cards)
        {
            CardsMovement m;
            ICard result;
            m.cards = cards;
            m.to = new DeckPlace(null, DeckType.Discard);
            bool status = CommitCardTransform(p, skill, cards, out result);
            if (!status)
            {
                return false;
            }
            MoveCards(m, new CardUseLog() { Source = p, Targets = null, Cards = null, Skill = skill });
            PlayerPlayedCard(p, result);
            return true;
        }

        public bool CommitCardTransform(Player p, ISkill skill, List<Card> cards, out ICard result)
        {
            if (skill != null)
            {
                CompositeCard card;
                CardTransformSkill s = (CardTransformSkill)skill;                
                if (!s.Transform(cards, null, out card))
                {
                    result = null;
                    return false;
                }
                result = card;
            }
            else
            {
                result = cards[0];
            }
            return true;
        }
    }
}
