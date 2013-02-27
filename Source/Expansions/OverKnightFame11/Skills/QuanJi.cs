using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    /// 权计-每当你受到1点伤害后，你可以摸一张牌，然后将一张手牌置于你的武将牌上，称为“权”；每有一张“权”，你的手牌上限+1。
    /// </summary>
    public class QuanJi : TriggerSkill
    {
        class QuanJiVerifier : CardsAndTargetsVerifier
        {
            public QuanJiVerifier()
            {
                MinCards = 1;
                MaxCards = 1;
                MinPlayers = 0;
                MaxPlayers = 0;
                Discarding = false;
            }
            protected override bool VerifyCard(Player source, Card card)
            {
                return card.Place.DeckType == DeckType.Hand;
            }
        }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            var args = eventArgs as DamageEventArgs;
            int damage = args.Magnitude;
            while (damage-- > 0)
            {
                if (!AskForSkillUse())
                    break;

                Game.CurrentGame.DrawCards(Owner, 1);
                if (Owner.HandCards().Count == 0) continue;

                ISkill skill;
                List<Card> cards;
                List<Player> players;
                if (!Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("QuanJi"), new QuanJiVerifier(), out skill, out cards, out players))
                {
                    cards = new List<Card>();
                    cards.Add(Owner.HandCards().First());
                }
                Game.CurrentGame.HandleCardTransfer(Owner, Owner, QuanDeck, cards, HeroTag);
            }
        }

        public QuanJi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    return Game.CurrentGame.Decks[p, QuanDeck].Count > 0;
                },
                (p, e, a) =>
                {
                    var args = a as AdjustmentEventArgs;
                    args.AdjustmentAmount += Game.CurrentGame.Decks[p, QuanDeck].Count;
                },
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };

            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.OwnerIsTarget
            ) { AskForConfirmation = false };

            Triggers.Add(GameEvent.PlayerHandCardCapacityAdjustment, trigger);
            Triggers.Add(GameEvent.AfterDamageInflicted, trigger2);
            IsAutoInvoked = true;
            DeckCleanup.Add(QuanDeck);
        }

        public static PrivateDeckType QuanDeck = new PrivateDeckType("Quan", true);
    }
}
