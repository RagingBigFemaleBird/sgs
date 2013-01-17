using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Woods.Skills
{
    /// <summary>
    /// 狂暴―锁定技，游戏开始时，你获得2枚“暴怒”标记；每当你造成或受到1点伤害后，你获得1枚“暴怒”标记。
    /// </summary>
    public class KuangBao : TriggerSkill
    {
        public override Player Owner
        {
            get
            {
                return base.Owner;
            }
            set
            {
                Player original = base.Owner;
                base.Owner = value;
                if (base.Owner == null && original != null)
                {
                    original[BaoNuMark] = 0;
                }
            }
        }

        public KuangBao()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { var args = a as DamageEventArgs; p[BaoNuMark] += args.Magnitude; },
                TriggerCondition.OwnerIsTarget
            );
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { var args = a as DamageEventArgs; p[BaoNuMark] += args.Magnitude; },
                TriggerCondition.OwnerIsSource
            );
            var trigger3 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { p[BaoNuMark] += 2; },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.AfterDamageInflicted, trigger);
            Triggers.Add(GameEvent.AfterDamageCaused, trigger2);
            Triggers.Add(GameEvent.PlayerGameStartAction, trigger3);
            IsEnforced = true;
        }
        public static PlayerAttribute BaoNuMark = PlayerAttribute.Register("BaoNu", false, true);
    }
}
