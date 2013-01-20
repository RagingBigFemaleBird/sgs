using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.BronzeSparrowTerrace.Skills
{
    /// <summary>
    /// 密诏-出牌阶段，你可以将所有手牌交给一名其他角色，令该角色与你选择的另一名角色拼点，视为点数大的角色对点数小的角色使用一张【杀】（无距离限制）。每阶段限一次。
    /// </summary>
    public class MiZhao : AutoVerifiedActiveSkill
    {
        public MiZhao()
        {
            MaxPlayers = 1;
            MinPlayers = 1;
            Helper.NoCardReveal = true;
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            return true;
        }

        protected override bool VerifyCard(Player source, Card card)
        {
            return card.Place.DeckType == DeckType.Hand;
        }

        protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            if (source[MiZhaoUsed] != 0 || source.HandCards().Count == 0) return false;
            if (cards != null && cards.Count < source.HandCards().Count) return null;
            return true;
        }

        public override bool Commit(GameEventArgs arg)
        {
            return true;
        }

        private static PlayerAttribute MiZhaoUsed = PlayerAttribute.Register("MiZhaoUsed", true);
    }
}
