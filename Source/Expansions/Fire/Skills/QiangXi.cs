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

namespace Sanguosha.Expansions.Fire.Skills
{
    /// <summary>
    /// 强袭-出牌阶段，你可以失去一点体力或弃置一张武器牌，并对你攻击范围内的一名角色造成1点伤害。每阶段限一次。
    /// </summary>
    public class QiangXi : AutoVerifiedActiveSkill
    {
        public override bool Commit(GameEventArgs arg)
        {
            Owner[QiangXiUsed] = 1;
            List<Card> cards = arg.Cards;
            if (cards.Count > 0)
            {
                Game.CurrentGame.HandleCardDiscard(Owner, cards);
            }
            else
            {
                Game.CurrentGame.LoseHealth(Owner, 1);
            }
            Game.CurrentGame.DoDamage(arg.Source, arg.Targets[0], 1, DamageElement.None, null, null);
            return true;
        }

        public static PlayerAttribute QiangXiUsed = PlayerAttribute.Register("QiangXiUsed", true);

        public QiangXi()
        {
            MinCards = 0;
            MaxCards = 1;
            MaxPlayers = 1;
            MinPlayers = 1;
            Discarding = true;
        }

        protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            if (source[QiangXiUsed] != 0) return false;
            if (cards != null && cards.Count > 0)
            {
                if (cards[0].Place.DeckType == DeckType.Equipment)
                {
                    if (players != null && players.Count > 0)
                    {
                        return Game.CurrentGame.DistanceTo(source, players[0]) <= 1;
                    }
                }
            }
            if (players != null && players.Count > 0)
            {
                return Game.CurrentGame.DistanceTo(source, players[0]) <= source[Player.AttackRange] + 1;
            }
            return true;
        }

        protected override bool VerifyCard(Player source, Card card)
        {
            return card.Type is Weapon;
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            return true;
        }

    }
}
