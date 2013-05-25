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
    /// 峻刑-出牌阶段，你可以弃置至少一张手牌，然后令一名其他角色弃置一张与你所弃置牌类别均不同的手牌。若其不如此做，该角色将武将牌翻面并摸等同于你弃牌数的牌。每阶段限一次。
    /// </summary>
    public class JunXing : AutoVerifiedActiveSkill
    {
        public JunXing()
        {
            MaxCards = 3;
            MinCards = 1;
            MaxPlayers = 1;
            MinPlayers = 1;
            Discarding = true;
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            return player != source;
        }

        protected override bool VerifyCard(Player source, Card card)
        {
            return true;
        }

        protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            return source[JunXingUsed] == 0;
        }

        class JunXingVerifier : CardsAndTargetsVerifier
        {
            HashSet<CardCategory> cc;
            public JunXingVerifier(HashSet<CardCategory> baseCategory)
            {
                MaxCards = 1;
                MinCards = 1;
                MaxPlayers = 0;
                Discarding = true;
                cc = baseCategory;
            }

            protected override bool VerifyCard(Player source, Card card)
            {
                return !cc.Contains(card.Type.BaseCategory()) && card.Place.DeckType == DeckType.Hand;
            }
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[JunXingUsed] = 1;
            int count = arg.Cards.Count;
            HashSet<CardCategory> cc = new HashSet<CardCategory>();
            foreach (var c in arg.Cards)
            {
                cc.Add(c.Type.BaseCategory());
            }
            Game.CurrentGame.HandleCardDiscard(Owner, arg.Cards);

            Player target = arg.Targets.First();
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (target.AskForCardUsage(new CardUsagePrompt("JunXing"), new JunXingVerifier(cc), out skill, out cards, out players))
            {
                Game.CurrentGame.HandleCardDiscard(target, cards);
            }
            else
            {
                target.IsImprisoned = !target.IsImprisoned;
                Game.CurrentGame.DrawCards(target, count);
            }

            return true;
        }

        static PlayerAttribute JunXingUsed = PlayerAttribute.Register("JunXingUsed", true);
    }
}
