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
using Sanguosha.Expansions.Basic.Skills;
using Sanguosha.Expansions.SP.Skills;

namespace Sanguosha.Expansions.SP.Skills
{
    /// <summary>
    /// 单骑—觉醒技，回合开始阶段开始时，若你的手牌数大于你当前的体力值，且主公为曹操，你须减1点体力上限并获得技能“马术”。
    /// </summary>
    public class DanJi : TriggerSkill
    {
        public DanJi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => {
                    bool caoCaoZhu = false;
                    foreach (Player pl in Game.CurrentGame.Players)
                        if (pl.Role == Role.Ruler && pl.Hero.Name.Contains("CaoCao"))
                        {
                            caoCaoZhu = true;
                            break;
                        }
                    return p[DanJiAwaken] == 0
                        && Game.CurrentGame.Decks[p, DeckType.Hand].Count > p.Health
                        && caoCaoZhu;
                },
                (p, e, a) =>
                {
                    p[DanJiAwaken] = 1;
                    Game.CurrentGame.LoseMaxHealth(p, 1);
                    Game.CurrentGame.PlayerAcquireSkill(p, new MaShu());
                },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Start], trigger);
            IsAwakening = true;
        }

        public static PlayerAttribute DanJiAwaken = PlayerAttribute.Register("DanJiAwaken", false);
    }
}
