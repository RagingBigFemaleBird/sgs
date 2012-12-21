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

namespace Sanguosha.Expansions.SP.Skills
{
    /// <summary>
    /// 庸肆-锁定技，摸牌阶段，你额外摸X张牌，X为场上现存势力数。弃牌阶段，你至少须弃掉等同于场上现存势力数的牌(不足则全弃)。
    /// </summary>
    public class YongSi : TriggerSkill
    {
        public YongSi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { p[Player.DealAdjustment] += Game.CurrentGame.NumberOfAliveAllegiances; },
                TriggerCondition.OwnerIsSource
            );
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    Game.CurrentGame.ForcePlayerDiscard(
                     p,
                     (pl, i) =>
                     {
                         return Game.CurrentGame.NumberOfAliveAllegiances - i;
                     },
                     true);
                },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PhaseProceedEvents[TurnPhase.Draw], trigger);
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Discard], trigger2);
            IsEnforced = true;
        }
    }
}