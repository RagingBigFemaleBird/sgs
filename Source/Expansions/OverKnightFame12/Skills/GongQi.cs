using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;

namespace Sanguosha.Expansions.OverKnightFame12.Skills
{
    /// <summary>
    /// 弓骑-出牌阶段，你可以弃置一张牌，令你的攻击范围无限直到回合结束；若你以此法弃置的牌为装备牌，你可以弃置一名其他角色的一张牌。每阶段限一次。
    /// </summary>
    public class GongQi : AutoVerifiedActiveSkill
    {
        public GongQi()
        {
            MaxCards = 1;
            MinCards = 1;
            MaxPlayers = 1;
            Discarding = true;
        }

        protected override bool VerifyCard(Player source, Card card)
        {
            return true;
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            return player != source && player.HandCards().Count + player.Equipments().Count != 0;
        }

        protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            if (source[GongQiUsed] != 0)
            {
                return false;
            }
            if ((cards == null || cards.Count == 0) && players != null && players.Count > 0)
            {
                return false;
            }
            if (cards.Count == 1 && !cards[0].Type.IsCardCategory(CardCategory.Equipment) && players != null && players.Count > 0)
            {
                return false;
            }
            return true;
        }

        class GongQiRemoval : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner)
                {
                    return;
                }
                Owner[Player.AttackRange] -= 500;
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhasePostEnd, this);
            }
            public GongQiRemoval(Player p)
            {
                Owner = p;
            }
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[GongQiUsed] = 1;
            Game.CurrentGame.HandleCardDiscard(Owner, arg.Cards);
            Owner[Player.AttackRange] += 500;
            Game.CurrentGame.RegisterTrigger(GameEvent.PhasePostEnd, new GongQiRemoval(Owner));
            if (arg.Targets != null && arg.Targets.Count == 1)
            {
                Card toGet = Game.CurrentGame.SelectACardFrom(arg.Targets[0], Owner, new CardChoicePrompt("GongQi", arg.Targets[0], Owner), "HuoDe");
                Game.CurrentGame.HandleCardDiscard(arg.Targets[0], new List<Card>() { toGet });
            }
            return true;
        }

        private PlayerAttribute GongQiUsed = PlayerAttribute.Register("GongQiUsed", true);
    }
}
