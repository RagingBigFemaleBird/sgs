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

namespace Sanguosha.Expansions.OverKnightFame11.Skills
{
    /// <summary>
    /// 伤逝-除弃牌阶段外，每当你的手牌数小于你已损失的体力值时，可立即将手牌数补至等同于你已损失的体力值。
    /// </summary>
    public class ShangShi : TriggerSkill
    {
        public ShangShi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return (Game.CurrentGame.CurrentPhaseEventIndex == 3 || Game.CurrentGame.CurrentPhase != TurnPhase.Discard) && p.HandCards().Count < p.MaxHealth - p.Health; },
                (p, e, a) => { Game.CurrentGame.DrawCards(p, p.MaxHealth - p.Health - p.HandCards().Count); },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.CardsLost, trigger);
            Triggers.Add(GameEvent.CardsAcquired, trigger);
            Triggers.Add(GameEvent.AfterHealthChanged, trigger);
            Triggers.Add(GameEvent.PhaseOutEvents[TurnPhase.Discard], trigger);
            Triggers.Add(GameEvent.PlayerSkillSetChanged, trigger);
            IsAutoInvoked = true;
        }
    }
}
