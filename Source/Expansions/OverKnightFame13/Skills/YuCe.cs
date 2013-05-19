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
    /// 御策-每当你受到一次伤害后，你可以展示一张手牌，令伤害来源弃置一张相同类别的手牌，否则，你回复1点体力。
    /// </summary>
    public class YuCe : TriggerSkill
    {
        class YuCeVerfier : CardsAndTargetsVerifier
        {
            CardCategory c = CardCategory.Unknown;
            public YuCeVerfier(CardCategory c = CardCategory.Unknown)
            {
                MaxCards = 1;
                MinCards = 1;
                MaxPlayers = 0;
                this.c = c;
            }

            protected override bool VerifyCard(Player source, Card card)
            {
                return (c == CardCategory.Unknown ? true : card.Type.BaseCategory() != c) && card.Place.DeckType == DeckType.Hand;
            }
        }
        public YuCe()
        {
            var trigger = new AutoNotifyUsagePassiveSkillTrigger(
                this,
                (p, e, a) => { return p.HandCards().Count > 0 && a.Source != null; },
                (p, e, a, cards, players) =>
                {
                    ISkill skill;
                    List<Card> nCards;
                    List<Player> nPlayer;
                    Game.CurrentGame.SyncImmutableCardAll(cards.First());
                    Game.CurrentGame.NotificationProxy.NotifyShowCard(p, cards.First());
                    if (a.Source.AskForCardUsage(new CardUsagePrompt("YuCeSource"), new YuCeVerfier(cards[0].Type.BaseCategory()), out skill, out nCards, out nPlayer))
                    {
                        Game.CurrentGame.HandleCardDiscard(a.Source, nCards);
                    }
                    else
                    {
                        Game.CurrentGame.RecoverHealth(p, p, 1);
                    }
                },
                TriggerCondition.OwnerIsTarget,
                new YuCeVerfier()
            ) { AskForConfirmation = false };
            Triggers.Add(GameEvent.AfterDamageInflicted, trigger);

            IsAutoInvoked = null;
        }
    }
}
