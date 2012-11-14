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
    [Serializable]
    public class WuGuFengDeng : CardHandler
    {
        protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard)
        {
            DeckType wuguDeck = new DeckType("WuGu");
            List<List<Card>> answer;
            if (!Game.CurrentGame.UiProxies[dest].AskForCardChoice(new CardChoicePrompt("WuGuFengDeng"),
                    new List<DeckPlace>() { new DeckPlace(null, wuguDeck) },
                    new List<string>() { "WuGu" },
                    new List<int>() { 1 },
                    new RequireOneCardChoiceVerifier(),
                    out answer,
                    new List<bool>() { false }))
            {
                Trace.TraceInformation("Invalid answer for WuGu, choosing for you");
                answer = new List<List<Card>>();
                answer.Add(new List<Card>());
                answer[0].Add(Game.CurrentGame.Decks[null, wuguDeck][0]);
            }

            Game.CurrentGame.HandleCardTransferToHand(null, dest, answer[0]);
        }

        public override void Process(Player source, List<Player> dests, ICard card, ReadOnlyCard readonlyCard)
        {
            DeckType wuguDeck = new DeckType("WuGu");
            CardsMovement move = new CardsMovement();
            move.cards = new List<Card>();
            for (int i = 0; i < dests.Count; i++)
            {
                Game.CurrentGame.SyncCardAll(Game.CurrentGame.PeekCard(0));
                Card c = Game.CurrentGame.DrawCard();
                move.cards.Add(c);
            }
            move.to = new DeckPlace(null, wuguDeck);
            Game.CurrentGame.MoveCards(move, null);
            base.Process(source, dests, card, readonlyCard);
            if (Game.CurrentGame.Decks[null, wuguDeck].Count > 0)
            {
                move = new CardsMovement();
                move.cards = new List<Card>(Game.CurrentGame.Decks[null, wuguDeck]);
                move.to = new DeckPlace(null, DeckType.Discard);
                Game.CurrentGame.MoveCards(move, null);
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

        public override List<Player> ActualTargets(Player source, List<Player> dests)
        {
            var z = new List<Player>(Game.CurrentGame.AlivePlayers);
            return z;
        }
    }
}
