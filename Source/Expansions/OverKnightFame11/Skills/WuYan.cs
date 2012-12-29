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

namespace Sanguosha.Expansions.OverKnightFame11.Skills
{
    /// <summary>
    /// 无言-锁定技，你使用的非延时类锦囊对其他角色无效；其他角色使用的非延时类锦囊对你无效。
    /// </summary>
    public class WuYan : TriggerSkill
    {
        public WuYan()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => 
                {
                    return ((a.Source == p && a.Targets[0] != p) || (a.Source != p && a.Targets[0] == p)) &&
                             a.ReadonlyCard.Type.IsCardCategory(CardCategory.ImmediateTool);
                },
                (p, e, a) => { throw new TriggerResultException(TriggerResult.End); },
                TriggerCondition.Global
            );
            Triggers.Add(GameEvent.CardUsageTargetValidating, trigger);
            IsEnforced = true;
        }

    }
}
