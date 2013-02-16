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
    /// 若愚-主公技，觉醒技，回合开始阶段开始时，若你的体力是全场最少的(或之一)，你须加1点体力上限，回复1点体力，并获得技能“激将”。
    /// </summary>
    public class RuoYu : TriggerSkill
    {
        public RuoYu()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => 
                {
                    int minHp = int.MaxValue;
                    foreach (var pl in Game.CurrentGame.AlivePlayers) if (pl.Health < minHp) minHp = pl.Health;
                    return p[RuoYuAwakened] == 0 && p.Health == minHp; },
                (p, e, a) => 
                { 
                    p[RuoYuAwakened] = 1; 
                    p.MaxHealth++; 
                    Game.CurrentGame.RecoverHealth(p, p, 1);
                    Game.CurrentGame.PlayerAcquireAdditionalSkill(p, new JiJiang(), HeroTag); 
                },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Start], trigger);
            IsAwakening = true;
            IsRulerOnly = true;
        }
        public static PlayerAttribute RuoYuAwakened = PlayerAttribute.Register("RuoYuAwakened");
    }
}
