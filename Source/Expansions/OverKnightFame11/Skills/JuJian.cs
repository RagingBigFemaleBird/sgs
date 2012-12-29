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

namespace Sanguosha.Expansions.OverKnightFame11.Skills
{
    /// <summary>
    /// 举荐-出牌阶段，你可以弃至多三张牌，然后让一名其他角色摸等量的牌。若你以此法弃牌不少于三张且均为同一类别，你回复1点体力。每回合限一次。
    /// </summary>
    public class JuJian : AutoVerifiedActiveSkill
    {
        public override bool Commit(GameEventArgs arg)
        {
            int count = arg.Cards.Count;
            bool regen = false;
            if (arg.Cards.Count >= 3)
            {
                CardCategory cat;
                if (CardCategoryManager.IsCardCategory(arg.Cards[0].Type.Category, CardCategory.Basic)) cat = CardCategory.Basic;
                else if (CardCategoryManager.IsCardCategory(arg.Cards[0].Type.Category, CardCategory.Tool)) cat = CardCategory.Tool;
                else cat = CardCategory.Equipment;
                if (!arg.Cards.Any(cd => !CardCategoryManager.IsCardCategory(cd.Type.Category, cat))) regen = true;
            }
            Game.CurrentGame.HandleCardDiscard(Owner, arg.Cards);
            Game.CurrentGame.DrawCards(arg.Targets[0], count);
            if (regen)
            {
                Game.CurrentGame.RecoverHealth(Owner, Owner, 1);
            }
            return true;
        }


        public JuJian()
        {
            MinCards = 1;
            MaxCards = 3;
            MaxPlayers = 1;
            MinPlayers = 1;
            Discarding = true;
        }
        protected override bool VerifyCard(Player source, Card card)
        {
            return true;
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            return source != player;
        }

        public override void CardRevealPolicy(Player p, List<Card> cards, List<Player> players)
        {
            foreach (Card c in cards)
            {
                c.RevealOnce = true;
            }
        }
    }
}
