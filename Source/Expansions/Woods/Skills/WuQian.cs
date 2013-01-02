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
                Game.CurrentGame.PlayerLoseSkill(Owner, WqWuShuang);
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhaseEndEvents[TurnPhase.End], this);
            }

            public WuShuangRemoval(Player p)
            {
                Owner = p;
            }
        }

        /*关于防具无效部分尝未实现。*/

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
                Game.CurrentGame.PlayerAcquireSkill(Owner, WqWuShuang);
                Game.CurrentGame.RegisterTrigger(GameEvent.PhaseEndEvents[TurnPhase.End], new WuShuangRemoval(Owner));
            }
                //这里写目标防具无效部分代码，待完善。
            return true;
        }

        private static ISkill WqWuShuang = new WuShuang();
        private static PlayerAttribute WuQianUsed = PlayerAttribute.Register("WuQianUsed", true);
    }
}
