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
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Woods.Skills
{
    /// <summary>
    /// 祸首-锁定技，【南蛮入侵】对你无效；你是任何【南蛮入侵】造成伤害的来源。
    /// </summary>
    public class HuoShou : TriggerSkill
    {
        public HuoShou()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.ReadonlyCard.Type is NanManRuQin; },
                (p, e, a) => { throw new TriggerResultException(TriggerResult.End); },
                TriggerCondition.OwnerIsTarget
            );
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.ReadonlyCard.Type is NanManRuQin; },
                (p, e, a) => { a.Source = Owner; },
                TriggerCondition.Global
            );
            Triggers.Add(GameEvent.CardUsageTargetValidating, trigger);
            Triggers.Add(GameEvent.CardUsageTargetConfirmed, trigger2);
            IsEnforced = true;
        }

    }
}
