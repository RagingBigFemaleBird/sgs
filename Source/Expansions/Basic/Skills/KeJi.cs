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
    /// 克己-若你于出牌阶段未使用或打出过【杀】，你可以跳过此回合的弃牌阶段。
    /// </summary>
    public class KeJi : TriggerSkill
    {
        public static PlayerAttribute KeJiFailed = PlayerAttribute.Register("KeJiFailed", true);

        protected override int GenerateSpecialEffectHintIndex(Player source, List<Player> targets)
        {
            if (Owner.Hero.Name == "SPLvMeng" || (Owner.Hero2 != null && Owner.Hero2.Name == "SPLvMeng")) return 1;
            return 0;
        }

        public KeJi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return Game.CurrentGame.CurrentPhase == TurnPhase.Play && Game.CurrentGame.CurrentPlayer == p && a.Card != null && a.ReadonlyCard != null && a.ReadonlyCard.Type is Sha; },
                (p, e, a) => { p[KeJiFailed] = 1;},
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false, AskForConfirmation = false };

            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return p[KeJiFailed] == 0; },
                (p, e, a) => { Game.CurrentGame.CurrentPhase++; Game.CurrentGame.CurrentPhaseEventIndex = 2; throw new TriggerResultException(TriggerResult.End); },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PlayerUsedCard, trigger);
            Triggers.Add(GameEvent.PlayerPlayedCard, trigger);

            Triggers.Add(GameEvent.PhaseOutEvents[TurnPhase.Play], trigger2);
            IsAutoInvoked = true;
        }
    }
}
