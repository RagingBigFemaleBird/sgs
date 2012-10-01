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
    public abstract class Aoe : CardHandler
    {

        private SingleCardUsageVerifier responseCardVerifier;

        protected SingleCardUsageVerifier ResponseCardVerifier
        {
            get { return responseCardVerifier; }
            set { responseCardVerifier = value; }
        }

        protected abstract string UsagePrompt {get;}

        protected override void Process(Player source, Player dest)
        {
            throw new NotImplementedException();
        }

        public override void Process(Player source, List<Player> dests)
        {
            Trace.Assert(dests == null || dests.Count == 0);
            Player current = source;
            SingleCardUsageVerifier v1 = responseCardVerifier;
            do
            {
                current = Game.CurrentGame.NextPlayer(current);
                while (true)
                {
                    IUiProxy ui = Game.CurrentGame.UiProxies[current];
                    ISkill skill;
                    List<Player> p;
                    List<Card> cards;
                    if (!ui.AskForCardUsage(UsagePrompt, v1, out skill, out cards, out p))
                    {
                        Trace.TraceInformation("Player {0} Invalid answer", current);
                        Game.CurrentGame.DoDamage(source, current, 1, DamageElement.None, Game.CurrentGame.Decks[DeckType.Compute]);
                    }
                    else
                    {
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
                        Trace.TraceInformation("Player {0} Responded. ", current.Id);
                    }
                    break;
                }

            } while (current != source);
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
    }
}
