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
    /// 业炎-限定技，出牌阶段，可以指定一至三名角色，你分别对他们造成最多共3点火焰伤害(你可以任意分配)，若你将对1名角色分配2点或更多的火焰伤害，你须先弃置四张不同花色的手牌并失去3点体力。
    /// </summary>
    public class YeYan : AutoVerifiedActiveSkill
    {
        public override bool Commit(GameEventArgs arg)
        {
            Owner[YeYanUsed] = 1;
            List<Card> cards = arg.Cards;
            if (cards.Count > 0)
            {
                Game.CurrentGame.HandleCardDiscard(Owner, cards);
            }
            Game.CurrentGame.SortByOrderOfComputation(Owner, arg.Targets);
            Dictionary<Player, int> toDamage = new Dictionary<Player, int>();
            foreach (var p in arg.Targets)
            {
                if (!toDamage.ContainsKey(p))
                {
                    toDamage.Add(p, 0);
                }
                toDamage[p]++;
            }
            if (toDamage.Count > 0 && toDamage.Values.Max() > 1)
            {
                Game.CurrentGame.LoseHealth(arg.Source, 3);
            }
            foreach (var dmg in toDamage)
            {
                Game.CurrentGame.DoDamage(arg.Source, dmg.Key, dmg.Value, DamageElement.Fire, null, null);
            }
            return true;
        }

        public static PlayerAttribute YeYanUsed = PlayerAttribute.Register("YeYanUsed");

        public YeYan()
        {
            Helper.IsPlayerRepeatable = true;
            MinCards = 0;
            MaxCards = 4;
            MaxPlayers = 3;
            MinPlayers = 1;
            Discarding = true;
            IsSingleUse = true;
        }

        protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            if (source[YeYanUsed] != 0) return false;
            Dictionary<Player, int> toDamage = new Dictionary<Player, int>();
            foreach (var p in players)
            {
                if (!toDamage.ContainsKey(p))
                {
                    toDamage.Add(p, 0);
                }
                toDamage[p]++;
            }

            bool isGreatYeYan = toDamage.Count > 0 && toDamage.Values.Max() > 1;

            // First, verify if great yeyan is possible            
            var allSuits = (from card in Game.CurrentGame.Decks[source, DeckType.Hand]
                            select card.Suit).Distinct();
            if (allSuits.Count() < 4 && (cards.Count > 0 || isGreatYeYan)) return false;

            if (cards.Count > 0) // Great YeYan
            {
                if (toDamage.Count >= 3) return false;
                HashSet<SuitType> suits = new HashSet<SuitType>();
                foreach (var card in cards)
                {
                    if (suits.Contains(card.Suit))
                    {
                        return false;
                    }
                    suits.Add(card.Suit);
                }
                if (suits.Count == 4 && isGreatYeYan) return true;
                else return null;
            }
            else
            {
                if (isGreatYeYan) return null;
                if (toDamage.Count > 3) return false;
                else if (toDamage.Count == 0) return null;
                else return true;
            }
        }

        protected override bool VerifyCard(Player source, Card card)
        {
            return card.Place.DeckType == DeckType.Hand;
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            return true;
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
