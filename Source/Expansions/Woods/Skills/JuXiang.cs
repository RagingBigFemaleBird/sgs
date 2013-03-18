using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Woods.Skills
{
    /// <summary>
    /// 巨象-锁定技，【南蛮入侵】对你无效；若其他角色使用的【南蛮入侵】在结算后置入弃牌堆，你获得之。
    /// </summary>
    public class JuXiang : TriggerSkill
    {
        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            var args = eventArgs as DiscardCardEventArgs;
            if (args.Source == null || args.Source == Owner || args.Reason != DiscardReason.Use)
            {
                return;
            }
            List<Card> cardsToProcess = new List<Card>(eventArgs.Cards);
            foreach (Card c in cardsToProcess)
            {
                if (c.Type is NanManRuQin)
                {
                    ActionLog log = new ActionLog();
                    log.GameAction = GameAction.None;
                    log.SkillAction = this;
                    log.Source = Owner;
                    log.CardAction = c;
                    Game.CurrentGame.NotificationProxy.NotifySkillUse(log);
                    List<Card> cc = new List<Card>();
                    cc.Add(c);
                    Game.CurrentGame.HandleCardTransferToHand(null, Owner, cc);
                    eventArgs.Cards.Remove(c);
                }
            }
        }

        public JuXiang()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.ReadonlyCard.Type is NanManRuQin; },
                (p, e, a) => { throw new TriggerResultException(TriggerResult.End); },
                TriggerCondition.OwnerIsTarget
            );
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.Global
            ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.CardUsageTargetValidating, trigger);
            Triggers.Add(GameEvent.CardsEnteringDiscardDeck, trigger2);
            IsEnforced = true;
        }

    }
}