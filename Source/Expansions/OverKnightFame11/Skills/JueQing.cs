using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.OverKnightFame11.Skills
{
    /// <summary>
    /// 绝情-锁定计，你造成的伤害均为体力流失。
    /// </summary>
    public class JueQing : TriggerSkill
    {

        public void BeforeDamage(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            Game.CurrentGame.LoseHealth(eventArgs.Targets[0], (eventArgs as DamageEventArgs).Magnitude);
            throw new TriggerResultException(TriggerResult.End);
        }

        public JueQing()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                BeforeDamage,
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.BeforeDamageComputing, trigger);
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
