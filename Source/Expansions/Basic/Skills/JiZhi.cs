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
    /// 集智-当你使用一张非延时类锦囊牌时，你可以摸一张牌。
    /// </summary>
    public class JiZhi : TriggerSkill
    {
        public JiZhi()
        {
            Triggers.Add(GameEvent.PlayerUsedCard, new AutoNotifyPassiveSkillTrigger(this,
                (p, e, a) => { return CardCategoryManager.IsCardCategory(a.Card.Type.Category, CardCategory.ImmediateTool); },
                (p, e, a) => { Game.CurrentGame.DrawCards(p, 1); },
                TriggerCondition.OwnerIsSource
            ) { Type = TriggerType.Skill });
            IsAutoInvoked = true;
        }
    }
}
