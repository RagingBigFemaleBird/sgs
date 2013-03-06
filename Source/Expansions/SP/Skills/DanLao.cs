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

namespace Sanguosha.Expansions.SP.Skills
{
    /// <summary>
    /// 啖酪-当一个锦囊指定了包括你在内的多个目标，你可以立即摸一张牌，若如此做，该锦囊对你无效。
    /// </summary>
    public class DanLao : TriggerSkill
    {
        private static readonly CardAttribute DanLaoEffect = CardAttribute.Register("DanLao");

        public void OnPlayerIsCardTarget(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            Game.CurrentGame.DrawCards(owner, 1);
            eventArgs.ReadonlyCard[DanLaoEffect[owner]] = 1;
        }
        
        public DanLao()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { if (a.ReadonlyCard != null && !a.ReadonlyCard.Type.IsCardCategory(CardCategory.Tool)) return false; return a.Targets.Count > 1; },
                OnPlayerIsCardTarget,
                TriggerCondition.OwnerIsTarget
            );
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.ReadonlyCard != null && a.ReadonlyCard[DanLaoEffect[p]] == 1; },
                (p, e, a) => { throw new TriggerResultException(TriggerResult.End); },
                TriggerCondition.OwnerIsTarget
            ) { AskForConfirmation = false };
            Triggers.Add(GameEvent.CardUsageTargetValidating, trigger2);
            Triggers.Add(GameEvent.CardUsageTargetConfirming, trigger);
            IsAutoInvoked = null;
        }
    }
}
