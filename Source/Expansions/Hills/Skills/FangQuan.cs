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

namespace Sanguosha.Expansions.Hills.Skills
{
    /// <summary>
    /// 放权-你可以跳过你的出牌阶段，然后在回合结束时可弃置一张手牌令一名其他角色进行一个额外的回合。
    /// </summary>
    public class FangQuan : TriggerSkill
    {
        class FangQuanVerifier : CardsAndTargetsVerifier
        {
            public FangQuanVerifier()
            {
                MinCards = 1;
                MaxCards = 1;
                MinPlayers = 1;
                MaxPlayers = 1;
                Discarding = true;
            }
            protected override bool VerifyCard(Player source, Card card)
            {
                return card.Place.DeckType == DeckType.Hand;
            }
            protected override bool VerifyPlayer(Player source, Player player)
            {
                return player != source;
            }
        }
        public static PlayerAttribute FangQuanUsed = PlayerAttribute.Register("FangQuanUsed", true);

        public FangQuan()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    p[FangQuanUsed] = 1;
                    Game.CurrentGame.CurrentPhase++;
                    Game.CurrentGame.CurrentPhaseEventIndex = 2;
                    throw new TriggerResultException(TriggerResult.End);
                },
                TriggerCondition.OwnerIsSource
            );
            var trigger2 = new AutoNotifyUsagePassiveSkillTrigger(
                this,
                (p, e, a) => { return p[FangQuanUsed] == 1; },
                (p, e, a, cards, players) =>
                {
                    Game.CurrentGame.HandleCardDiscard(a.Source, cards);
                    Game.CurrentGame.RegisterTrigger(GameEvent.PhasePostEnd, new FangQuanTrigger(Owner, players[0]));
                },
                TriggerCondition.OwnerIsSource,
                new FangQuanVerifier()
            ) { AskForConfirmation = false };
            Triggers.Add(GameEvent.PhaseOutEvents[TurnPhase.Draw], trigger);
            Triggers.Add(GameEvent.PhaseProceedEvents[TurnPhase.End], trigger2);
        }

        class FangQuanTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner) return;
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhasePostEnd, this);
                Game.CurrentGame.DoPlayer(target);
            }

            Player target;
            public FangQuanTrigger(Player p, Player target)
            {
                Owner = p;
                this.target = target;
                Priority = int.MinValue;
            }
        }
    }
}
