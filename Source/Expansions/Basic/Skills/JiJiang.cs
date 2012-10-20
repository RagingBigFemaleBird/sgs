using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;

namespace Sanguosha.Expansions.Basic.Skills
{
    /// <summary>
    /// 激将―主公技，当你需要使用或打出一张【杀】时，你可令其他蜀势力角色打出一张【杀】(视为由你使用或打出)。
    /// </summary>
    class JiJiang : CardTransformSkill
    {
        public override VerifierResult TryTransform(List<Card> cards, object arg, out CompositeCard card)
        {
            card = null;
            if (cards != null && cards.Count != 0)
            {
                return VerifierResult.Fail;
            }
            Player player = Owner;
            do
            {
                player = Game.CurrentGame.NextPlayer(player);
                if (player.Hero.Allegiance == Core.Heroes.Allegiance.Shu)
                {
                    break;
                }
            } while (player != Owner);

            if (player == Owner)
            {
                return VerifierResult.Fail;
            }

            card = new CompositeCard();
            if (cards == null)
            {
                card.Subcards = new List<Card>();
            }
            else
            {
                card.Subcards = new List<Card>(cards);
            }
            card.Type = new Sha();
            return VerifierResult.Success;
        }

        protected override bool DoTransformSideEffect(CompositeCard card, object arg, List<Player> targets)
        {
            Player player = Owner;
            ICard result = null;
            player = Game.CurrentGame.NextPlayer(player);
            do
            {
                if (player.Hero.Allegiance == Core.Heroes.Allegiance.Shu)
                {
                    bool failToRespond = false;
                    while (true)
                    {
                        IUiProxy ui = Game.CurrentGame.UiProxies[player];
                        SingleCardUsageVerifier v1 = new SingleCardUsageVerifier((c) => { return c.Type is Sha; });
                        ISkill skill;
                        List<Player> p;
                        List<Card> cards;
                        if (!ui.AskForCardUsage("JiJiang", v1, out skill, out cards, out p))
                        {
                            failToRespond = true;
                            break;
                        }
                        if (!Game.CurrentGame.CommitCardTransform(player, skill, cards, out result, targets))
                        {
                            continue;
                        }

                        Trace.TraceInformation("Player {0} Responded JiJiang with SHA, ", player.Id);
                        break;
                    }
                    if (failToRespond)
                    {
                        continue;
                    }
                    break;
                }
            } while ((player = Game.CurrentGame.NextPlayer(player)) != Owner);

            if (player == Owner)
            {
                return false;
            }

            Trace.Assert(result != null);
            card.Subcards = new List<Card>();
            if (result is CompositeCard)
            {
                card.Subcards.AddRange(((CompositeCard)result).Subcards);
            }
            else
            {
                Trace.Assert(result is Card);
                card.Subcards.Add((Card)result);
            }
            card.Type = new Sha();
            return true;
        }

        public override CardHandler PossibleResult
        {
            get { return null; }
        }
    }
}
