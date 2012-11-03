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
        protected override void Process(Player source, Player dest, ICard card)
        {
            Player current = dest;
            bool firstTime = true;
            while (true)
            {
                List<Player> sourceList = new List<Player>();
                if (current == dest)
                {
                    sourceList.Add(source);
                }
                else
                {
                    sourceList.Add(dest);
                }
                IUiProxy ui = Game.CurrentGame.UiProxies[current];
                SingleCardUsageVerifier v1 = new SingleCardUsageVerifier((c) => { return c.Type is Sha; });
                ISkill skill;
                List<Player> p;
                List<Card> cards;
                CardUsagePrompt prompt;
                if (current.IsDead) return;
                if (firstTime)
                {
                    prompt = new CardUsagePrompt("JueDou", source);;
                }
                else
                {
                    prompt = new CardUsagePrompt("JueDou2", current == dest ? source : dest);
                    firstTime = false;
                }
                if (!ui.AskForCardUsage(prompt, v1, out skill, out cards, out p))
                {
                    Trace.TraceInformation("Player {0} Invalid answer", current);
                    break;
                }
                if (!Game.CurrentGame.HandleCardPlay(current, skill, cards, sourceList))
                {
                    continue;
                }
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
            Game.CurrentGame.DoDamage(won, current, 1, DamageElement.Fire, card);
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
            if (targets[0] == source)
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
