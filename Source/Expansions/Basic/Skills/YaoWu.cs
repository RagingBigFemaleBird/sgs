using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.Basic.Skills
{
    /// <summary>
    /// 耀武-锁定技。当一名角色使用红色杀对你造成伤害时，该角色恢复一点体力或摸一张牌。
    /// </summary>
    public class YaoWu : TriggerSkill
    {
        public YaoWu()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    return a.ReadonlyCard != null && a.ReadonlyCard.Type is Sha && a.Source != null &&
                        a.ReadonlyCard.SuitColor == SuitColorType.Red;
                },
                (p, e, a) =>
                {
                    int answer = 0;
                    if (a.Source.LostHealth > 0 && a.Source.AskForMultipleChoice(new MultipleChoicePrompt("YaoWu"), Prompt.YesNoChoices, out answer) && answer == 1)
                    {
                        Game.CurrentGame.RecoverHealth(a.Source, a.Source, 1);
                    }
                    else
                    {
                        Game.CurrentGame.DrawCards(a.Source, 1);
                    }
                },
                TriggerCondition.OwnerIsTarget
            );
            Triggers.Add(GameEvent.AfterDamageCaused, trigger);
            IsEnforced = true;
        }
    }
}
