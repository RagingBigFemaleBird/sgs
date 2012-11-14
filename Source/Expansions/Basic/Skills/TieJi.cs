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
    /// 铁骑-当你使用【杀】指定了一名角色为目标后，你可以进行一次判定，若结果为红色，此【杀】不可被【闪】响应。
    /// </summary>
    public class TieJi : TriggerSkill
    {
        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            var card = Game.CurrentGame.Judge(Owner);
            if (card.SuitColor == SuitColorType.Red)
            {
                eventArgs.ReadonlyCard[ShaCancelling.CannotProvideShan] = 1;
            }
        }

        public TieJi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.ReadonlyCard.Type is Sha; },
                Run,
                TriggerCondition.OwnerIsSource
            );

            Triggers.Add(GameEvent.PlayerIsCardTargetConfirmed, trigger);
        }

    }
}
