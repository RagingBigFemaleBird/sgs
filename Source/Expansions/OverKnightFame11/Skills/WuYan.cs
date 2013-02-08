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
    /// 无言-锁定技，你防止你造成或受到的任何锦囊牌的伤害。
    /// </summary>
    public class WuYan : TriggerSkill
    {
        public WuYan()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    return (a.Source == p && e == GameEvent.DamageCaused || a.Targets[0] == p && e == GameEvent.DamageInflicted) && a.ReadonlyCard.Type != null && a.ReadonlyCard.Type.IsCardCategory(CardCategory.Tool);
                },
                (p, e, a) => { throw new TriggerResultException(TriggerResult.End); },
                TriggerCondition.Global
            );
            Triggers.Add(GameEvent.DamageCaused, trigger);
            Triggers.Add(GameEvent.DamageInflicted, trigger);
            IsEnforced = true;
        }
    }
}
