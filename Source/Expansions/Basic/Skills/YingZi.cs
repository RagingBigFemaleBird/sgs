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

namespace Sanguosha.Expansions.Basic.Skills
{
    /// <summary>
    /// 英姿-摸牌阶段摸牌时，你可以额外摸一张牌。
    /// </summary>
    public class YingZi : TriggerSkill
    {
        protected override int GenerateSpecialEffectHintIndex(Player source, List<Player> targets)
        {
            Trace.Assert(Owner != null && Owner.Hero != null);
            if (Owner == null || Owner.Hero == null) return 0;
            else if (Owner.Hero.Name == "SunCe" || (Owner.Hero2 != null && Owner.Hero2.Name == "SunCe")) return 1;
            else if (Owner.Hero.Name == "SPLvMeng" || (Owner.Hero2 != null && Owner.Hero2.Name == "SPLvMeng")) return 2;
            return 0;
        }

        public YingZi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { p[Player.DealAdjustment]++; },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PhaseProceedEvents[TurnPhase.Draw], trigger);
            IsAutoInvoked = true;
        }

    }
}