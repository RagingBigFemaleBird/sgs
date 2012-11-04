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
    /// 闭月-回合结束阶段开始时，你可以摸一张牌。
    /// </summary>
    public class BiYue : TriggerSkill
    {
        public BiYue()
        {
            Trigger trigger = new AutoNotifyPassiveSkillTrigger
            (
                this,
                (p, a, e) =>
                {
                    Game.CurrentGame.DrawCards(p, 1);
                },
                TriggerCondition.OwnerIsSource
            );

            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.End], trigger);
        }
    }
}
