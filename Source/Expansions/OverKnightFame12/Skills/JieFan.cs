using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;
using System.Diagnostics;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Utils;

namespace Sanguosha.Expansions.OverKnightFame12.Skills
{
    /// <summary>
    /// 解烦-限定技，出牌阶段，你可以指定一名角色，攻击范围内含有该角色的所有角色须依次选择一项：弃置一张武器牌，或令该角色摸一张牌。
    /// </summary>
    public class JieFan : AutoVerifiedActiveSkill
    {
        public JieFan()
        {
            MaxCards = 0;
            MaxPlayers = 1;
            MinPlayers = 1;
            IsSingleUse = true;
        }

        protected override bool VerifyCard(Player source, Card card)
        {
            return false;
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            return true;
        }

        protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            return Owner[JieFanUsed] == 0;
        }

        class JieFanVerifier : CardsAndTargetsVerifier
        {
            public JieFanVerifier()
            {
                MaxCards = 1;
                MinCards = 1;
                MaxPlayers = 0;
                Discarding = true;
            }

            protected override bool VerifyCard(Player source, Card card)
            {
                return card.Type.IsCardCategory(CardCategory.Weapon);
            }
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[JieFanUsed] = 1;
            Player target = arg.Targets[0];
            List<Player> players = new List<Player>();
            foreach (Player p in Game.CurrentGame.AlivePlayers)
            {
                if (Game.CurrentGame.DistanceTo(p, target) <= p[Player.AttackRange] + 1)
                    players.Add(p);
            }
            foreach (Player p in players)
            {
                ISkill skill;
                List<Card> cards;
                List<Player> nPlayers;
                if (p.AskForCardUsage(new CardUsagePrompt("JieFan", target), new JieFanVerifier(), out skill, out cards, out nPlayers))
                {
                    Game.CurrentGame.HandleCardDiscard(p, cards);
                }
                else if (!target.IsDead)
                {
                    Game.CurrentGame.DrawCards(target, 1);
                }
            }
            return true;
        }

        private PlayerAttribute JieFanUsed = PlayerAttribute.Register("JieFanUsed");
    }
}
