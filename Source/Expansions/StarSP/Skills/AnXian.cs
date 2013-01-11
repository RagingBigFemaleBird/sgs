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
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.StarSP.Skills
{
    /// <summary>
    /// 安娴–当你使用【杀】对目标角色造成伤害时，你可以防止此伤害，该角色须弃置一张手牌，然后你摸一张牌；当你成为【杀】的目标时，你可以弃置一张手牌使之无效，然后该【杀】的使用者摸一张牌。
    /// </summary>
    public class AnXian : TriggerSkill
    {
        class AnXianVerifier : CardsAndTargetsVerifier
        {
            public AnXianVerifier()
            {
                MaxCards = 1;
                MinCards = 1;
                MaxPlayers = 0;
                MinPlayers = 0;
                Discarding = true;
            }

            protected override bool VerifyCard(Player source, Card card)
            {
                return card.Place.DeckType == DeckType.Hand;
            }
        }

        public AnXian()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.ReadonlyCard != null && a.ReadonlyCard.Type is Sha; },
                (p, e, a) =>
                {
                    while (true)
                    {
                        if (a.Targets[0].HandCards().Count == 0) break;
                        Game.CurrentGame.ForcePlayerDiscard(a.Targets[0], (pl, d) => { return 1 - d; }, false);
                        Game.CurrentGame.DrawCards(p, 1);
                        break;
                    }
                    throw new TriggerResultException(TriggerResult.End);
                },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.DamageCaused, trigger);

            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.ReadonlyCard != null && a.ReadonlyCard.Type is Sha; },
                (p, e, a) =>
                {
                    ISkill skill;
                    List<Card> cards;
                    List<Player> players;
                    if (p.AskForCardUsage(new CardUsagePrompt("AnXian"), new AnXianVerifier(), out skill, out cards, out players))
                    {
                        NotifySkillUse();
                        Game.CurrentGame.HandleCardDiscard(p, cards);
                        a.ReadonlyCard[AnXianSha[p]] = 1;
                    }
                },
                TriggerCondition.OwnerIsTarget
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.CardUsageTargetConfirming, trigger2);

            var trigger3 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.ReadonlyCard != null && a.ReadonlyCard[AnXianSha[p]] != 0; },
                (p, e, a) => { throw new TriggerResultException(TriggerResult.End); },
                TriggerCondition.OwnerIsTarget
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.CardUsageTargetValidating, trigger3);

            var trigger4 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.ReadonlyCard != null && a.ReadonlyCard[AnXianSha[p]] != 0; },
                (p, e, a) => { Game.CurrentGame.DrawCards(a.Source, 1); },
                TriggerCondition.OwnerIsTarget
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.CardUsageDone, trigger4);
            IsAutoInvoked = null;
        }

        private static CardAttribute AnXianSha = CardAttribute.Register("AnXianSha");
    }
}
