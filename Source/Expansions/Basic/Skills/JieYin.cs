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

namespace Sanguosha.Expansions.Basic.Skills
{
    /// <summary>
    /// 结姻―出牌阶段，你可以弃置两张手牌并选择一名已受伤的男性角色，你与其各回复1点体力。每阶段限一次。
    /// </summary>
    public class JieYin : AutoVerifiedActiveSkill
    {
        public override bool Commit(GameEventArgs arg)
        {
            Owner[JieYinUsed] = 1;
            List<Card> cards = arg.Cards;
            Trace.Assert(cards.Count == 2 && arg.Targets.Count == 1);
            NotifyAction(arg.Source, arg.Targets, cards);
            Game.CurrentGame.HandleCardDiscard(Owner, cards);
            Game.CurrentGame.RecoverHealth(Owner, Owner, 1);
            Game.CurrentGame.RecoverHealth(Owner, arg.Targets[0], 1);
            return true;
        }

        public static PlayerAttribute JieYinUsed = PlayerAttribute.Register("JieYinUsed", true);

        protected override bool VerifyCard(Player source, Card card)
        {
            return card.Place.DeckType == DeckType.Hand;
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            return player.IsMale && player.Health < player.MaxHealth;
        }

        public JieYin()
        {
            MinCards = 2;
            MaxCards = 2;
            MinPlayers = 1;
            MaxPlayers = 1;
            Discarding = true;
        }

        protected override bool AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            return source[JieYinUsed] == 0;
        }
    }
}

