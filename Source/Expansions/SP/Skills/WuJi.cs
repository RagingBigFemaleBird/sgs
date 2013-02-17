using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Heroes;
using Sanguosha.Core.UI;

namespace Sanguosha.Expansions.SP.Skills
{
    /// <summary>
    /// 武继-觉醒技，回合结束阶段开始时，若本回合你已造成3点或更多伤害，你须加1点体力上限并回复1点体力，然后失去技能“虎啸”。
    /// </summary>
    public class WuJi : TriggerSkill
    {
        public static PlayerAttribute WuJiCount = PlayerAttribute.Register("WuJiCount", true);
        public static PlayerAttribute WuJiAwaken = PlayerAttribute.Register("WuJiAwaken", false);

        public WuJi()
        {
            var trigger1 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return p[WuJiAwaken] == 0; },
                (p, e, a) => { p[WuJiCount] += (a as DamageEventArgs).Magnitude; },
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.AfterDamageCaused, trigger1);

            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return p[WuJiCount] >= 3 && p[WuJiAwaken] == 0; },
                (p, e, a) =>
                {
                    p[WuJiAwaken]++;
                    p.MaxHealth++;
                    Game.CurrentGame.RecoverHealth(p, p, 1);
                    ISkill huxiao = p.LoseHeroSkill("HuXiao", HeroTag);
                    Trace.Assert(huxiao != null);
                    Game.CurrentGame.NotificationProxy.NotifyLogEvent(new LogEvent("WuJi", Owner, huxiao), new List<Player>() { Owner });
                },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.End], trigger2);
            IsAwakening = true;
        }
    }
}
