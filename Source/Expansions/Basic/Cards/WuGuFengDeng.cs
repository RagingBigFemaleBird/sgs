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
        protected override void Process(Player source, Player dest, ICard card)
        {
            throw new NotImplementedException();
        }

        public override void Process(Player source, List<Player> dests, ICard card)
        {
            DeckType wuguDeck = new DeckType("WuGu");
            Trace.Assert(dests == null || dests.Count == 0);
            List<Player> toProcess = new List<Player>(Game.CurrentGame.AlivePlayers);
            Game.CurrentGame.SortByOrderOfComputation(source, toProcess);
            CardsMovement move = new CardsMovement();
            move.cards = new List<Card>();
            for (int i = 0; i < toProcess.Count; i++)
            {
                Game.CurrentGame.SyncCardAll(Game.CurrentGame.PeekCard(0));
                Card c = Game.CurrentGame.DrawCard();
                move.cards.Add(c);
            }
            move.to = new DeckPlace(null, wuguDeck);
            Game.CurrentGame.MoveCards(move, null);
            foreach (Player player in toProcess)
            {
                Player current = player;
                if (!PlayerIsCardTargetCheck(ref source, ref current, card))
                {
                    continue;
                }
                List<List<Card>> answer;
                if (!Game.CurrentGame.UiProxies[player].AskForCardChoice(new CardChoicePrompt("WuGuFengDeng"),
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

                Game.CurrentGame.HandleCardTransferToHand(null, player, answer[0]);
            }
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

        protected override List<Player> LogTargetsModifier(Player source, List<Player> dests)
        {
            var z = new List<Player>(Game.CurrentGame.AlivePlayers);
            return z;
        }
    }
}
