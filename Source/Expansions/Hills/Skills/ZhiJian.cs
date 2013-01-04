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

namespace Sanguosha.Expansions.Hills.Skills
{
    /// <summary>
    /// 直谏-出牌阶段，你可以将手牌中的一张装备牌置于一名其他角色装备区里，然后摸一张牌。
    /// </summary>
    public class ZhiJian : AutoVerifiedActiveSkill
    {
        public override bool Commit(GameEventArgs arg)
        {
            CardsMovement move = new CardsMovement();
            move.Cards = new List<Card>(arg.Cards);
            move.To = new DeckPlace(arg.Targets[0], DeckType.Equipment);
            Game.CurrentGame.MoveCards(move);
            Game.CurrentGame.PlayerLostCard(Owner, arg.Cards);
            Game.CurrentGame.PlayerAcquiredCard(arg.Targets[0], arg.Cards);
            Game.CurrentGame.DrawCards(Owner, 1);
            return true;
        }


        public ZhiJian()
        {
            MinCards = 1;
            MaxCards = 1;
            MaxPlayers = 1;
            MinPlayers = 1;
        }

        protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            if (cards != null && cards.Count > 0)
            {
                if (players != null && players.Count > 0)
                {
                    if (Game.CurrentGame.Decks[players[0], DeckType.Equipment].Any(cd => CardCategoryManager.IsCardCategory(cd.Type.Category, cards[0].Type.Category)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        protected override bool VerifyCard(Player source, Card card)
        {
            return card.Place.DeckType == DeckType.Hand && CardCategoryManager.IsCardCategory(card.Type.Category, CardCategory.Equipment);
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            return source != player;
        }

    }
}
