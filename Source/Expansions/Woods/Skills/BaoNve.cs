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
    public class BaoNveGivenSkill : TriggerSkill, IRulerGivenSkill
    {
        public BaoNveGivenSkill()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.ReadonlyCard[BaoNveUsable] == 1; },
                (p, e, a) =>
                {
                    NotifySkillUse(new List<Player>() { Master });
                    a.ReadonlyCard[BaoNveUsable] = 0;
                    var result = Game.CurrentGame.Judge(p, this, null, (judgeResultCard) => { return judgeResultCard.Suit == SuitType.Spade; });
                    if (result.Suit == SuitType.Spade)
                    {
                        Game.CurrentGame.RecoverHealth(p, Master, 1);
                    }
                },
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.AfterDamageCaused, trigger);
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { a.ReadonlyCard[BaoNveUsable] = 1; },
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false, AskForConfirmation = false, Priority = int.MinValue };
            Triggers.Add(GameEvent.DamageInflicted, trigger2);
            IsAutoInvoked = false;
        }

        private static CardAttribute BaoNveUsable = CardAttribute.Register("BaoNveUsable");
        public Player Master { get; set; }
    }

    public class BaoNve : RulerGivenSkillContainerSkill
    {
        public BaoNve()
            : base(new BaoNveGivenSkill(), Allegiance.Qun)
        {
        }
    }
}
