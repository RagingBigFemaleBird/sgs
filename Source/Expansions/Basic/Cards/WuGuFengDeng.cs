using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;

namespace Sanguosha.Expansions.Basic.Cards
{
    
    public class WuGuFengDeng : CardHandler
    {
        public Dictionary<Card, Card> FakeMapping { get; set; }
        public class WuGuCardChoiceVerifier : ICardChoiceVerifier
        {
            bool noCardReveal;
            Dictionary<Card, Card> fakeMapping;
            public WuGuCardChoiceVerifier(Dictionary<Card, Card> mapping)
            {
                noCardReveal = false;
                fakeMapping = mapping;
            }

            public VerifierResult Verify(List<List<Card>> answer)
            {
                if ((answer.Count > 1) || (answer.Count > 0 && answer[0].Count > 1))
                {
                    return VerifierResult.Fail;
                }
                if (answer.Count == 0 || answer[0].Count == 0)
                {
                    return VerifierResult.Partial;
                }
                if (fakeMapping[answer[0][0]] == null || !fakeMapping.ContainsKey(answer[0][0]))
                {
                    return VerifierResult.Fail;
                }
                return VerifierResult.Success;
            }
            public UiHelper Helper
            {
                get { return new UiHelper() { RevealCards = !noCardReveal, ShowToAll = true}; }
            }
        }

        protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs inResponseTo)
        {
            DeckType wuguDeck = new DeckType("WuGu");
            DeckType wuguFakeDeck = new DeckType("WuGuFake");
            List<List<Card>> answer;
            if (!Game.CurrentGame.UiProxies[dest].AskForCardChoice(new CardChoicePrompt("WuGuFengDeng"),
                    new List<DeckPlace>() { new DeckPlace(null, wuguFakeDeck) },
                    new List<string>() { "WuGu" },
                    new List<int>() { 1 },
                    new WuGuCardChoiceVerifier(FakeMapping),
                    out answer,
                    new AdditionalCardChoiceOptions() { IsWuGu = true }))
            {
                Trace.TraceInformation("Invalid answer for WuGu, choosing for you");
                answer = new List<List<Card>>();
                answer.Add(new List<Card>());
                answer[0].Add(Game.CurrentGame.Decks[null, wuguDeck][0]);
            }
            else
            {
                if (!FakeMapping.ContainsKey(answer[0][0]) || FakeMapping[answer[0][0]] == null)
                {
                    answer[0] = new List<Card>() { Game.CurrentGame.Decks[null, wuguDeck][0] };
                }
                var theCard = answer[0][0];
                answer[0] = new List<Card>() { FakeMapping[theCard] };
                FakeMapping[theCard] = null;
            }
            Game.CurrentGame.HandleCardTransferToHand(null, dest, answer[0], new MovementHelper() { IsWuGu = true });
        }

        public override void Process(GameEventArgs handlerArgs)
        {
            base.Process(handlerArgs);
            DeckType wuguDeck = new DeckType("WuGu");
            DeckType wuguFakeDeck = new DeckType("WuGuFake");
            CardsMovement move = new CardsMovement();
            Game.CurrentGame.NotificationProxy.NotifyWuGuEnd();
            Game.CurrentGame.Decks[null, wuguFakeDeck].Clear();
            if (Game.CurrentGame.Decks[null, wuguDeck].Count > 0)
            {
                move = new CardsMovement();
                move.Cards = new List<Card>(Game.CurrentGame.Decks[null, wuguDeck]);
                move.To = new DeckPlace(null, DeckType.Discard);
                Game.CurrentGame.MoveCards(move, false, Core.Utils.GameDelayTypes.Draw);
            }
        }

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            if (targets != null && targets.Count >= 1)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public override CardCategory Category
        {
            get { return CardCategory.ImmediateTool; }
        }

        public override List<Player> ActualTargets(Player source, List<Player> dests, ICard card)
        {
            var z = new List<Player>(Game.CurrentGame.AlivePlayers);
            return z;
        }
    }

    class WuGuFengDengTrigger : Trigger
    {
        public WuGuFengDengTrigger()
        {
            Type = TriggerType.Card;
        }
        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            var card = eventArgs.Card;
            if (!(card.Type is WuGuFengDeng)) return;
            var wugu = card.Type as WuGuFengDeng;
            var dests = eventArgs.Targets;
            DeckType wuguDeck = new DeckType("WuGu");
            DeckType wuguFakeDeck = new DeckType("WuGuFake");
            CardsMovement move = new CardsMovement();
            move.Cards = new List<Card>();
            for (int i = 0; i < dests.Count; i++)
            {
                Game.CurrentGame.SyncImmutableCardAll(Game.CurrentGame.PeekCard(0));
                Card c = Game.CurrentGame.DrawCard();
                move.Cards.Add(c);
            }
            move.To = new DeckPlace(null, wuguDeck);
            Game.CurrentGame.MoveCards(move, false, Core.Utils.GameDelayTypes.None);
            wugu.FakeMapping = new Dictionary<Card, Card>();
            Game.CurrentGame.Decks[null, wuguFakeDeck].Clear();
            foreach (var c in Game.CurrentGame.Decks[null, wuguDeck])
            {
                var faked = new Card(c);
                faked.Place = new DeckPlace(null, wuguFakeDeck);
                Game.CurrentGame.Decks[null, wuguFakeDeck].Add(faked);
                wugu.FakeMapping.Add(faked, c);
            }
            Game.CurrentGame.NotificationProxy.NotifyWuGuStart(new CardChoicePrompt("WuGuFengDeng.Init"), new DeckPlace(null, wuguFakeDeck));
        }
    }
}
