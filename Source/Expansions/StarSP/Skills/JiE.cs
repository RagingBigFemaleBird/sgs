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

namespace Sanguosha.Expansions.StarSP.Skills
{
    /// <summary>
    /// 嫉恶-锁定技，你使用的红色【杀】造成的伤害+1。
    /// </summary>
    public class JiE : TriggerSkill
    {
        public JiE()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    var args = a as DamageEventArgs;
                    return args.ReadonlyCard.SuitColor == SuitColorType.Red && args.ReadonlyCard.Type is Sha;
                },
                (p, e, a) =>
                {
                    var args = a as DamageEventArgs;
                    args.Magnitude++;
                },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.DamageCaused, trigger);
            IsEnforced = true;
        }

    }
}
