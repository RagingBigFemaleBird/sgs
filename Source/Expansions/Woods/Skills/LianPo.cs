using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;

namespace Sanguosha.Expansions.Woods.Skills
{
    /// <summary>
    /// 连破-若你于一回合内杀死了至少一名角色，可于此回合结束后，进行一个额外的回合。
    /// </summary>
    public class LianPo : TriggerSkill
    {
        public LianPo()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { p[LianPoCount]++; },
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { int count = p[LianPoCount]; p[LianPoCount] = 0; return count > 0; },
                (p, e, a) => { Game.CurrentGame.DoPlayer(p); },
                TriggerCondition.Global
            ) { Priority = int.MinValue };
            Triggers.Add(GameEvent.PlayerIsDead, trigger);
            Triggers.Add(GameEvent.PhasePostEnd, trigger2);
            IsAutoInvoked = true;
        }
        public static PlayerAttribute LianPoCount = PlayerAttribute.Register("LianPoCount", false);
    }
}
