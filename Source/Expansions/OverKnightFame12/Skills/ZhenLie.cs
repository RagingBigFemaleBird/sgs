using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Expansions.Basic.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.OverKnightFame12.Skills
{
    /// <summary>
    /// 贞烈-每当你成为一名其他角色使用的【杀】或非延时类锦囊牌的目标后，你可以失去1点体力，令此牌对你无效，然后你弃置其一张牌。
    /// </summary>
    public class ZhenLie : TriggerSkill
    {
        public ZhenLie()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return (a.ReadonlyCard.Type.IsCardCategory(CardCategory.ImmediateTool) || a.ReadonlyCard.Type is Sha) && a.Source != Owner; },
                (p, e, a) =>
                {
                    Game.CurrentGame.LoseHealth(p, 1);
                    a.ReadonlyCard[ZhenLieUsed[p]] = 1;
                    if (!p.IsDead)
                    {
                        var theCard = Game.CurrentGame.SelectACardFrom(a.Source, p, new CardChoicePrompt("ZhenLie", a.Source), "ZhenLie");
                        if (theCard != null) Game.CurrentGame.HandleCardDiscard(a.Source, new List<Card>() { theCard });
                    }
                },
                TriggerCondition.OwnerIsTarget
            );

            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.ReadonlyCard[ZhenLieUsed[p]] == 1; },
                (p, e, a) => { throw new TriggerResultException(TriggerResult.End); },
                TriggerCondition.OwnerIsTarget
            ) { AskForConfirmation = false, IsAutoNotify = false };

            Triggers.Add(GameEvent.CardUsageTargetConfirmed, trigger);
            Triggers.Add(GameEvent.CardUsageTargetValidating, trigger2);
            IsAutoInvoked = null;
        }
        private static CardAttribute ZhenLieUsed = CardAttribute.Register("ZhenLieUsed");
    }
}
