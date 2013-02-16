using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;
using Sanguosha.Expansions.Basic.Skills;

namespace Sanguosha.Expansions.Woods.Skills
{
    /// <summary>
    /// 无前―出牌阶段，你可以弃2枚“暴怒”标记并选择一名其他角色，该角色的防具无效且你获得技能“无双”，直到回合结束。
    /// </summary>
    public class WuQian : ActiveSkill
    {
        class WuShuangRemoval : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner)
                {
                    return;
                }
                Game.CurrentGame.PlayerLoseAdditionalSkill(Owner, WqWuShuang);
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhaseEndEvents[TurnPhase.End], this);
            }

            public WuShuangRemoval(Player p)
            {
                Owner = p;
            }
        }

        class ArmorFailureRemoval : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner)
                {
                    return;
                }
                List<Player> players = Game.CurrentGame.AlivePlayers;
                players.Remove(Owner);
                foreach (Player p in players)
                {
                    if (p[WuQianTarget] > 0)
                    {
                        while (p[WuQianTarget]-- > 0)
                            p[Armor.UnconditionalIgnoreArmor]--;
                    }
                }
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhaseEndEvents[TurnPhase.End], this);
            }

            public ArmorFailureRemoval(Player p)
            {
                Owner = p;
            }
        }

        public override VerifierResult Validate(GameEventArgs arg)
        {
            if (Owner[KuangBao.BaoNuMark] < 2)
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
            if (arg.Targets[0] == Owner)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[KuangBao.BaoNuMark] -= 2;
            if (Owner[WuQianUsed] == 0)
            {
                Owner[WuQianUsed] = 1;
                Game.CurrentGame.PlayerAcquireAdditionalSkill(Owner, WqWuShuang, HeroTag);
                Game.CurrentGame.RegisterTrigger(GameEvent.PhaseEndEvents[TurnPhase.End], new WuShuangRemoval(Owner));
                Game.CurrentGame.RegisterTrigger(GameEvent.PhaseEndEvents[TurnPhase.End], new ArmorFailureRemoval(Owner));
            }
            arg.Targets[0][Armor.UnconditionalIgnoreArmor]++;
            arg.Targets[0][WuQianTarget]++;
            return true;
        }
        
        private static ISkill WqWuShuang = new WuShuang();
        private static PlayerAttribute WuQianUsed = PlayerAttribute.Register("WuQianUsed", true);
        private static PlayerAttribute WuQianTarget = PlayerAttribute.Register("WuQianTarget");
    }
}
