using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Players;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;

namespace Sanguosha.Expansions.SP.Skills
{
    /// <summary>    
    /// 星舞-弃牌阶段开始时，你可以将一张与你本回合使用的牌颜色均不同的手牌置于武将牌上。若此时你武将牌上的牌达到三张，则弃置这些牌，然后对一名男性角色造成2点伤害弃置其装备区中的所有牌。
    /// </summary>
    public class XingWu : TriggerSkill
    {
        class XingWuVerifier : CardsAndTargetsVerifier
        {
            public XingWuVerifier()
            {
                MaxCards = 1;
                MinCards = 1;
                MaxPlayers = 0;
            }

            protected override bool VerifyCard(Player source, Card card)
            {
                return card.Place.DeckType == DeckType.Hand && (source[CardSuitUsed] & (1 << (int)card.Suit)) != 1 << (int)card.Suit;
            }
        }

        class XingWuChoicePlayer : CardsAndTargetsVerifier
        {
            public XingWuChoicePlayer()
            {
                MaxCards = 0;
                MaxPlayers = 1;
                MinPlayers = 1;
            }

            protected override bool VerifyPlayer(Player source, Player player)
            {
                return player.IsMale;
            }
        }

        public XingWu()
        {
            var tagClear = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { p[CardSuitUsed] = 0; },
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false, AskForConfirmation = false };
            Triggers.Add(GameEvent.PhaseBeforeStart, tagClear);

            var cardUsed = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    var card = a.ReadonlyCard;
                    p[CardSuitUsed] |= 1 << (int)card.Suit;
                },
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false, AskForConfirmation = false };
            Triggers.Add(GameEvent.PlayerUsedCard, cardUsed);

            var trigger = new AutoNotifyUsagePassiveSkillTrigger(
                this,
                (p, e, a) => { return p.HandCards().Count > 0; },
                (p, e, a, cards, players) => 
                {
                    NotifySkillUse();
                    Game.CurrentGame.HandleCardTransfer(p, p, XingWuDeck, cards, HeroTag);
                    Game.CurrentGame.PlayerAcquiredCard(p, cards);
                    List<Card> XingWuCards = Game.CurrentGame.Decks[p, XingWuDeck];
                    if (XingWuCards.Count >= 3)
                    {
                        Game.CurrentGame.HandleCardDiscard(p, XingWuCards);
                        List<Player> aPlayers = Game.CurrentGame.AlivePlayers;
                        if (!aPlayers.Any(pl => pl.IsMale))
                        {
                            return;
                        }
                        ISkill skill;
                        List<Card> nCards;
                        List<Player> nPlayers;
                        if (!p.AskForCardUsage(new CardUsagePrompt("XingWuChoicePlayer"), new XingWuChoicePlayer(), out skill, out nCards, out nPlayers))
                        {
                            nPlayers.Add((from m in aPlayers where m.IsMale select m).First());
                        }
                        Player target = nPlayers.First();
                        NotifySkillUse(nPlayers);
                        Game.CurrentGame.DoDamage(p, target, 2, DamageElement.None, null, null);
                        Game.CurrentGame.HandleCardDiscard(target, target.Equipments());
                    }
                },
                TriggerCondition.OwnerIsSource,
                new XingWuVerifier()
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Discard], trigger);
            IsAutoInvoked = null;
            DeckCleanup.Add(XingWuDeck);
        }

        public static PrivateDeckType XingWuDeck = new PrivateDeckType("XingWu", true);
        private static PlayerAttribute CardSuitUsed = PlayerAttribute.Register("CardSuitUsed");
    }
}
