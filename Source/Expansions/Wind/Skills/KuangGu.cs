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

namespace Sanguosha.Expansions.Wind.Skills
{
    /// <summary>
    /// 狂骨-锁定技，每当你对距离1以内的一名角色造成1点伤害后，你回复1点体力。
    /// </summary>
    public class KuangGu : TriggerSkill
    {
        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            Game.CurrentGame.RecoverHealth(Owner, Owner, -eventArgs.IntArg);
        }

        public KuangGu()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return p.Health < p.MaxHealth &&  Game.CurrentGame.DistanceTo(p, a.Targets[0]) <= 1; },
                Run,
                TriggerCondition.OwnerIsSource
            );

            Triggers.Add(GameEvent.AfterDamageCaused, trigger);
        }

        public override bool IsEnforced
        {
            get
            {
                return true;
            }
        }
    }
}
