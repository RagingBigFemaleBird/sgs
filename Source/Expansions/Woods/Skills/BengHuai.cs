using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;

namespace Sanguosha.Expansions.Woods.Skills
{
    /// <summary>
    /// 崩坏-锁定技，回合结束阶段开始时，若你的体力不是全场最少的(或之一)，你须减1点体力或体力上限。
    /// </summary>
    public class BengHuai : TriggerSkill
    {
        public BengHuai()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    int minHp = int.MaxValue;
                    foreach (Player pl in Game.CurrentGame.AlivePlayers)
                    {
                        if (pl.Health < minHp) minHp = pl.Health;
                    }
                    return p.Health > minHp;
                },
                (p, e, a) =>
                {
                    int answer = 0;
                    Game.CurrentGame.UiProxies[p].AskForMultipleChoice(new MultipleChoicePrompt("BengHuai"), new List<string>() { "TiLiZhi", "TiLiShangXian" }, out answer);
                    if (answer == 0)
                    {
                        Game.CurrentGame.LoseHealth(p, 1);
                    }
                    else
                    {
                        Game.CurrentGame.LoseMaxHealth(p, 1);
                    }
                },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.End], trigger);
        }
        public override bool IsEnforced
        {
            get
            {
                return true;
            }
        }
    }
}
