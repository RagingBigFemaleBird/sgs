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

namespace Sanguosha.Expansions.OverKnightFame11.Skills
{
    /// <summary>
    /// 明策-出牌阶段，你可以交给一名其他角色一张装备牌或【杀】，该角色选择一项：
    /// 1. 视为对其攻击范围内你选择的另一名角色使用一张【杀】。
    /// 2. 摸一张牌。
    /// 每阶段限一次。
    /// </summary>
    public class MingCe : AutoVerifiedActiveSkill
    {
        protected override bool VerifyCard(Player source, Card card)
        {
            return card.Type.IsCardCategory(CardCategory.Equipment) || card.Type is Sha;
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            return true;
        }

        protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            if (Owner[MingCeUsed] == 1 || players.Count == 1 && Owner == players[0]) return false;
            // you can choose only 1 player as target, iff this target cannot SHA anyone
            // i.e. you need to STOP returning Success (return Partial instead) if we have chosen card and one player and this player can SHA anyone
            if (cards.Count == 1 && players.Count == 1)
            {
                var pl = Game.CurrentGame.AlivePlayers;
                if (pl.Any(test => test != players[0] && Game.CurrentGame.DistanceTo(players[0], test) <= players[0][Player.AttackRange] + 1 &&
                    Game.CurrentGame.PlayerCanBeTargeted(players[0], new List<Player>() { test }, new CompositeCard() { Type = new Sha() }))) return null;
            }
            if (players.Count == 2)
            {
                if (!Game.CurrentGame.PlayerCanBeTargeted(players[0], new List<Player>() { players[1] }, new CompositeCard() { Type = new Sha() }))
                {
                    return false;
                }
                if (Game.CurrentGame.DistanceTo(players[0], players[1]) > players[0][Player.AttackRange] + 1) return false;
            }
            return true;
        }

        public MingCe()
        {
            MinCards = 1;
            MaxCards = 1;
            MinPlayers = 1;
            MaxPlayers = 2;
        }

        protected override void TargetsSplit(List<Player> targets, out List<Player> firstTargets, out List<Player> secondaryTargets)
        {
            firstTargets = new List<Player>() { targets[0] };
            secondaryTargets = null;
            if (targets.Count == 2) secondaryTargets = new List<Player>() { targets[1] };
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[MingCeUsed] = 1;
            Game.CurrentGame.HandleCardTransferToHand(Owner, arg.Targets[0], arg.Cards);
            if (arg.Targets.Count == 1)
            {
                Game.CurrentGame.DrawCards(arg.Targets[0], 1);
                return true;
            }
            int answer = 0;
            arg.Targets[0].AskForMultipleChoice(new MultipleChoicePrompt("MingCe"), new List<OptionPrompt>() { new OptionPrompt("MingCeSha", arg.Targets[1]), new OptionPrompt("MingCeMoPai") }, out answer);
            if (answer == 0)
            {
                Sha.UseDummyShaTo(arg.Targets[0], arg.Targets[1], new RegularSha(), new CardUsagePrompt("MingCe", arg.Targets[1]));
            }
            else
            {
                Game.CurrentGame.DrawCards(arg.Targets[0], 1);
            }
            return true;
        }

        private static PlayerAttribute MingCeUsed = PlayerAttribute.Register("MingCeUsed", true);
    }
}
