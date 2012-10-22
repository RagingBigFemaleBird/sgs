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

        List<DelayedTriggerRegistration> triggersToRegister;

        public Game()
        {
            cardSet = new List<Card>();
            triggers = new Dictionary<GameEvent, List<Trigger>>();
            decks = new DeckContainer();
            players = new List<Player>();
            cardHandlers = new Dictionary<string, CardHandler>();
            uiProxies = new Dictionary<Player, IUiProxy>();
            currentActingPlayer = null;
            triggersToRegister = new List<DelayedTriggerRegistration>();
        }

        public void LoadExpansion(Expansion expansion)
        {
            cardSet.AddRange(expansion.CardSet);
            if (expansion.TriggerRegistration != null)
            {
                triggersToRegister.AddRange(expansion.TriggerRegistration);
            }
        }

        public Network.Server GameServer { get; set; }
        public Network.Client GameClient { get; set; }


        public void SyncCard(Player player, Card card)
        {
            if (GameClient != null)
            {
                if (player.Id != GameClient.SelfId)
                {
                    return;
                }
                GameClient.Receive();
            }
            else if (GameServer != null)
            {
                card.RevealOnce = true;
                GameServer.SendObject(player.Id, card);
            }
        }

        public void SyncCards(Player player, List<Card> cards)
        {
            foreach (Card c in cards)
            {
                SyncCard(player, c);
            }
        }

        public void SyncCardAll(Card card)
        {
            foreach (Player p in players)
            {
                SyncCard(p, card);
            }
        }

        public void SyncCardsAll(List<Card> cards)
        {
            foreach (Player p in players)
            {
                SyncCards(p, cards);
            }
        }

        public void SyncConfirmationStatus(ref bool confirmed)
        {
            if (GameServer != null)
            {
                for (int i = 0; i < GameServer.MaxClients; i++)
                {
                    GameServer.SendObject(i, confirmed ? 1 : 0);
                }
            }
            else if (GameClient != null)
            {
                object o = GameClient.Receive();
                Trace.Assert(o is int);
                if ((int)o == 1)
                {
                    confirmed = true;
                }
                else
                {
                    confirmed = false;
                }
            }
        }

        public bool IsSlave { get; set; }
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
            if (GameServer != null)
            {
                GameServer.Ready();
            }
            int id = 0;
            int serial = 0;
            foreach (var g in cardSet)
            {
                g.Id = serial;
                if (id < players.Count && g.Type is Core.Heroes.HeroCardHandler)
                {
                    Core.Heroes.HeroCardHandler h = (Core.Heroes.HeroCardHandler)g.Type;
                    Trace.TraceInformation("Assign {0} to player {1}", h.Hero.Name, id);
                    Game.CurrentGame.Players[id].Hero = h.Hero;
                    Game.CurrentGame.Players[id].Allegiance = h.Hero.Allegiance;
                    Game.CurrentGame.Players[id].MaxHealth = Game.CurrentGame.Players[id].Health = h.Hero.MaxHealth;
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
                serial++;
                //todo: hero card go somewhere else
            }
            if (IsSlave)
            {
                slaveCardSet = cardSet;
                cardSet = new List<Card>();
                for (int i = 0; i < slaveCardSet.Count; i++)
                {
                    unknownCard = new Card();
                    unknownCard.Id = Card.UnknownCardId;
                    unknownCard.Rank = 0;
                    unknownCard.Suit = SuitType.None;
                    unknownCard.Type = new UnknownCardHandler();
                    cardSet.Add(unknownCard);
                }
            }
            foreach (var trig in triggersToRegister)
            {
                RegisterTrigger(trig.key, trig.trigger);
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

        public void RegisterCurrentThread()
        {
            games.Add(Thread.CurrentThread, this);
        }

        List<Card> cardSet;

        public List<Card> CardSet
        {
            get { return cardSet; }
            set { cardSet = value; }
        }

        List<Card> slaveCardSet;

        public List<Card> SlaveCardSet
        {
            get { return slaveCardSet; }
            set { slaveCardSet = value; }
        }

        Card unknownCard;
        Dictionary<GameEvent, List<Trigger>> triggers;

        public void RegisterTrigger(GameEvent gameEvent, Trigger trigger)
        {
            if (trigger == null)
            {
                return;
            }
            if (!triggers.ContainsKey(gameEvent))
            {                
                triggers[gameEvent] = new List<Trigger>();
            }
            triggers[gameEvent].Add(trigger);
        }

        public void UnregisterTrigger(GameEvent gameEvent, Trigger trigger)
        {
            if (trigger == null)
            {
                return;
            }
            if (triggers.ContainsKey(gameEvent))
            {
                triggers[gameEvent].Remove(trigger);
            }
        }

        /// <summary>
        /// Emit a game event to invoke associated triggers.
        /// </summary>
        /// <param name="gameEvent">Game event to be emitted.</param>
        /// <param name="eventParam">Additional helper for triggers listening on this game event.</param>
        public void Emit(GameEvent gameEvent, GameEventArgs eventParam)
        {
            if (!this.triggers.ContainsKey(gameEvent)) return;
            List<Trigger> triggers = new List<Trigger>(this.triggers[gameEvent]);
            if (triggers == null) return;
            //todo: sort this
            var sortedTriggers = triggers;
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

        public IGlobalUiProxy GlobalProxy { get; set; }

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

        public void EventlessMoveCards(List<CardsMovement> moves, List<UI.IGameLog> logs)
        {
            foreach (CardsMovement move in moves)
            {
                List<Card> cards = new List<Card>(move.cards);
                foreach (Card card in cards)
                {
                    if (move.to.Player == null && move.to.DeckType == DeckType.Discard)
                    {
                        SyncCardAll(card);
                    }
                    if (card.Place.Player != null && move.to.Player != null && move.to.DeckType == DeckType.Hand)
                    {
                        SyncCard(move.to.Player, card);
                    }
                }
            }

            foreach (var v in uiProxies)
            {
                v.Value.NotifyCardMovement(moves, logs);
            }
            foreach (CardsMovement move in moves)
            {
                List<Card> cards = new List<Card>(move.cards);
                // 注意：在此处绝对不能发出任何trigger. cards movement是一个atomic operation必须全部move完毕才可以trigger
                // 
                // Update card's deck mapping
                foreach (Card card in cards)
                {
                    Trace.TraceInformation("Card {0}{1}{2} from {3}{4} to {5}{6}.", card.Suit, card.Rank, card.Type.CardType.ToString(),
                        card.Place.Player == null ? "G" : card.Place.Player.Id.ToString(), card.Place.DeckType.Name, move.to.Player == null ? "G" : move.to.Player.Id.ToString(), move.to.DeckType.Name);
                    // unregister triggers for equipment 例如武圣将红色的雌雄双绝（假设有这么一个雌雄双绝）打出杀女性角色，不能发动雌雄
                    if (card.Place.Player != null && card.Place.DeckType == DeckType.Equipment && CardCategoryManager.IsCardCategory(card.Type.Category, CardCategory.Equipment))
                    {
                        Equipment e = card.Type as Equipment;
                        e.UnregisterTriggers(card.Place.Player);
                    }
                    if (move.to.Player != null && move.to.DeckType == DeckType.Equipment && CardCategoryManager.IsCardCategory(card.Type.Category, CardCategory.Equipment))
                    {
                        Equipment e = card.Type as Equipment;
                        e.RegisterTriggers(move.to.Player);
                    }
                    decks[card.Place].Remove(card);
                    decks[move.to].Add(card);
                    card.Place = move.to;
                }
            }
        }

        public void MoveCards(List<CardsMovement> moves, List<UI.IGameLog> logs)
        {
            // trigger entering discard here
            EventlessMoveCards(moves, logs);
            // trigger everything else here
        }

        public void MoveCards(CardsMovement move, UI.IGameLog log)
        {
            List<CardsMovement> moves = new List<CardsMovement>();
            moves.Add(move);
            List<UI.IGameLog> logs = new List<IGameLog>();
            MoveCards(moves, logs);
        }

        public Card PeekCard(int i)
        {
            var drawDeck = decks[DeckType.Dealing];
            if (i >= drawDeck.Count)
            {
                Emit(GameEvent.Shuffle, new GameEventArgs() { Game = this });
            }
            if (drawDeck.Count == 0)
            {
                throw new GameOverException();
            }
            return drawDeck[i];
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
                    SyncCard(player, PeekCard(0));
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

        Player currentActingPlayer;

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>UI ONLY!</remarks>
        public Player CurrentActingPlayer
        {
            get { return currentActingPlayer; }
            set
            {
                if (currentActingPlayer == value) return;
                currentActingPlayer = value;
                OnPropertyChanged("CurrentActingPlayer");
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
            //todo: move card to judge area and remove this
            SyncCardAll(c);
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

        public bool HandleCardUse(Player p, ISkill skill, List<Card> cards, List<Player> targets)
        {
            Trace.Assert(cards != null);
            CardsMovement m;
            ICard result;
            m.cards = cards;
            m.to = new DeckPlace(null, DeckType.Discard);
            bool status = CommitCardTransform(p, skill, cards, out result, targets);
            if (!status)
            {
                return false;
            }
            if (skill != null)
            {
                var r = result as CompositeCard;
                Trace.Assert(r != null);
                cards.Clear();
                cards.AddRange(r.Subcards);
            }
            MoveCards(m, new CardUseLog() { Source = p, Targets = null, Cards = null, Skill = skill });
            PlayerPlayedCard(p, result);
            return true;
        }

        public bool CommitCardTransform(Player p, ISkill skill, List<Card> cards, out ICard result, List<Player> targets)
        {
            if (skill != null)
            {
                CompositeCard card;
                CardTransformSkill s = (CardTransformSkill)skill;                
                if (!s.Transform(cards, null, out card, targets))
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

        public bool PlayerCanDiscardCard(Player p, Card c)
        {
            GameEventArgs arg = new GameEventArgs();
            arg.Source = p;
            arg.Card = c;
            try
            {
                Game.CurrentGame.Emit(GameEvent.PlayerCanDiscardCard, arg);
            }
            catch (TriggerResultException e)
            {
                if (e.Status == TriggerResult.Fail)
                {
                    Trace.TraceInformation("Player {0} cannot discard {1}", p.Id, c.Type.CardType);
                    return false;
                }
                else
                {
                    Trace.Assert(false);
                }
            }
            return true;
        }

        public bool PlayerCanUseCard(Player p, ICard c)
        {
            GameEventArgs arg = new GameEventArgs();
            arg.Source = p;
            arg.Card = c;
            try
            {
                Game.CurrentGame.Emit(GameEvent.PlayerCanUseCard, arg);
            }
            catch (TriggerResultException e)
            {
                if (e.Status == TriggerResult.Fail)
                {
                    Trace.TraceInformation("Player {0} cannot use {1}", p.Id, c.Type.CardType);
                    return false;
                }
                else
                {
                    Trace.Assert(false);
                }
            }
            return true;
        }

        public bool PlayerCanDiscardCards(Player p, List<Card> cards)
        {
            foreach (Card c in cards)
            {
                if (!PlayerCanDiscardCard(p, c))
                {
                    return false;
                }
            }
            return true;
        }

        public bool PlayerCanBeTargeted(Player source, List<Player> targets, ICard card)
        {
            GameEventArgs arg = new GameEventArgs();
            arg.Source = source;
            arg.Targets = targets;
            arg.Card = card;
            try
            {
                Game.CurrentGame.Emit(GameEvent.PlayerCanBeTargeted, arg);
                return true;
            }
            catch (TriggerResultException e)
            {
                if (e.Status == TriggerResult.Fail)
                {
                    Trace.TraceInformation("Players cannot be targeted by {0}", card.Type.CardType);
                    return false;
                }
                else
                {
                    Trace.Assert(false);
                }
            }
            return true;
        }
    }
}
