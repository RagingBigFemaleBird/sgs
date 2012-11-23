using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;
using System.Diagnostics;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.SP.Skills
{
    /// <summary>
    /// 虎啸-可以啸一下
    /// </summary>
    public class HuXiao : TriggerSkill
    {
        public HuXiao()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return Game.CurrentGame.CurrentPhase == TurnPhase.Play && Game.CurrentGame.CurrentPlayer == p && p[HuXiaoTriggered] == 0; },
                (p, e, a) => { p[HuXiaoTriggered] = 1; p[Sha.AdditionalShaUsable]++; },
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false };
            Triggers.Add(ShaCancelling.PlayerShaTargetDodged, trigger);
            IsAutoInvoked = null;
        }


        public static PlayerAttribute HuXiaoTriggered = PlayerAttribute.Register("HuXiaoTriggered", true);

    }
}
