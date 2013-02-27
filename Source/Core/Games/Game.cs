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
using Sanguosha.Lobby.Core;
using Sanguosha.Core.Utils;


namespace Sanguosha.Core.Games
{

    public class GameOverException : SgsException { }

    public class CardsMovement
    {
        public CardsMovement()
        {
            cards = new List<Card>();
            helper = new MovementHelper();
        }

        private List<Card> cards;

        public List<Card> Cards
        {
            get { return cards; }
            set { cards = value; }
        }

        private DeckPlace to;

        public DeckPlace To
        {
            get { return to; }
            set { to = value; }
        }

        private MovementHelper helper;

        public MovementHelper Helper
        {
            get { return helper; }
            set { helper = value; }
        }

    }

    public enum DamageElement
    {
        None,
        Fire,
        Lightning,
    }

    public enum DiscardReason
    {
        Discard,
        Play,
        Use,
        Judge,
    }

    public abstract partial class Game : INotifyPropertyChanged
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


        class EndOfDealingDeckException : SgsException { }


        class GameAlreadyStartedException : SgsException { }

        public GameSettings Settings { get; set; }

        public AccountConfiguration Configuration { get; set; }

        static Game()
        {
            games = new Dictionary<Thread, Game>();
        }

        List<CardHandler> availableCards;

        public List<CardHandler> AvailableCards
        {
            get { return availableCards; }
        }


        List<DelayedTriggerRegistration> triggersToRegister;

        private Dictionary<Player, List<Player>> handCardVisibility;

        public Dictionary<Player, List<Player>> HandCardVisibility
        {
            get { return handCardVisibility; }
            set { handCardVisibility = value; }
        }

        public Game()
        {
            cardSet = new List<Card>();
            originalCardSet = new List<Card>();
            triggers = new Dictionary<GameEvent, List<Trigger>>();
            decks = new DeckContainer();
            players = new List<Player>();
            cardHandlers = new Dictionary<string, CardHandler>();
            uiProxies = new Dictionary<Player, IUiProxy>();
            triggersToRegister = new List<DelayedTriggerRegistration>();
            isDying = new Stack<Player>();
            handCardVisibility = new Dictionary<Player, List<Player>>();
            Settings = new GameSettings();
            cleanupSquad = new CleanupSquad();
            cleanupSquad.Priority = -1;
        }

        public void LoadExpansion(Expansion expansion)
        {
            OriginalCardSet.AddRange(expansion.CardSet);

            if (expansion.TriggerRegistration != null)
            {
                triggersToRegister.AddRange(expansion.TriggerRegistration);
            }
        }

        protected CleanupSquad cleanupSquad;

        public Network.Server GameServer { get; set; }
        public Network.Client GameClient { get; set; }

        public ReplayController ReplayController
        {
            get
            {
                if (GameClient == null) return null;
                return GameClient.ReplayController;
            }
        }

        public void SyncUnknownLocationCard(Player player, Card card)
        {
            if (GameClient != null)
            {
                bool confirmed = true;
                Game.CurrentGame.SyncConfirmationStatus(ref confirmed);
                if (confirmed)
                {
                    int id = player != null ? player.Id : Game.CurrentGame.Players.Count;
                    if (id != GameClient.SelfId)
                    {
                        return;
                    }
                    GameClient.Receive();
                }
            }
            else if (GameServer != null)
            {
                bool status = true;
                if (card.Place.DeckType == DeckType.Equipment || card.Place.DeckType == DeckType.DelayedTools)
                {
                    status = false;
                }
                Game.CurrentGame.SyncConfirmationStatus(ref status);
                if (status)
                {
                    card.RevealOnce = true;
                    if (player == null) GameServer.SendObject(GameServer.MaxClients - 1, card);
                    else GameServer.SendObject(player.Id, card);
                    if (player == null) GameServer.Flush(GameServer.MaxClients - 1);
                    else GameServer.Flush(player.Id);
                }
            }
        }
        public void SyncUnknownLocationCardAll(Card card)
        {
            foreach (Player p in players)
            {
                SyncUnknownLocationCard(p, card);
            }
            SyncUnknownLocationCard(null, card);
        }

        public void SyncCard(Player player, ref Card card)
        {
            var cards = new List<Card>() { card };
            SyncCards(player, cards);
            card = cards[0];
        }

        private void SyncCards(Player player, List<Card> cards)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                Card card = cards[i];
                if (card.Place.DeckType == DeckType.Equipment || card.Place.DeckType == DeckType.DelayedTools)
                {
                    continue;
                }
                if (GameClient != null)
                {
                    int id = player != null ? player.Id : Game.CurrentGame.Players.Count;
                    if (id != GameClient.SelfId)
                    {
                        return;
                    }
                    var recv = GameClient.Receive();
                    Trace.Assert(recv is Card);
                    card = recv as Card;
                }
                else if (GameServer != null)
                {
                    card.RevealOnce = true;
                    if (player == null) GameServer.SendObject(GameServer.MaxClients - 1, card);
                    else GameServer.SendObject(player.Id, card);
                }
                cards[i] = card;
            }
            if (GameServer != null)
            {
                if (player == null) GameServer.Flush(GameServer.MaxClients - 1);
                else GameServer.Flush(player.Id);
            }
        }

        public void SyncCardAll(ref Card card)
        {
            foreach (Player p in players)
            {
                SyncCard(p, ref card);
            }
            SyncCard(null, ref card);
        }

        private void SyncCardsAll(List<Card> cards)
        {
            foreach (Player p in players)
            {
                SyncCards(p, cards);
            }
            SyncCards(null, cards);
        }

        public void SyncImmutableCard(Player player, Card card)
        {
            SyncImmutableCards(player, new List<Card>() { card });
        }

        public void SyncImmutableCards(Player player, List<Card> cards)
        {
            foreach (var card in cards)
            {
                if (card.Place.DeckType == DeckType.Equipment || card.Place.DeckType == DeckType.DelayedTools)
                {
                    return;
                }
                if (GameClient != null)
                {
                    int id = player != null ? player.Id : Game.CurrentGame.Players.Count;
                    if (id != GameClient.SelfId)
                    {
                        return;
                    }
                    GameClient.Receive();
                }
                else if (GameServer != null)
                {
                    card.RevealOnce = true;
                    if (player == null) GameServer.SendObject(GameServer.MaxClients - 1, card);
                    else GameServer.SendObject(player.Id, card);
                }
            }
            if (GameServer != null)
            {
                if (player == null) GameServer.Flush(GameServer.MaxClients - 1);
                else GameServer.Flush(player.Id);
            }
        }

        public void SyncImmutableCardAll(Card card)
        {
            foreach (Player p in players)
            {
                SyncImmutableCard(p, card);
            }
            SyncImmutableCard(null, card);
        }

        public void SyncImmutableCardsAll(List<Card> cards)
        {
            foreach (Player p in players)
            {
                SyncImmutableCards(p, cards);
            }
            SyncImmutableCards(null, cards);
        }

        public void SyncConfirmationStatus(ref bool confirmed)
        {
            if (GameServer != null)
            {
                for (int i = 0; i < GameServer.MaxClients; i++)
                {
                    GameServer.SendObject(i, confirmed ? 1 : 0);
                    GameServer.Flush(i);
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

        public bool IsClient
        {
            get
            {
                return GameClient != null;
            }
        }

        public void Abort()
        {
            if (mainThread == null) return;
            Trace.Assert(mainThread != Thread.CurrentThread);            
            if (GlobalProxy != null) GlobalProxy.Abort();
            mainThread.Abort();
            mainThread = null;
        }

        Thread mainThread;

        public virtual void Run()
        {
            mainThread = Thread.CurrentThread;
            if (!games.ContainsKey(Thread.CurrentThread))
            {
                /*throw new GameAlreadyStartedException();
            }
            else
            {*/
                games.Add(Thread.CurrentThread, this);
            }
            if (GameServer != null)
            {
                GameServer.Start();
                Trace.Assert(Settings != null);
                for (int i = 0; i < GameServer.MaxClients; i++)
                {
                    GameServer.SendObject(i, Settings);
                    GameServer.SendObject(i, i);
                    GameServer.Flush(i);
                }

            }

            availableCards = new List<CardHandler>();
            foreach (Card c in OriginalCardSet)
            {
                bool typeCheck = false;
                foreach (var type in availableCards)
                {
                    if (type.GetType().Name.Equals(c.Type.GetType().Name))
                    {
                        typeCheck = true;
                        break;
                    }
                }
                if (!typeCheck)
                {
                    availableCards.Add(c.Type);
                }
            }

            foreach (var card in OriginalCardSet)
            {
                //you are client. everything is unknown
                if (IsClient)
                {
                    unknownCard = new Card();
                    unknownCard.Id = Card.UnknownCardId;
                    unknownCard.Rank = 0;
                    unknownCard.Suit = SuitType.None;
                    if (card.Type is Heroes.HeroCardHandler)
                    {
                        unknownCard.Type = new Heroes.UnknownHeroCardHandler();
                        unknownCard.Id = (card.Type as Heroes.HeroCardHandler).Hero.IsSpecialHero ? Card.UnknownSPHeroId : Card.UnknownHeroId;
                    }
                    else if (card.Type is RoleCardHandler)
                    {
                        unknownCard.Id = Card.UnknownRoleId;
                        unknownCard.Type = card.Type;
                    }
                    else
                    {
                        unknownCard.Type = new UnknownCardHandler();
                    }
                }
                //you are server.
                else
                {
                    unknownCard = new Card();
                    unknownCard.CopyFrom(card);
                    if (unknownCard.Type is CardHandler)
                    {
                        unknownCard.Type = (CardHandler)(unknownCard.Type as CardHandler).Clone();
                    }
                }
                cardSet.Add(unknownCard);
            }

            foreach (var trig in triggersToRegister)
            {
                RegisterTrigger(trig.key, trig.trigger);
            }

            InitTriggers();
            try
            {
                Emit(GameEvent.GameStart, new GameEventArgs());
            }
            catch (GameOverException)
            {

                var keys = new List<Thread>(from t in games.Keys where games[t] == this select t);
                foreach (var t in keys)
                {
                    games.Remove(t);
                }

                this.NotificationProxy = null;
                this.uiProxies = null;
            }
            mainThread = null;
/*
            catch (Exception e)
            {
                var keys = new List<Thread>(from t in games.Keys where games[t] == this select t);
                foreach (var t in keys)
                {
                    games.Remove(t);
                }
                this.NotificationProxy = null;
#if TRACE
                while (e.InnerException != null)
                {
                    e = e.InnerException;
                }
                Trace.TraceError(e.StackTrace);
                Trace.Assert(false, e.StackTrace);
#endif
            }
*/
            if (GameServer != null)
            {
                GameServer.Stop();
            }
            else if (GameClient != null)
            {
                GameClient.Stop();
            }
            Trace.TraceInformation("Game exited normally");
        }

        /// <summary>
        /// Initialize triggers at game start time.
        /// </summary>
        protected abstract void InitTriggers();

        /// <summary>
        /// Speed up current game access for client process
        /// </summary>
        public static Game CurrentGameOverride
        {
            get;
            set;
        }

        public static Game CurrentGame
        {
            get 
            {
                return CurrentGameOverride ?? games[Thread.CurrentThread]; 
            }
        }

        /// <summary>
        /// Mapping from a thread to the game it hosts.
        /// </summary>
        static Dictionary<Thread, Game> games;

        public void RegisterCurrentThread()
        {
            if (games.ContainsKey(Thread.CurrentThread))
            {
                games.Remove(Thread.CurrentThread);
            }
            games.Add(Thread.CurrentThread, this);
        }

        List<Card> originalCardSet;

        /// <summary>
        /// All eligible card copied verbatim from the game engine. All cards in this set are known cards.
        /// </summary>
        public List<Card> OriginalCardSet
        {
            get { return originalCardSet; }
        }

        List<Card> cardSet;

        /// <summary>
        /// Current state of all cards used in the game. Some of the cards can be unknown in the client side.
        /// The collection is empty before Run() is called.
        /// </summary>
        public List<Card> CardSet
        {
            get { return cardSet; }
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
                Trace.Assert(triggers[gameEvent].Contains(trigger));
                triggers[gameEvent].Remove(trigger);
            }
        }

        private class TriggerComparer : IComparer<TriggerWithParam>
        {
            public TriggerComparer(Game game)
            {
                this.game = game;
            }
            Game game;
            public int Compare(TriggerWithParam a, TriggerWithParam b)
            {
                int result2 = a.trigger.Type.CompareTo(b.trigger.Type);
                if (result2 != 0)
                {
                    return -result2;
                }
                int result = a.trigger.Priority.CompareTo(b.trigger.Priority);
                if (result != 0)
                {
                    return -result;
                }
                Player p = game.CurrentPlayer;
                int result3 = 0;
                if (a.trigger.Owner != b.trigger.Owner)
                {
                    do
                    {
                        if (p == a.trigger.Owner)
                        {
                            result3 = -1;
                            break;
                        }
                        if (p == b.trigger.Owner)
                        {
                            result3 = 1;
                            break;
                        }
                        p = game.NextAlivePlayer(p);
                    } while (p != game.CurrentPlayer);

                }
                return result3;
            }
        }

        private void EmitTriggers(List<TriggerWithParam> triggers)
        {
            var result = triggers.OrderBy((a) => { return a; }, new TriggerComparer(this));
            foreach (var t in result)
            {
                if (this.triggers[t.gameEvent].Contains(t.trigger) && (t.trigger.Owner == null || !t.trigger.Owner.IsDead))
                {
                    t.trigger.Run(t.gameEvent, t.args);
                }
            }
        }


        /// <summary>
        /// Emit a game event to invoke associated triggers.
        /// </summary>
        /// <param name="gameEvent">Game event to be emitted.</param>
        /// <param name="eventParam">Additional helper for triggers listening on this game event.</param>
        public void Emit(GameEvent gameEvent, GameEventArgs eventParam, bool beforeMove = false)
        {
            if (!triggers.ContainsKey(gameEvent)) return;
            List<Trigger> additionalTriggers = new List<Trigger>(triggers[gameEvent]);
            List<Trigger> oldTriggers = new List<Trigger>(triggers[gameEvent]);
            while (true)
            {
                if (additionalTriggers == null || additionalTriggers.Count == 0) return;
                List<TriggerWithParam> triggersToRun = new List<TriggerWithParam>();
                foreach (var t in additionalTriggers)
                {
                    if (t.Enabled)
                    {
                        triggersToRun.Add(new TriggerWithParam() { gameEvent = gameEvent, trigger = t, args = eventParam });
                    }
                }
                if (!atomic)
                {
                    EmitTriggers(triggersToRun);
                    additionalTriggers = new List<Trigger>(triggers[gameEvent].Except(oldTriggers));
                    oldTriggers = new List<Trigger>(triggers[gameEvent]);
                    continue;
                }
                else
                {
                    var triggerPlace = atomicTriggers;
                    if (beforeMove)
                    {
                        triggerPlace = atomicTriggersBeforeMove;
                    }
                    triggerPlace.AddRange(triggersToRun);
                }
                break;
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

        private int isUiDetached;
        private int lastUiState;

        public int IsUiDetached
        {
            get { return isUiDetached; }
            set 
            {
                isUiDetached = value;
                if (notificationProxy == null) return;
                if (!((lastUiState == 0) ^ (isUiDetached == 0))) return;
                lastUiState = isUiDetached;
                if (isUiDetached == 0)
                {
                    foreach (var pair in uiProxies)
                    {
                        ClientNetworkUiProxy proxy = pair.Value as ClientNetworkUiProxy;
                        if (proxy != null) proxy.Suppressed = false;
                    }
                    notificationProxy.NotifyUiAttached();
                }
                if (isUiDetached > 0)
                {
                    foreach (var pair in uiProxies)
                    {
                        ClientNetworkUiProxy proxy = pair.Value as ClientNetworkUiProxy;
                        if (proxy != null) proxy.Suppressed = true;
                    }
                    notificationProxy.NotifyUiDetached();
                }
            }
        }

        private INotificationProxy notificationProxy;

        public INotificationProxy NotificationProxy
        {
            get { return notificationProxy; }
            set { notificationProxy = value; }
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

        public List<Player> AlivePlayers
        {
            get
            {
                var list = new List<Player>();
                foreach (Player p in players)
                {
                    if (!p.IsDead)
                    {
                        list.Add(p);
                    }
                }
                SortByOrderOfComputation(currentPlayer, list);
                return list;
            }
        }

        public int NumberOfAliveAllegiances
        {
            get
            {
                var ret =
                (from p in Game.CurrentGame.AlivePlayers select p.Allegiance).Distinct().Count();
                return ret;
            }
        }

        private bool atomic = false;
        private int atomicLevel = 0;

        private struct TriggerWithParam
        {
            public GameEvent gameEvent;
            public Trigger trigger;
            public GameEventArgs args;
        }

        List<CardsMovement> atomicMoves;
        List<TriggerWithParam> atomicTriggers;
        List<TriggerWithParam> atomicTriggersBeforeMove;

        public void EnterAtomicContext()
        {
            atomic = true;
            if (atomicLevel == 0)
            {
                atomicMoves = new List<CardsMovement>();
                atomicTriggers = new List<TriggerWithParam>();
                atomicTriggersBeforeMove = new List<TriggerWithParam>();
            }
            atomicLevel++;
        }

        public void ExitAtomicContext()
        {
            atomicLevel--;
            if (atomicLevel > 0)
            {
                return;
            }
            var moves = atomicMoves;
            var triggers = atomicTriggers;
            var btriggers = atomicTriggersBeforeMove;
            atomic = false;
            EmitTriggers(btriggers);
            MoveCards(moves);
            EmitTriggers(triggers);
        }

        private void AddAtomicMoves(List<CardsMovement> moves)
        {
            int i = 0;
            foreach (var m in moves)
            {
                CardsMovement newM = new CardsMovement();
                newM.Cards = m.Cards;
                newM.To = new DeckPlace(m.To.Player, m.To.DeckType);
                newM.Helper = new MovementHelper(m.Helper);
                atomicMoves.Add(newM);
                i++;
            }
        }

        ///<remarks>
        ///YOU ARE NOT ALLOWED TO TRIGGER ANY EVENT ANYWHERE INSIDE THIS FUNCTION!!!!!
        ///你不可以在这个函数中触发任何事件!!!!!
        ///</remarks>
        public void MoveCards(List<CardsMovement> moves, List<bool> insertBefore = null, GameDelayTypes delay = GameDelayTypes.CardTransfer)
        {
            if (atomic)
            {
                AddAtomicMoves(moves);
                return;
            }
            foreach (CardsMovement move in moves)
            {
                List<Card> cards = new List<Card>(move.Cards);
                foreach (Card card in cards)
                {
                    if (move.To.Player == null && move.To.DeckType == DeckType.Discard)
                    {
                        SyncImmutableCardAll(card);
                    }
                    if (card.Place.Player != null && move.To.Player != null && move.To.DeckType == DeckType.Hand)
                    {
                        SyncImmutableCard(move.To.Player, card);
                    }
                }
            }

            NotificationProxy.NotifyCardMovement(moves);

            int i = 0;
            foreach (CardsMovement move in moves)
            {
                List<Card> cards = new List<Card>(move.Cards);
                // Update card's deck mapping
                foreach (Card card in cards)
                {
                    Trace.TraceInformation("Card {0}{1}{2} from {3}{4} to {5}{6}.", card.Suit, card.Rank, card.Type.CardType.ToString(),
                        card.Place.Player == null ? "G" : card.Place.Player.Id.ToString(), card.Place.DeckType.Name, move.To.Player == null ? "G" : move.To.Player.Id.ToString(), move.To.DeckType.Name);
                    // unregister triggers for equipment 例如武圣将红色的雌雄双绝（假设有这么一个雌雄双绝）打出杀女性角色，不能发动雌雄
                    if (card.Place.Player != null && card.Place.DeckType == DeckType.Equipment && CardCategoryManager.IsCardCategory(card.Type.Category, CardCategory.Equipment))
                    {
                        Equipment e = card.Type as Equipment;
                        e.UnregisterTriggers(card.Place.Player);
                    }
                    if (move.To.Player != null && move.To.DeckType == DeckType.Equipment && CardCategoryManager.IsCardCategory(card.Type.Category, CardCategory.Equipment))
                    {
                        Equipment e = card.Type as Equipment;
                        e.RegisterTriggers(move.To.Player);
                    }
                    decks[card.Place].Remove(card);
                    int isLastHandCard = (card.Place.DeckType == DeckType.Hand && decks[card.Place].Count == 0) ? 1 : 0;
                    if (insertBefore != null && insertBefore[i])
                    {
                        decks[move.To].Insert(0, card);
                    }
                    else
                    {
                        decks[move.To].Add(card);
                    }
                    card.HistoryPlace2 = card.HistoryPlace1;
                    card.HistoryPlace1 = card.Place;
                    card.Place = move.To;
                    //reset card type if entering hand or discard
                    if (!IsClient && (move.To.DeckType == DeckType.Dealing || move.To.DeckType == DeckType.Discard || move.To.DeckType == DeckType.Hand))
                    {
                        card.Log = new ActionLog();
                        _ResetCard(card);
                        if (card.Attributes != null) card.Attributes.Clear();
                    }

                    //reset color if entering delayedtools
                    if (move.To.DeckType == DeckType.DelayedTools)
                    {
                        card.Suit = GameEngine.CardSet[card.Id].Suit;
                    }

                    //reset card type if entering hand or discard
                    if (IsClient && (move.To.DeckType == DeckType.Dealing || move.To.DeckType == DeckType.Discard || move.To.DeckType == DeckType.Hand))
                    {
                        card.Log = new ActionLog();
                        _ResetCard(card);
                        if (card.Attributes != null) card.Attributes.Clear();
                    }
                    card[Card.IsLastHandCard] = isLastHandCard;

                    if (IsClient && (move.To.DeckType == DeckType.Hand && GameClient.SelfId != move.To.Player.Id))
                    {
                        card.Id = -1;
                    }
                    if (move.To.Player != null)
                    {
                        _FilterCard(move.To.Player, card);
                    }
                }
                i++;
            }
            GameDelays.Delay(delay);
        }

        public void MoveCards(CardsMovement move, bool insertBefore = false, GameDelayTypes delay = GameDelayTypes.CardTransfer)
        {
            if (move.Cards.Count == 0) return;
            List<CardsMovement> moves = new List<CardsMovement>();
            moves.Add(move);
            MoveCards(moves, new List<bool>() { insertBefore }, delay);
        }

        public Card PeekCard(int i)
        {
            var drawDeck = decks[DeckType.Dealing];
            if (i >= drawDeck.Count)
            {
                Emit(GameEvent.Shuffle, new GameEventArgs());
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
                Emit(GameEvent.Shuffle, new GameEventArgs());
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
            if (player.IsDead) return;
            List<Card> cardsDrawn = new List<Card>();
            try
            {
                for (int i = 0; i < num; i++)
                {
                    SyncImmutableCard(player, PeekCard(0));
                    cardsDrawn.Add(DrawCard());
                }
            }
            catch (ArgumentException)
            {
                throw new EndOfDealingDeckException();
            }
            CardsMovement move = new CardsMovement();
            move.Cards = cardsDrawn;
            move.To = new DeckPlace(player, DeckType.Hand);
            MoveCards(move, false, GameDelayTypes.Draw);
            PlayerAcquiredCard(player, cardsDrawn);
        }

        Player currentPlayer;

        public Player CurrentPlayer
        {
            get { return currentPlayer; }
            set
            {
                Trace.Assert(value != null);
                if (currentPlayer != null)
                {
                    var temp = new Dictionary<PlayerAttribute, int>(currentPlayer.Attributes);
                    foreach (var pair in temp)
                    {
                        if (pair.Key.AutoReset)
                        {
                            currentPlayer[pair.Key] = 0;
                        }
                    }
                }
                if (currentPlayer == value) return;
                currentPlayer = value;
                OnPropertyChanged("CurrentPlayer");
            }
        }

        //回合结束后，直到下个角色回合开始时这段时间里，不属于任何角色的回合

        public Player PhasesOwner
        {
            get { if (currentPhase == TurnPhase.Inactive) return null; return currentPlayer.IsDead ? null : currentPlayer; }
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

        int currentPhaseEventIndex;

        public int CurrentPhaseEventIndex
        {
            get { return currentPhaseEventIndex; }
            set { currentPhaseEventIndex = value; }
        }

        public static Dictionary<TurnPhase, GameEvent>[] PhaseEvents = new Dictionary<TurnPhase, GameEvent>[]
                         { GameEvent.PhaseBeginEvents, GameEvent.PhaseProceedEvents,
                           GameEvent.PhaseEndEvents, GameEvent.PhaseOutEvents };

        /// <summary>
        /// Get player next to the a player in counter-clock seat map. (must be alive)
        /// </summary>
        /// <param name="p">Player</param>
        /// <returns></returns>
        public virtual Player NextAlivePlayer(Player p)
        {
            p = NextPlayer(p);
            while (p.IsDead)
            {
                p = NextPlayer(p);
            }
            return p;
        }

        /// <summary>
        /// Get player next to the a player in counter-clock seat map. (must be alive)
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
                Trace.Assert(false);
                return null;
            }
            else
            {
                return players[i + 1];
            }
        }

        public virtual int OrderOf(Player withRespectTo, Player target)
        {
            int numPlayers = players.Count;
            int i;
            for (i = 0; i < numPlayers; i++)
            {
                if (players[i] == withRespectTo)
                {
                    break;
                }
            }

            // The next player to the last player is the first player.
            int order = 0;
            while (players[i] != target)
            {
                if (i == numPlayers - 1)
                {
                    i = 0;
                }
                else
                {
                    i = i + 1;
                }
                order++;
            }
            Trace.Assert(order < numPlayers);
            return order;
        }

        public virtual void SortByOrderOfComputation(Player withRespectTo, List<Player> players)
        {
            if (withRespectTo == null) return;
            players.Sort((a, b) =>
                {
                    return OrderOf(withRespectTo, a).CompareTo(OrderOf(withRespectTo, b));
                });
        }


        /// <summary>
        /// Get player previous to the a player in counter-clock seat map. (must be alive)
        /// </summary>
        /// <param name="p">Player</param>
        /// <returns></returns>
        public virtual Player PreviousAlivePlayer(Player p)
        {
            p = PreviousPlayer(p);
            while (p.IsDead)
            {
                p = PreviousPlayer(p);
            }
            return p;
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
            int distRight = from[Player.RangeMinus], distLeft = from[Player.RangeMinus];
            Player p = from;
            while (p != to)
            {
                p = NextAlivePlayer(p);
                distRight++;
            }
            distRight += to[Player.RangePlus];
            p = from;
            while (p != to)
            {
                p = PreviousAlivePlayer(p);
                distLeft++;
            }
            distLeft += to[Player.RangePlus];

            var args = new AdjustmentEventArgs();
            args.Source = from;
            args.Targets = new List<Player>() { to };
            args.AdjustmentAmount = 0;
            Game.CurrentGame.Emit(GameEvent.PlayerDistanceAdjustment, args);
            distLeft += args.AdjustmentAmount;
            distRight += args.AdjustmentAmount;

            var ret = distRight > distLeft ? distLeft : distRight;
            if (ret < 1)
            {
                ret = from == to ? 0 : 1;
            }

            // the minimum distance is 1 between any two. if distance is 0, it means this is the distance to heself or herself.
            // some skills took ignore distance, it means the distance between them is aways 1.
            args = new AdjustmentEventArgs();
            args.Source = from;
            args.Targets = new List<Player>() { to };
            args.AdjustmentAmount = ret;
            Game.CurrentGame.Emit(GameEvent.PlayerDistanceOverride, args);
            ret = args.AdjustmentAmount;

            return ret;
        }

        void _FilterCard(Player p, Card card)
        {
            GameEventArgs args = new GameEventArgs();
            args.Source = p;
            args.Card = card;
            Emit(GameEvent.EnforcedCardTransform, args);
        }

        void _ResetCard(Card card)
        {
            if (card.Id > 0)
            {
                card.Type = (CardHandler)GameEngine.CardSet[card.Id].Type.Clone();
                card.Suit = GameEngine.CardSet[card.Id].Suit;
                card.Rank = GameEngine.CardSet[card.Id].Rank;
            }
        }

        void _ResetCards(Player p)
        {
            foreach (var card in decks[p, DeckType.Hand])
            {
                if (card.Id > 0)
                {
                    _ResetCard(card);
                    _FilterCard(p, card);
                }
            }
        }

        public void NotifyIntermediateJudgeResults(Player player, ActionLog log, JudgementResultSucceed intermDel)
        {
            Trace.Assert(Game.CurrentGame.Decks[player, DeckType.JudgeResult].Count > 0);
            Card c = Game.CurrentGame.Decks[player, DeckType.JudgeResult][0];
            bool? succeed = null;
            if (intermDel != null)
            {
                succeed = intermDel(c);
            }
            Game.CurrentGame.NotificationProxy.NotifyJudge(player, c, log, succeed, false);
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
            if (c.Type is Equipment && (c.Type as Equipment).InUse) return false;
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

        public bool PlayerCanPlayCard(Player p, ICard c)
        {
            GameEventArgs arg = new GameEventArgs();
            arg.Source = p;
            arg.Card = c;
            try
            {
                Game.CurrentGame.Emit(GameEvent.PlayerCanPlayCard, arg);
            }
            catch (TriggerResultException e)
            {
                if (e.Status == TriggerResult.Fail)
                {
                    Trace.TraceInformation("Player {0} cannot play {1}", p.Id, c.Type.CardType);
                    return false;
                }
                else
                {
                    Trace.Assert(false);
                }
            }
            return true;
        }

        Stack<Player> isDying;

        public Stack<Player> DyingPlayers
        {
            get { return isDying; }
            set { isDying = value; }
        }

        public class PlayerHpChanged : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Trace.Assert(eventArgs.Targets.Count == 1);
                Player target = eventArgs.Targets[0];
                if (target.Health > 0 || (eventArgs as HealthChangedEventArgs).Delta > 0) return;
                if (target[Player.SkipDeathComputation] != 0) { target[Player.SkipDeathComputation] = 0; return; }

                Game.CurrentGame.DyingPlayers.Push(target);
                target[Player.IsDying] = 1;
                Trace.TraceInformation("Player {0} dying", target.Id);
                GameEventArgs args = new GameEventArgs();
                args.Source = eventArgs.Source;
                args.Targets = new List<Player>() { target };
                try
                {
                    Game.CurrentGame.Emit(GameEvent.PlayerIsAboutToDie, args);
                }
                catch (TriggerResultException)
                {
                }
                if (target.Health <= 0)
                {
                    try
                    {
                        Game.CurrentGame.Emit(GameEvent.PlayerDying, args);
                    }
                    catch (TriggerResultException)
                    {
                    }
                }
                Trace.Assert(target == Game.CurrentGame.DyingPlayers.Pop());
                target[Player.IsDying] = 0;
                if (target.IsDead || target.Health > 0) return;
                if (target[Player.SkipDeathComputation] != 0) { target[Player.SkipDeathComputation] = 0; return; }
                Trace.TraceInformation("Player {0} dead", target.Id);
                Game.CurrentGame.Emit(GameEvent.GameProcessPlayerIsDead, eventArgs);
            }
        }

        public delegate int NumberOfCardsToForcePlayerDiscard(Player p, int discarded);

        private class PlayerForceDiscardVerifier : ICardUsageVerifier
        {
            public UiHelper Helper { get { return new UiHelper(); } }

            public VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                if (skill != null)
                {
                    return VerifierResult.Fail;
                }
                if (players != null && players.Count > 0)
                {
                    return VerifierResult.Fail;
                }
                if (cards == null || cards.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                foreach (Card c in cards)
                {
                    if (!Game.CurrentGame.PlayerCanDiscardCard(source, c))
                    {
                        return VerifierResult.Fail;
                    }
                    if (!canDiscardEquip && c.Place.DeckType != DeckType.Hand)
                    {
                        return VerifierResult.Fail;
                    }
                }
                if (cards.Count < minimun)
                {
                    return VerifierResult.Partial;
                }
                if (cards.Count > toDiscard)
                {
                    return VerifierResult.Fail;
                }
                return VerifierResult.Success;
            }

            public IList<CardHandler> AcceptableCardTypes
            {
                get { throw new NotImplementedException(); }
            }

            public VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                return FastVerify(source, skill, cards, players);
            }

            int toDiscard;
            bool canDiscardEquip;
            int minimun;
            public PlayerForceDiscardVerifier(int n, bool equip, int min)
            {
                toDiscard = n;
                canDiscardEquip = equip;
                minimun = min;
            }
        }


        public class PinDianVerifier : ICardUsageVerifier
        {
            public VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                if (skill != null || (players != null && players.Count > 0))
                {
                    return VerifierResult.Fail;
                }
                if (cards == null || cards.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                if (cards.Count > 1)
                {
                    return VerifierResult.Fail;
                }
                if (cards[0].Place.DeckType != DeckType.Hand)
                {
                    return VerifierResult.Fail;
                }
                return VerifierResult.Success;
            }

            public IList<CardHandler> AcceptableCardTypes
            {
                get { return null; }
            }

            public VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                return FastVerify(source, skill, cards, players);
            }

            public UiHelper Helper
            {
                get { return new UiHelper(); }
            }
        }

        public void MoveHandCard(Player player, int from, int to)
        {
            if (IsClient && player.Id == GameClient.SelfId)
            {
                GameClient.MoveHandCard(from, to);
            }
        }

        public void DoPlayer(Player p)
        {
            var phase = CurrentPhase;
            var index = CurrentPhaseEventIndex;
            var player = CurrentPlayer;
            CurrentPhaseEventIndex = 0;
            CurrentPhase = TurnPhase.BeforeStart;
            Emit(GameEvent.DoPlayer, new GameEventArgs() { Source = p });
            CurrentPhase = phase;
            CurrentPhaseEventIndex = index;
            CurrentPlayer = player;
        }
    }
}

