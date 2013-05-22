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
    /// 妄尊-主公的准备阶段开始时，你可以摸一张牌，若如此做，本回合主公手牌上限-1。
    /// </summary>
    public class WangZun : TriggerSkill
    {
        public WangZun()
        {
            Trigger trigger = new AutoNotifyPassiveSkillTrigger
            (
                this,
                (p, e, a) => { return a.Source.Role == Role.Ruler; },
                (p, e, a) => { Game.CurrentGame.DrawCards(p, 1); a.Source[WangZunUsed] = 1; },
                TriggerCondition.Global
            );

            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.BeforeStart], trigger);
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    var args = a as AdjustmentEventArgs;
                    if (a.Source[WangZunUsed] == 1)
                    {
                        args.AdjustmentAmount -= 1;
                        NotifySkillUse();
                    }
                },
                TriggerCondition.Global
            ) { IsAutoNotify = false, AskForConfirmation = false };
            Triggers.Add(GameEvent.PlayerHandCardCapacityAdjustment, trigger2);
            IsAutoInvoked = false;

        }
        public static PlayerAttribute WangZunUsed = PlayerAttribute.Register("WangZunUsed", true);
    }
}
