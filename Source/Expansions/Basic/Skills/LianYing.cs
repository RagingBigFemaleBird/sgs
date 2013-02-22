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
    /// 连营-当你失去最后一张手牌时，你可以摸一张牌。
    /// </summary>
    public class LianYing : TriggerSkill
    {
        public LianYing()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Cards.Any(c => c[Card.IsLastHandCard] == 1); },
                (p, e, a) => { Game.CurrentGame.DrawCards(Owner, 1); },
                TriggerCondition.OwnerIsSource
            ) { Priority = SkillPriority.LianYing };
            Triggers.Add(GameEvent.CardsLost, trigger);
            IsAutoInvoked = true;
        }
    }
}
