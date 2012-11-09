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
    /// 无双-锁定技，当你使用【杀】指定一名角色为目标后，该角色需连续使用两张【闪】才能抵消；与你进行【决斗】的角色每次需连续打出两张【杀】。
    /// </summary>
    public class WuShuang : TriggerSkill
    {
        void RunSha(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            eventArgs.IntArg = 2;
        }

        void RunJueDou(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            eventArgs.IntArg = 2;
        }

        public WuShuang()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                RunSha,
                TriggerCondition.OwnerIsSource
            );

            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                RunJueDou,
                TriggerCondition.OwnerIsSource
            );

            Triggers.Add(Sha.PlayerShaTargetShanModifier, trigger);
            Triggers.Add(JueDou.JueDouModifier, trigger2);
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
