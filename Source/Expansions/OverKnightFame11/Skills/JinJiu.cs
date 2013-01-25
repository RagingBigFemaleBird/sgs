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
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.OverKnightFame11.Skills
{
    /// <summary>
    /// 禁酒-锁定技，你的【酒】均视为【杀】。
    /// </summary>
    public class JinJiu : TriggerSkill
    {
        public JinJiu()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => {return a.Card.Place.DeckType == DeckType.Hand && a.Card != null && a.Card.Type is Jiu; },
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
            IsEnforced = true;
        }
    }
}
