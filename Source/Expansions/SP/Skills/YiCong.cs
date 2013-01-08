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
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.SP.Skills
{
    /// <summary>
    /// 义从-锁定技，若你当前的体力值大于2，你计算的与其他角色的距离-1；若你当前的体力值小于或等于2，其他角色计算的与你的距离+1。
    /// </summary>
    public class YiCong : TriggerSkill
    {
        public YiCong()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => 
                {
                    if (p.Health > 2)
                        return p == a.Source;
                    return a.Targets.Contains(p);
                },
                (p, e, a) => { var args = a as AdjustmentEventArgs; args.AdjustmentAmount = p.Health > 2 ? -1 : 1; },
                TriggerCondition.Global
            ) { IsAutoNotify = false, AskForConfirmation = false };
            var effect = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => 
                {
                    if (p[YiCongEffect] == 0 && p.Health <= 2)
                    {
                        p[YiCongEffect] = 1;
                        NotifySkillUse();
                    }
                    if (p[YiCongEffect] == 1 && p.Health > 2)
                    {
                        p[YiCongEffect] = 0;
                        NotifySkillUse();
                    }
                },
                TriggerCondition.OwnerIsTarget
            ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.PlayerDistanceAdjustment, trigger);
            Triggers.Add(GameEvent.AfterHealthChanged, effect);
            IsEnforced = true;
        }

        protected override int GenerateSpecialEffectHintIndex(Player source, List<Player> targets)
        {
            return source[YiCongEffect];
        }

        private static PlayerAttribute YiCongEffect = PlayerAttribute.Register("YiCongEffect");
    }
}
