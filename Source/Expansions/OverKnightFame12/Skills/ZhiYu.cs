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

namespace Sanguosha.Expansions.OverKnightFame12.Skills
{
    /// <summary>
    /// 智愚-每当你受到一次伤害后，可以摸一张牌，然后展示所有手牌，若均为同一颜色，则伤害来源弃一张手牌。
    /// </summary>
    public class ZhiYu : TriggerSkill
    {
        public void Run(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            Game.CurrentGame.DrawCards(owner, 1);
            Game.CurrentGame.SyncImmutableCardsAll(owner.HandCards());
            Game.CurrentGame.ShowHandCards(owner, owner.HandCards());
            var result = from card in owner.HandCards() select card.SuitColor;
            if (result.Distinct().Count() == 1)
            {
                Game.CurrentGame.ForcePlayerDiscard(eventArgs.Source,
                                                    (p, i) =>
                                                    {
                                                        return 1 - i;
                                                    },
                                                    false);
            }
        }

        public ZhiYu()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Source != null; },
                Run,
                TriggerCondition.OwnerIsTarget
            );
            Triggers.Add(GameEvent.AfterDamageInflicted, trigger);
            IsAutoInvoked = null;
        }
    }
}
