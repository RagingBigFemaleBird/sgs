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


        public WuShen()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Card.Place.DeckType == DeckType.Hand && a.Card != null && a.Card.Suit == SuitType.Heart; },
                (p, e, a) => 
                {
                    a.Card.Type = new RegularSha();
                    if (a.Card is Card)
                    {
                        Card c = a.Card as Card;
                        if (c.Log == null) c.Log = new ActionLog();
                        c.Log.SkillAction = this;
                    }
                },
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.EnforcedCardTransform, trigger);
            Triggers.Add(Sha.PlayerShaTargetValidation, new WuShenShaTrigger());
            IsEnforced = true;
        }

    }
}
