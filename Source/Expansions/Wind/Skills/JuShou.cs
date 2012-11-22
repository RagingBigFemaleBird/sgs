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
    /// 据守-回合结束阶段开始时，你可以摸三张牌，然后将你的武将牌翻面。
    /// </summary>
    public class JuShou : TriggerSkill
    {
        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            Game.CurrentGame.DrawCards(Owner, 3);
            Owner.IsImprisoned = !Owner.IsImprisoned;
        }

        public JuShou()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.OwnerIsSource
            );

            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.End], trigger);
            IsAutoInvoked = null;
        }

    }
}
