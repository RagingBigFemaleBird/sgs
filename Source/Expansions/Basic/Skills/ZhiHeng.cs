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
    /// 制衡-出牌阶段，你可以弃置任意数量的牌，然后摸等量的牌，每阶段限一次。 
    /// </summary>
    public class ZhiHeng : AutoVerifiedActiveSkill
    {
        public override bool Commit(GameEventArgs arg)
        {
            Owner[ZhiHengUsed] = 1;
            List<Card> cards = arg.Cards;
            Trace.Assert(cards.Count > 0);
            int toDraw = cards.Count;
            Game.CurrentGame.HandleCardDiscard(Owner, cards);
            Game.CurrentGame.DrawCards(Owner, toDraw);
            return true;
        }

        public static PlayerAttribute ZhiHengUsed = PlayerAttribute.Register("ZhiHengUsed", true);

        public ZhiHeng()
        {
            MinCards = 1;
            MaxPlayers = 0;
            Discarding = true;
        }

        protected override bool AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            return source[ZhiHengUsed] == 0;
        }

        protected override bool VerifyCard(Player source, Card card)
        {
            return true;
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            return true;
        }
    }
}
