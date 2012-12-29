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
    /// 恩怨-锁定技，其他角色每令你回复1点体力，该角色摸一张牌；其他角色每对你造成一次伤害，须给你一张红桃手牌，否则该角色失去1点体力。
    /// </summary>
    public class EnYuan : TriggerSkill
    {
        public class EnYuanVerifier : CardsAndTargetsVerifier
        {
            public EnYuanVerifier()
            {
                MinCards = 1;
                MaxCards = 1;
                MinPlayers = 0;
                MaxPlayers = 0;
            }
            protected override bool VerifyCard(Player source, Card card)
            {
                return card.Suit == SuitType.Heart;
            }
        }

        public void Run(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (eventArgs.Source.AskForCardUsage(new CardUsagePrompt("EnYuan", owner), new EnYuanVerifier(), out skill, out cards, out players))
            {
                Game.CurrentGame.HandleCardTransferToHand(eventArgs.Source, owner, cards);
            }
            else
            {
                Game.CurrentGame.LoseHealth(eventArgs.Source, 1);
            }
        }

        public EnYuan()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    var arg = a as HealthChangedEventArgs;
                    return arg.Delta > 0 && arg.Source != p && arg.Source != null;
                },
                (p, e, a) => { Game.CurrentGame.DrawCards(a.Source, 1); },
                TriggerCondition.OwnerIsTarget
            );
            Triggers.Add(GameEvent.AfterHealthChanged, trigger);
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Source != null; },
                Run,
                TriggerCondition.OwnerIsTarget
            );
            Triggers.Add(GameEvent.AfterDamageInflicted, trigger2);
            IsEnforced = true;
        }

    }
}
