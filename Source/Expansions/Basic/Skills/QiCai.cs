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
        class QiCaiTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Trace.Assert(eventArgs != null);
                if (eventArgs.Targets.IndexOf(Owner) < 0)
                {
                    return;
                }
                if (CardCategoryManager.IsCardCategory(eventArgs.Card.Type.Category, CardCategory.Tool))
                {
                    eventArgs.IntArg += 16;
                }
                return;
            }
        }

        public QiCai()
        {
            Triggers.Add(GameEvent.CardRangeModifier, new QiCaiTrigger());
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
