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

namespace Sanguosha.Expansions.Hills.Skills
{
    /// <summary>
    /// 忍戒-锁定技，当你受到伤害或于弃牌阶段弃牌时，获得等同于受到伤害(或弃牌)数量的“忍”。
    /// </summary>
    public class RenJie : TriggerSkill
    {
        public RenJie()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { Trace.Assert(a.IntArg < 0); p[RenMark] += (-a.IntArg); },
                TriggerCondition.OwnerIsTarget
            );
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return Game.CurrentGame.CurrentPhase == TurnPhase.Discard && a.Source == p; },
                (p, e, a) => { p[RenMark] += a.Cards.Count; },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.AfterDamageInflicted, trigger);
            Triggers.Add(GameEvent.CardsLost, trigger2);
        }

        public override bool IsEnforced
        {
            get
            {
                return true;
            }
        }
        public static PlayerAttribute RenMark = PlayerAttribute.Register("Ren", false, true);
    }
}
