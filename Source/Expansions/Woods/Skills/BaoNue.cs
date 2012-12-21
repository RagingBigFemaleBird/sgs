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
using Sanguosha.Core.Heroes;

namespace Sanguosha.Expansions.Woods.Skills
{
    /// <summary>
    /// 暴虐―主公技，每当其他群雄角色每造成一次伤害后，可进行一次判定，若为黑桃，你回复1点体力。
    /// </summary>
    public class BaoNueGivenSkill : TriggerSkill, IRulerGivenSkill
    {
        public BaoNueGivenSkill()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => 
                {
                    var result = Game.CurrentGame.Judge(p, this, null, (judgeResultCard) => { return judgeResultCard.Suit == SuitType.Spade; });
                    if (result.Suit == SuitType.Spade)
                    {
                        Game.CurrentGame.RecoverHealth(p, Master, 1);
                    }
                },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.AfterDamageCaused, trigger);
            IsAutoInvoked = false;
        }

        public Player Master { get; set; }
    }

    public class BaoNue : RulerGivenSkillContainerSkill
    {
        public BaoNue()
            : base(new BaoNueGivenSkill(), Allegiance.Qun)
        {
        }
    }
}
