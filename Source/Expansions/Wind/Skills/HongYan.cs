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
    /// 红颜-锁定技，你的黑桃牌均视为红桃牌。
    /// </summary>
    public class HongYan : TriggerSkill
    {
        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            foreach (Card c in eventArgs.Cards)
            {
                if (c.Suit == SuitType.Spade)
                {
                    c.Suit = SuitType.Heart;
                    if (c.Log == null) c.Log = new ActionLog();
                    c.Log.SkillAction = this;
                }
            }
        }

        public HongYan()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Card != null && a.Card.Suit == SuitType.Spade; },
                (p, e, a) => { Card c = new Card(a.Card); c.Suit = SuitType.Heart; a.Card = new ReadOnlyCard(c); },
                TriggerCondition.OwnerIsSource
            );
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.PlayerJudgeBegin, trigger);
            Triggers.Add(GameEvent.PlayerJudgeDone, trigger);
            Triggers.Add(GameEvent.CardsAcquired, trigger2);
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
