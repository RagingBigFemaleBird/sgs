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

namespace Sanguosha.Expansions.Hills.Skills
{
    /// <summary>
    /// 挑衅-出牌阶段，你可以指定一名你在其攻击范围内的角色，该角色选择一项：对你使用一张【杀】，或令你弃置其一张牌，每阶段限一次。
    /// </summary>
    public class TiaoXin : AutoVerifiedActiveSkill
    {
        public override bool Commit(GameEventArgs arg)
        {
            Owner[TiaoXinUsed] = 1;
            var target = arg.Targets[0];
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            while (true)
            {
                if (Game.CurrentGame.UiProxies[target].AskForCardUsage(new CardUsagePrompt("TiaoXin", Owner), new JieDaoShaRen.JieDaoShaRenVerifier(Owner),
                    out skill, out cards, out players))
                {
                    try
                    {
                        GameEventArgs args = new GameEventArgs();
                        target[Sha.NumberOfShaUsed]--;
                        args.Source = target;
                        args.Targets = new List<Player>(players);
                        args.Targets.Add(Owner);
                        args.Skill = skill;
                        args.Cards = cards;
                        Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
                    }
                    catch (TriggerResultException e)
                    {
                        Trace.Assert(e.Status == TriggerResult.Retry);
                        continue;
                    }
                }
                else
                {
                    var theCard = Game.CurrentGame.SelectACardFrom(arg.Targets[0], Owner, new CardChoicePrompt("TiaoXin", Owner), "TiaoXin");
                    if (theCard != null )Game.CurrentGame.HandleCardDiscard(arg.Targets[0], new List<Card>() { theCard });
                }
                break;
            }
            return true;
        }

        public TiaoXin()
        {
            MinCards = 0;
            MaxCards = 0;
            MaxPlayers = 1;
            MinPlayers = 1;
        }

        public static PlayerAttribute TiaoXinUsed = PlayerAttribute.Register("TiaoXinUsed", true);

        protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            if (Owner[TiaoXinUsed] == 1) return false;
            return true;
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            if (Game.CurrentGame.DistanceTo(player, source) > player[Player.AttackRange] + 1) return false;
            return source != player;
        }

        protected override bool VerifyCard(Player source, Card card)
        {
            return true;
        }
    }
}
