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
    public class JueDou : CardHandler
    {
        public class JueDouCardChoiceVerifier : ICardUsageVerifier
        {
            public VerifierResult Verify(ISkill skill, List<Card> cards, List<Player> players)
            {
                if (cards == null || cards.Count != 1 || (players != null && players.Count != 0))
                {
                    return VerifierResult.Fail;
                }
                if (skill != null)
                {
                    CompositeCard card;
                    CardTransformSkill s = (CardTransformSkill)skill;
                    VerifierResult r = s.Transform(cards, null, out card);
                    if (!(card.Type is Sha))
                    {
                        return VerifierResult.Fail;
                    }
                    return VerifierResult.Success;
                }
                if (cards[0].Place.DeckType != DeckType.Hand)
                {
                    return VerifierResult.Fail;
                }
                if (!(cards[0].Type is Sha))
                {
                    return VerifierResult.Fail;
                }
                return VerifierResult.Success;
            }
        }

        protected override void Process(Player source, Player dest)
        {
            Player current = dest;
            while (true)
            {
                IUiProxy ui = Game.CurrentGame.UiProxies[current];
                JueDouCardChoiceVerifier v1 = new JueDouCardChoiceVerifier();
                ISkill skill;
                List<Player> p;
                List<Card> cards;
                if (!ui.AskForCardUsage("JueDou", v1, out skill, out cards, out p))
                {
                    Trace.TraceInformation("Player {0} Invalid answer", current);
                    break;
                }
                CardsMovement m;
                m.cards = cards;
                m.to = new DeckPlace(null, DeckType.Discard);
                if (skill != null)
                {
                    CompositeCard card;
                    CardTransformSkill s = (CardTransformSkill)skill;
                    VerifierResult r = s.Transform(cards, null, out card);
                    Trace.Assert(r == VerifierResult.Success);
                    if (!s.Commit(cards, null))
                    {
                        continue;
                    }
                }
                Game.CurrentGame.MoveCards(m);
                Trace.TraceInformation("Player {0} SHA, ", current.Id);
                if (current == dest)
                {
                    current = source;
                }
                else
                {
                    current = dest;
                }
            }
            Player won = current == dest ? source : dest;
            Game.CurrentGame.DoDamage(won, current, 1, DamageElement.Fire, Game.CurrentGame.Decks[DeckType.Compute]);
        }

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            if (targets == null || targets.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (targets.Count > 1)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public override CardCategory Category
        {
            get { return CardCategory.ImmediateTool; }
        }
    }
}
