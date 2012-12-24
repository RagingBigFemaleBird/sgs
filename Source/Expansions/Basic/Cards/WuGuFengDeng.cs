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
        Dictionary<Card, Card> fakeMapping;
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
                get { return new UiHelper() { RevealCards = !noCardReveal }; }
            }
        }

        protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard)
        {
            DeckType wuguDeck = new DeckType("WuGu");
            DeckType wuguFakeDeck = new DeckType("WuGuFake");
            List<List<Card>> answer;
            if (!Game.CurrentGame.UiProxies[dest].AskForCardChoice(new CardChoicePrompt("WuGuFengDeng"),
                    new List<DeckPlace>() { new DeckPlace(null, wuguFakeDeck) },
                    new List<string>() { "WuGu" },
                    new List<int>() { 1 },
                    new WuGuCardChoiceVerifier(fakeMapping),
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
                if (!fakeMapping.ContainsKey(answer[0][0]) || fakeMapping[answer[0][0]] == null)
                {
                    answer[0] = new List<Card>() { Game.CurrentGame.Decks[null, wuguDeck][0] };
                }
                var theCard = answer[0][0];
                answer[0] = new List<Card>() { fakeMapping[theCard] };
                fakeMapping[theCard] = null;
            }
            Game.CurrentGame.HandleCardTransferToHand(null, dest, answer[0]);
        }

        public override void Process(Player source, List<Player> dests, ICard card, ReadOnlyCard readonlyCard)
        {
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
            Game.CurrentGame.MoveCards(move);
            fakeMapping = new Dictionary<Card, Card>();
            Game.CurrentGame.Decks[null, wuguFakeDeck].Clear();
            foreach (var c in Game.CurrentGame.Decks[null, wuguDeck])
            {
                var faked = new Card(c);
                faked.Place = new DeckPlace(null, wuguFakeDeck);
                Game.CurrentGame.Decks[null, wuguFakeDeck].Add(faked);
                fakeMapping.Add(faked, c);
            }
            Game.CurrentGame.NotificationProxy.NotifyWuGuStart(new DeckPlace(null, wuguFakeDeck));
            base.Process(source, dests, card, readonlyCard);
            Game.CurrentGame.NotificationProxy.NotifyWuGuEnd();
            Game.CurrentGame.Decks[null, wuguFakeDeck].Clear();
            if (Game.CurrentGame.Decks[null, wuguDeck].Count > 0)
            {
                move = new CardsMovement();
                move.Cards = new List<Card>(Game.CurrentGame.Decks[null, wuguDeck]);
                move.To = new DeckPlace(null, DeckType.Discard);
                Game.CurrentGame.MoveCards(move);
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
}
