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
    /// 武神-锁定技，你的红桃手牌均视为【杀】，你使用红桃【杀】时无距离限制。
    /// </summary>
    public class WuShen : TriggerSkill
    {
        class WuShenShaTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                ShaEventArgs args = (ShaEventArgs)eventArgs;
                Trace.Assert(args != null);
                if (args.Source != Owner)
                {
                    return;
                }
                if (args.Card.Suit != SuitType.Heart)
                {
                    return;
                }
                for (int i = 0; i < args.RangeApproval.Count; i++)
                {
                    args.RangeApproval[i] = true;
                }
            }
        }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            foreach (Card c in eventArgs.Cards)
            {
                if (c.Suit == SuitType.Heart)
                {
                    c.Type = new RegularSha();
                    if (c.Log == null) c.Log = new ActionLog();
                    c.Log.SkillAction = this;
                }
            }
        }

        public WuShen()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.CardsAcquired, trigger);
            Triggers.Add(Sha.PlayerShaTargetValidation, new WuShenShaTrigger());
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
