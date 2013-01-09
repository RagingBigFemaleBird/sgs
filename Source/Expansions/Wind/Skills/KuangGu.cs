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
            Game.CurrentGame.RecoverHealth(Owner, Owner, (eventArgs as DamageEventArgs).Magnitude);
        }

        public KuangGu()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return p.LostHealth > 0 && p[KuangGuUsable] == 1; },
                Run,
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.AfterDamageCaused, trigger);
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return Game.CurrentGame.DistanceTo(p, a.Targets[0]) <= 1 && a.Source[KuangGuUsable] == 0; },
                (p, e, a) => { a.Source[KuangGuUsable] = 1; },
                TriggerCondition.Global
            ) { IsAutoNotify = false, AskForConfirmation = false, Priority = int.MinValue };
            Triggers.Add(GameEvent.DamageInflicted, trigger2);
            IsEnforced = true;
        }

        private static PlayerAttribute KuangGuUsable = PlayerAttribute.Register("KuangGuUsable");
    }
}
