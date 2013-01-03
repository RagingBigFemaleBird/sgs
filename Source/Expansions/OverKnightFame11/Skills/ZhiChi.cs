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

namespace Sanguosha.Expansions.OverKnightFame11.Skills
{
    /// <summary>
    /// 智迟-锁定技，你的回合外，每当你受到一次伤害后，【杀】或非延时类锦囊牌对你无效，直到回合结束。
    /// </summary>
    public class ZhiChi : TriggerSkill
    {
        public class ZhiChiProtect : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Targets[0] != Owner ||
                    !(eventArgs.ReadonlyCard.Type.IsCardCategory(CardCategory.ImmediateTool) || eventArgs.ReadonlyCard.Type is Sha))
                    return;
                throw new TriggerResultException(TriggerResult.End);
            }
            public ZhiChiProtect(Player p)
            {
                Owner = p;
            }
        }

        public class ZhiChiRemoval : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner)
                    return;
                Game.CurrentGame.UnregisterTrigger(GameEvent.CardUsageTargetValidating, protectTrigger);
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhaseEndEvents[TurnPhase.End], this);
            }

            private Trigger protectTrigger;
            public ZhiChiRemoval(Player p, Trigger trigger)
            {
                Owner = p;
                protectTrigger = trigger;
            }
        }

        public ZhiChi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => {return Game.CurrentGame.CurrentPlayer != Owner;},
                (p, e, a) =>
                {
                    Trigger tri = new ZhiChiProtect(Owner);
                    Game.CurrentGame.RegisterTrigger(GameEvent.CardUsageTargetValidating, tri);
                    Game.CurrentGame.RegisterTrigger(GameEvent.PhaseEndEvents[TurnPhase.End], new ZhiChiRemoval(Game.CurrentGame.CurrentPlayer, tri));
                },
                TriggerCondition.OwnerIsTarget
            );
            Triggers.Add(GameEvent.AfterDamageInflicted, trigger);
            IsEnforced = true;
        }
    }
}
