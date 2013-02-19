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
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.SP.Skills
{
    /// <summary>    
    /// 颂词-出牌阶段，你可以选择一项：令一名手牌数小于其体力值的角色摸两张牌；或令一名手牌数大于其体力值的角色弃置两张牌。此技能对每名角色只能使用一次。
    /// </summary>
    class SongCi : AutoVerifiedActiveSkill
    {
        public SongCi()
        {
            MaxCards = 0;
            MinPlayers = 1;
            MaxPlayers = 1;
        }

        protected override int GenerateSpecialEffectHintIndex(Player source, List<Player> targets, List<Card> cards)
        {
            if (targets[0].HandCards().Count > Math.Max(targets[0].Health, 0)) return 1;
            return 0;
        }

        protected override bool VerifyCard(Player source, Card card)
        {
            return false;
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            return Math.Max(player.Health, 0) != player.HandCards().Count && source[SongCiTarget[player]] == 0;
        }

        protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            return Game.CurrentGame.AlivePlayers.Any(p => source[SongCiTarget[p]] == 0);
        }

        public override bool Commit(GameEventArgs arg)
        {
            Player target = arg.Targets[0];
            Owner[SongCiTarget[target]] = 1;
            target[SongCiStatus] = 1;
            if (target.HandCards().Count < target.Health)
            {
                Game.CurrentGame.DrawCards(target, 2);
            }
            else
            {
                int count = Math.Min(target.HandCards().Count + target.Equipments().Count, 2);
                Game.CurrentGame.ForcePlayerDiscard(target, (pl, d) => { return count - d; }, true);
            }
            return true;
        }

        private static PlayerAttribute SongCiTarget = PlayerAttribute.Register("SongCiTarget");
        private static PlayerAttribute SongCiStatus = PlayerAttribute.Register("SongCi", false, false, true);
    }
}
