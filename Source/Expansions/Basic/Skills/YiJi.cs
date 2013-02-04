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
    /// 遗计-你每受到1点伤害，可观看牌堆顶的两张牌，将其中一张交给一名角色，然后将另一张交给一名角色。
    /// </summary>
    public class YiJi : TriggerSkill
    {
        class YiJiVerifier : CardsAndTargetsVerifier
        {
            private List<Card> remainingCards;

            public YiJiVerifier(List<Card> remainingCards)
            {
                this.remainingCards = remainingCards;
                MaxPlayers = 1;
                MinPlayers = 1;
                MaxCards = 2;
                MinCards = 1;
                Helper.NoCardReveal = true;
            }

            protected override bool VerifyPlayer(Player source, Player player)
            {
                return source != player;
            }

            protected override bool VerifyCard(Player source, Card card)
            {
                return remainingCards.Contains(card);
            }
        }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            var args = eventArgs as DamageEventArgs;
            int damage = args.Magnitude;
            while (damage-- > 0)
            {
                if (AskForSkillUse())
                {
                    NotifySkillUse();
                    // hack the cards to owner's hand. do not trigger anything
                    Game.CurrentGame.SyncImmutableCard(Owner, Game.CurrentGame.PeekCard(0));
                    Card c1 = Game.CurrentGame.DrawCard();
                    Game.CurrentGame.SyncImmutableCard(Owner, Game.CurrentGame.PeekCard(0));
                    Card c2 = Game.CurrentGame.DrawCard();
                    CardsMovement move = new CardsMovement();
                    move.Cards = new List<Card>() { c1, c2 };
                    move.To = new DeckPlace(Owner, DeckType.Hand);
                    move.Helper.IsFakedMove = true;
                    Game.CurrentGame.MoveCards(move);
                    List<Card> remainingCards = new List<Card>() { c1, c2 };
                    Player giveSecondOneTo = null;
                    while (remainingCards.Count > 0)
                    {
                        if (giveSecondOneTo != null)
                        {
                            players = new List<Player>() { giveSecondOneTo };
                            cards = new List<Card>() { remainingCards[0] };
                        }
                        else if (!Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("YiJi"), new YiJiVerifier(remainingCards), out skill, out cards, out players))
                        {
                            players = new List<Player>() { Owner };
                            cards = new List<Card>() { remainingCards[0] };
                            giveSecondOneTo = Owner;
                        }
                        else if (cards.Count > 1)
                        {
                            Trace.Assert(cards.Count == 2);
                            cards.Remove(cards[1]);
                            giveSecondOneTo = players[0];
                        }
                        remainingCards.Remove(cards[0]);
                        Game.CurrentGame.InsertBeforeDeal(null, cards, new MovementHelper() { IsFakedMove = true });
                        Game.CurrentGame.DrawCards(players[0], 1);
                    }
                }
            }
        }


        public YiJi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.OwnerIsTarget
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.AfterDamageInflicted, trigger);
            IsAutoInvoked = true;
        }
    }
}
