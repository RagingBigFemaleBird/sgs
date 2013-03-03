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

namespace Sanguosha.Expansions.Basic.Skills
{
    /// <summary>
    /// 奇才-锁定技，你使用任何锦囊牌无距离限制。
    /// </summary>
    public class QiCai : TriggerSkill
    {
        public QiCai()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return CardCategoryManager.IsCardCategory(a.Card.Type.Category, CardCategory.Tool); },
                (p, e, a) => { (a as AdjustmentEventArgs).AdjustmentAmount += 500; },
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.CardRangeModifier, trigger);
            IsEnforced = true;
        }
    }
}
