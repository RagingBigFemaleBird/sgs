using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using System.Diagnostics;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.OverKnightFame13.Skills
{
    /// <summary>
    /// 惴恐-一名角色的回合开始时，若你已受伤，你可以和该角色进行一次拼点。若你赢，该角色跳过本回合的出牌阶段；若你没赢，该角色与你距离为1直到回合结束。
    /// </summary>
    public class ZhuiKong : TriggerSkill
    {
        class ZhuiKongVerifier : CardsAndTargetsVerifier
        {
            public ZhuiKongVerifier()
            {
                MaxCards = 1;
                MinCards = 1;
                MaxPlayers = 0;
            }

            protected override bool VerifyCard(Player source, Card card)
            {
                return card.Place.DeckType == DeckType.Hand;
            }
        }

        class ZhuiKongLose : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                var arg = eventArgs as AdjustmentEventArgs;
                if (arg.Source == Owner && arg.Targets[0] == target) arg.AdjustmentAmount = 1;
            }

            Player target = null;
            public ZhuiKongLose(Player t, Player fushou)
            {
                Owner = t;
                this.target = fushou;
            }
        }

        class ZhuiKongRemoval : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner)
                {
                    return;
                }

                Game.CurrentGame.UnregisterTrigger(GameEvent.PhasePostEnd, this);
                Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerDistanceOverride, trigger);
            }

            Trigger trigger = null;
            public ZhuiKongRemoval(Player t, Trigger trigger)
            {
                Owner = t;
                this.trigger = trigger;
            }
        }

        public ZhuiKong()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return p.LostHealth > 0 && p.HandCards().Count > 0 && a.Source.HandCards().Count > 0; },
                (p, e, a) => 
                {
                    if (Game.CurrentGame.PinDian(p, a.Source, this) == true)
                    {
                        Game.CurrentGame.PhasesSkipped.Add(TurnPhase.Play);
                    }
                    else
                    {
                        Trigger tri = new ZhuiKongLose(a.Source, p);
                        Game.CurrentGame.RegisterTrigger(GameEvent.PlayerDistanceOverride, tri);
                        Game.CurrentGame.RegisterTrigger(GameEvent.PhasePostEnd, new ZhuiKongRemoval(a.Source, tri));
                    }
                },
                TriggerCondition.Global
            );
            Triggers.Add(GameEvent.PhaseProceedEvents[TurnPhase.BeforeStart], trigger);
            IsAutoInvoked = null;
        }
    }
}
