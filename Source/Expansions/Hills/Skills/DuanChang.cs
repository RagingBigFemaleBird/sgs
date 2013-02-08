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
using Sanguosha.Core.Heroes;

namespace Sanguosha.Expansions.Hills.Skills
{
    /// <summary>
    /// 断肠-锁定技，杀死你的角色失去当前的所有武将技能。
    /// </summary>
    public class DuanChang : TriggerSkill
    {
        static PlayerAttribute DuanChangStatus = PlayerAttribute.Register("DuanChang", false, false, true);
     
        public DuanChang()
        {            
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Source != null; },
                (p, e, a) =>
                {
                    a.Source.LoseAllHerosSkills();
                    foreach (ISkill sk in new List<ISkill>(a.Source.AdditionalSkills))
                    {
                        Game.CurrentGame.PlayerLoseAdditionalSkill(a.Source, sk);
                    }
                    a.Source[DuanChangStatus] = 1;
                },
                TriggerCondition.OwnerIsTarget
            );

            Triggers.Add(GameEvent.PlayerIsDead, trigger);
            IsEnforced = true;
        }
    }
}
