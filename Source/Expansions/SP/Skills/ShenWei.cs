using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.SP.Skills
{
    /// <summary>
    /// 神威-锁定技，摸牌阶段，你额外摸两张牌；你的手牌上限+2。
    /// </summary>
    public class ShenWei : TriggerSkill
    {
        public ShenWei()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { p[Player.DealAdjustment] += 2; },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PhaseProceedEvents[TurnPhase.Draw], trigger);
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                 this,
                 (p, e, a) =>
                 {
                     var args = a as AdjustmentEventArgs;
                     args.AdjustmentAmount += 2;
                 },
                 TriggerCondition.OwnerIsSource
             ) { IsAutoNotify = false};
            Triggers.Add(GameEvent.PlayerHandCardCapacityAdjustment, trigger2);
            IsEnforced = true;
        }
    }
}
