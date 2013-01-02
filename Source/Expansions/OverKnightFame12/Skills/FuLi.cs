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

namespace Sanguosha.Expansions.OverKnightFame12.Skills
{
    /// <summary>
    /// 伏枥-限定技，当你处于濒死状态时，你可以将体力回复至X点（X为现存势力数），然后将你的武将牌翻面。
    /// </summary>
    public class FuLi : TriggerSkill
    {
        protected void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            Owner.Health = Game.CurrentGame.NumberOfAliveAllegiances;
            Owner.IsImprisoned = !Owner.IsImprisoned;
        }

        public FuLi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return p[FuLiUsed] == 0; },
                Run,
                TriggerCondition.OwnerIsTarget
            );
            Triggers.Add(GameEvent.PlayerDying, trigger);
            IsAutoInvoked = null;
            IsSingleUse = true;
        }
        public static PlayerAttribute FuLiUsed = PlayerAttribute.Register("FuLiUsed", false);

    }
}
