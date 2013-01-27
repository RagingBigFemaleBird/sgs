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
        bool canTrigger(Player p, GameEvent e, GameEventArgs a)
        {
            if (e == GameEvent.CardsLost)
            {
                if (!a.Cards.Any(c => c.HistoryPlace1.DeckType == DeckType.Hand))
                    return false;
            }
            if (e == GameEvent.CardsAcquired)
            {
                if (!a.Cards.Any(c => c.Place.DeckType == DeckType.Hand))
                    return false;
            }
            if (e == GameEvent.AfterHealthChanged && p.Health - (a as HealthChangedEventArgs).Delta < 0)
                return false;
            return (Game.CurrentGame.CurrentPhaseEventIndex == 3 || Game.CurrentGame.CurrentPhase != TurnPhase.Discard) && p.HandCards().Count < p.LostHealth;
        }

        void Run(Player p, GameEvent e, GameEventArgs a)
        {
            Game.CurrentGame.DrawCards(p, p.LostHealth - p.HandCards().Count);
        }

        public ShangShi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                canTrigger,
                Run,
                TriggerCondition.OwnerIsTarget
            ) { Priority = int.MinValue };
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                canTrigger,
                Run,
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.CardsLost, trigger2);
            Triggers.Add(GameEvent.CardsAcquired, trigger2);
            Triggers.Add(GameEvent.AfterHealthChanged, trigger);
            Triggers.Add(GameEvent.PhaseOutEvents[TurnPhase.Discard], trigger2);
            Triggers.Add(GameEvent.PlayerSkillSetChanged, trigger2);
            IsAutoInvoked = true;
        }
    }
}
