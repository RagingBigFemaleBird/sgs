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
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Hills.Skills
{
    /// <summary>
    /// 享乐-锁定技，当其他角色使用【杀】指定你为目标时，需额外弃置一张基本牌，否则该【杀】对你无效。
    /// </summary>
    public class XiangLe : TriggerSkill
    {
        class XiangLeVerifier : CardsAndTargetsVerifier
        {
            public XiangLeVerifier()
            {
                MinCards = 1;
                MaxCards = 1;
                MinPlayers = 0;
                MaxPlayers = 0;
                Discarding = true;
            }
            protected override bool VerifyCard(Player source, Card card)
            {
                return CardCategoryManager.IsCardCategory(card.Type.Category, CardCategory.Basic);
            }
        }

        public void OnPlayerIsCardTarget(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (!(eventArgs.Source == null || eventArgs.Source.IsDead) && Game.CurrentGame.UiProxies[eventArgs.Source].AskForCardUsage(new CardUsagePrompt("XiangLe"), new XiangLeVerifier(), out skill, out cards, out players))
            {
                Game.CurrentGame.HandleCardDiscard(eventArgs.Source, cards);
                return;
            }
            eventArgs.ReadonlyCard[XiangLeEffect[owner]] = 1;
        }

        private static CardAttribute XiangLeEffect = CardAttribute.Register("XiangLeEffect");

        public XiangLe()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.ReadonlyCard.Type is Sha; },
                OnPlayerIsCardTarget,
                TriggerCondition.OwnerIsTarget
            ) { Priority = SkillPriority.XiangLe };
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.ReadonlyCard != null && a.ReadonlyCard[XiangLeEffect[p]] == 1; },
                (p, e, a) => { throw new TriggerResultException(TriggerResult.End); },
                TriggerCondition.OwnerIsTarget
            );
            Triggers.Add(GameEvent.CardUsageTargetValidating, trigger2);
            Triggers.Add(GameEvent.CardUsageTargetConfirming, trigger);
            IsEnforced = true;
        }
    }
}
