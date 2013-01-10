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
using Sanguosha.Core.Heroes;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.StarSP.Skills
{
    /// <summary>
    /// 粪涌―当你受到一次伤害后，你可以丢弃你的节操；当你受到的伤害结算开始时，若你为无节操状态，防止此伤害。
    /// </summary>
    public class FenYong : TriggerSkill
    {
        protected override int GenerateSpecialEffectHintIndex(Player source, List<Player> targets)
        {
            return source[FenYongStatus];
        }

        public FenYong()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { Owner[FenYongStatus] = 1; },
                TriggerCondition.OwnerIsTarget
            );
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return Owner[FenYongStatus] == 1; },
                (p, e, a) => { throw new TriggerResultException(TriggerResult.End); },
                TriggerCondition.OwnerIsTarget
            ) { AskForConfirmation = false, Priority = int.MaxValue };
            IsAutoInvoked = true;
            Triggers.Add(GameEvent.AfterDamageInflicted, trigger);
            Triggers.Add(GameEvent.DamageComputingStarted, trigger2);
        }

        public static readonly PlayerAttribute FenYongStatus = PlayerAttribute.Register("FenYong", false, false, true);
    }
}
