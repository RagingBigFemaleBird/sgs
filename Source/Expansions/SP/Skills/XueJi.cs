using System.Collections.Generic;
using System.Linq;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Expansions.Basic.Cards;
using System.Diagnostics;

namespace Sanguosha.Expansions.SP.Skills
{
    /// <summary>
    /// 血祭-出牌阶段，你可弃置一张红色牌，并对你攻击范围内的至多X名其他角色各造成1点伤害，然后这些角色各摸一张牌，X为你损失的体力值。每阶段限一次。
    /// </summary>
    public class XueJi : AutoVerifiedActiveSkill
    {
        public override bool Commit(GameEventArgs arg)
        {
            Owner[XueJiUsed] = 1;
            List<Card> cards = arg.Cards;
            Game.CurrentGame.HandleCardDiscard(Owner, cards);
            Game.CurrentGame.SortByOrderOfComputation(Owner, arg.Targets);
            foreach (var p in arg.Targets) Game.CurrentGame.DoDamage(arg.Source, p, 1, DamageElement.None, null, null);
            foreach (var p in arg.Targets) Game.CurrentGame.DrawCards(p, 1);
            return true;
        }

        public static PlayerAttribute XueJiUsed = PlayerAttribute.Register("XueJiUsed", true);

        public XueJi()
        {
            MinCards = 1;
            MaxCards = 1;
            MaxPlayers = int.MaxValue;
            MinPlayers = 1;
            Discarding = true;
        }

        protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            if (source[XueJiUsed] != 0 || source.LostHealth == 0) return false;
            if (players != null && players.Count > 0 && (cards == null || cards.Count == 0)) return false;
            if (players != null && source.LostHealth < players.Count) return false;
            var temp = new Sha();
            temp.HoldInTemp(cards);
            if (players.Any(p => Game.CurrentGame.DistanceTo(source, p) > source[Player.AttackRange] + 1))
            {
                temp.ReleaseHoldInTemp();
                return false;
            }
            temp.ReleaseHoldInTemp();
            return true;
        }

        protected override bool VerifyCard(Player source, Card card)
        {
            return card.SuitColor == SuitColorType.Red;
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            return player != source;
        }
    }
}
