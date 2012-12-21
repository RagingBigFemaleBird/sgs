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

namespace Sanguosha.Expansions.Woods.Skills
{
    /// <summary>
    /// 乱武-限定技，出牌阶段，可令所有其他角色各选择一项：对与其距离最近的另一名角色使用一张【杀】，或失去1点体力。
    /// </summary>
    public class LuanWu : ActiveSkill
    {
        public LuanWu()
        {
            UiHelper.HasNoConfirmation = true;
            IsSingleUse = true;
        }

        private static PlayerAttribute LuanWuUsed = PlayerAttribute.Register("LuanWuUsed", false);

        public override VerifierResult Validate(GameEventArgs arg)
        {
            if (Owner[LuanWuUsed] != 0)
            {
                return VerifierResult.Fail;
            }
            if (arg.Cards != null && arg.Cards.Count != 0)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets != null && arg.Targets.Count != 0)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[LuanWuUsed] = 1;
            var toProcess = Game.CurrentGame.AlivePlayers;
            toProcess.Remove(Owner);
            Game.CurrentGame.SortByOrderOfComputation(Owner, toProcess);
            foreach (Player target in toProcess)
            {
                ISkill skill;
                List<Card> cards;
                List<Player> players;
                if (target.IsDead) continue;
                if (Game.CurrentGame.UiProxies[target].AskForCardUsage(new CardUsagePrompt("LuanWu"), new LuanWuVerifier(),
                    out skill, out cards, out players))
                {
                    while (true)
                    {
                        try
                        {
                            GameEventArgs args = new GameEventArgs();
                            target[Sha.NumberOfShaUsed]--;
                            args.Source = target;
                            args.Targets = players;
                            args.Skill = skill;
                            args.Cards = cards;
                            Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
                        }
                        catch (TriggerResultException e)
                        {
                            Trace.Assert(e.Status == TriggerResult.Retry);
                            continue;
                        }
                        break;
                    }
                }
                else
                {
                    Game.CurrentGame.LoseHealth(target, 1);
                }
            }
            return true;
        }

        class LuanWuVerifier : CardUsageVerifier
        {
            public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                if (!Game.CurrentGame.AllAlive(players))
                {
                    return VerifierResult.Fail;
                }
                if (players != null)
                {
                    var toProcess = Game.CurrentGame.AlivePlayers;
                    toProcess.Remove(source);
                    List<Player> closest = new List<Player>();
                    int minRange = int.MaxValue;
                    foreach (Player p in toProcess)
                    {
                        int dist = Game.CurrentGame.DistanceTo(source, p);
                        if (dist < minRange)
                        {
                            closest.Clear();
                            closest.Add(p);
                            minRange = dist;
                        }
                        else if (Game.CurrentGame.DistanceTo(source, p) == minRange)
                        {
                            closest.Add(p);
                        }
                    }
                    if (players != null && players.Count > 0 && !closest.Any(p => players.Contains(p)))
                    {
                        return VerifierResult.Fail;
                    }
                }
                return (new Sha()).Verify(source, skill, cards, players);
            }

            public override IList<CardHandler> AcceptableCardTypes
            {
                get { return new List<CardHandler>() {new Sha()}; }
            }
        }
    }
}
