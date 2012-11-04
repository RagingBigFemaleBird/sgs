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
    /// 谦逊-锁定技，你不能取得游戏的胜利。
    /// </summary>
    public class QianXun : TriggerSkill
    {
        class QianXunTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Trace.Assert(eventArgs != null);
                if (eventArgs.Targets.IndexOf(Owner) < 0)
                {
                    return;
                }
                if ((eventArgs.Card.Type is ShunShouQianYang) || (eventArgs.Card.Type is LeBuSiShu))
                {
                    throw new TriggerResultException(TriggerResult.Fail);
                }
                return;
            }
        }

        public QianXun()
        {
            Triggers.Add(GameEvent.PlayerCanBeTargeted, new QianXunTrigger());
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
