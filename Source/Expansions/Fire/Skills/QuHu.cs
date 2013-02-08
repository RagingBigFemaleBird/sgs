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
    /// 驱虎―出牌阶段，你可以与一名体力比你多的角色拼点，若你赢，则该角色对其攻击范围内你指定的另一名角色造成1点伤害。若你没赢，则其对你造成1点伤害。每阶段限一次。
    /// </summary>
    public class QuHu : ActiveSkill
    {
        public override VerifierResult Validate(GameEventArgs arg)
        {
            if (Owner[QuHuUsed] != 0 || Owner.HandCards().Count == 0)
            {
                return VerifierResult.Fail;
            }
            List<Card> cards = arg.Cards;
            if (cards != null && cards.Count > 0)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets != null && arg.Targets.Count > 1)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets == null || arg.Targets.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (arg.Targets[0] == Owner || arg.Targets[0].Health <= Owner.Health || Game.CurrentGame.Decks[arg.Targets[0], DeckType.Hand].Count == 0)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        class QuHuDamageTargetVerifier : CardsAndTargetsVerifier
        {
            public QuHuDamageTargetVerifier(Player t)
            {
                MinCards = 0;
                MaxCards = 0;
                MinPlayers = 1;
                MaxPlayers = 1;
                Discarding = false;
                target = t;
            }
            protected override bool VerifyPlayer(Player source, Player player)
            {
                return target != player && Game.CurrentGame.DistanceTo(target, player) <= target[Player.AttackRange] + 1;
            }
            Player target;
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[QuHuUsed] = 1;
            var result = Game.CurrentGame.PinDian(Owner, arg.Targets[0], this);
            if (result == true)
            {
                ISkill skill;
                List<Card> cards;
                List<Player> players;
                if (!Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("QuHu"), new QuHuDamageTargetVerifier(arg.Targets[0]), out skill, out cards, out players))
                {
                    players = new List<Player>();
                    foreach (Player p in Game.CurrentGame.AlivePlayers)
                    {
                        if (arg.Targets[0] != p && Game.CurrentGame.DistanceTo(arg.Targets[0], p) <= arg.Targets[0][Player.AttackRange] + 1)
                        {
                            players.Add(p);
                            break;
                        }
                    }
                }
                if (players != null && players.Count > 0)
                {
                    Game.CurrentGame.DoDamage(arg.Targets[0], players[0], 1, DamageElement.None, null, null);
                }
            }
            else
            {
                Game.CurrentGame.DoDamage(arg.Targets[0], Owner, 1, DamageElement.None, null, null);
            }
            return true;
        }

        private static PlayerAttribute QuHuUsed = PlayerAttribute.Register("QuHuUsed", true);

    }
}
