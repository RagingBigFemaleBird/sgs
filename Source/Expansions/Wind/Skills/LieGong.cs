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
    /// 烈弓-出牌阶段，当你使用【杀】指定一名角色为目标后，以下两种情况，你可以令此【杀】不可被【闪】响应：
    ///   1.目标角色的手牌数大于或等于你的体力值。
    ///   2.目标角色的手牌数小于或等于你的攻击范围。
    /// </summary>
    public class LieGong : TriggerSkill
    {

        public LieGong()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return (a.ReadonlyCard.Type is Sha) && Game.CurrentGame.PhasesOwner == p && Game.CurrentGame.CurrentPhase == TurnPhase.Play; },
                (p, e, a) =>
                {
                    int i = 0;
                    foreach (var target in a.Targets)
                    {
                        if ((Game.CurrentGame.Decks[target, DeckType.Hand].Count >= a.Source.Health || Game.CurrentGame.Decks[target, DeckType.Hand].Count <= a.Source[Player.AttackRange] + 1)
                         && AskForSkillUse())
                        {
                            a.ReadonlyCard[ShaCancelling.CannotProvideShan[target]] |= (1 << i);
                            NotifySkillUse(new List<Player>() { target });
                        }
                        i++;
                    }
                },
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false, AskForConfirmation = false };

            Triggers.Add(GameEvent.CardUsageTargetConfirmed, trigger);
            IsAutoInvoked = true;
        }

    }
}
