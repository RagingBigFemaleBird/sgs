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
                    bool rulerIsCaoCao = Game.CurrentGame.AlivePlayers.Any(pl => pl.Role == Role.Ruler && (pl.Hero.Name.Contains("CaoCao") || pl.Hero2 != null && pl.Hero2.Name.Contains("CaoCao")));
                    return rulerIsCaoCao && p[DanJiAwaken] == 0 && Game.CurrentGame.Decks[p, DeckType.Hand].Count > p.Health;
                },
                (p, e, a) =>
                {
                    p[DanJiAwaken] = 1;
                    Game.CurrentGame.LoseMaxHealth(p, 1);
                    Game.CurrentGame.PlayerAcquireAdditionalSkill(p, new MaShu(), HeroTag);
                },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Start], trigger);
            IsAwakening = true;
        }

        public static PlayerAttribute DanJiAwaken = PlayerAttribute.Register("DanJiAwaken", false);
    }
}
