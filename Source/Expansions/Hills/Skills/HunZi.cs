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
using Sanguosha.Expansions.Woods.Skills;

namespace Sanguosha.Expansions.Hills.Skills
{
    /// <summary>
    /// 魂姿-觉醒技，回合开始阶段开始时，若你的体力为1，你须减1点体力上限，并获得技能“英姿”和“英魂”。
    /// </summary>
    public class HunZi : TriggerSkill
    {
        public HunZi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return p[HunZiAwakened] == 0 && p.Health == 1; },
                (p, e, a) => { p[HunZiAwakened] = 1; Game.CurrentGame.LoseMaxHealth(p, 1); Game.CurrentGame.PlayerAcquireAdditionalSkill(p, new YingZi(), HeroTag); Game.CurrentGame.PlayerAcquireAdditionalSkill(p, new YingHun(), HeroTag); },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Start], trigger);
            IsAwakening = true;
        }
        public static PlayerAttribute HunZiAwakened = PlayerAttribute.Register("HunZiAwakened");
    }
}
