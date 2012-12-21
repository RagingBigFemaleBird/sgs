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

namespace Sanguosha.Expansions.Fire.Skills
{
    /// <summary>
    /// 血裔-主公技，锁定技，场上每有一名其他群雄角色存活，你的手牌上限便+2。
    /// </summary>
    public class XueYi : TriggerSkill
    {
        protected void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            var args = eventArgs as AdjustmentEventArgs;
            foreach (var p in Game.CurrentGame.AlivePlayers)
            {
                if (p != Owner && p.Allegiance == Core.Heroes.Allegiance.Qun)
                {
                    args.AdjustmentAmount += 2;
                }
            }
        }

        public XueYi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.OwnerIsSource
            ) {};
            Triggers.Add(GameEvent.PlayerHandCardCapacityAdjustment, trigger);
            IsEnforced = true;
            IsRulerOnly = true;
        }

    }
}
