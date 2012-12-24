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
    public class TanLao : TriggerSkill
    {
        class TanLaoVerifier : CardsAndTargetsVerifier
        {
            public TanLaoVerifier()
            {
                MinCards = 1;
                MaxCards = 1;
                MinPlayers = 0;
                MaxPlayers = 0;
                Discarding = true;
            }
            protected override bool VerifyCard(Player source, Card card)
            {
                return CardCategoryManager.IsCardCategory(card.Type.Category, CardCategory.Basic);
            }
        }

        public void OnPlayerIsCardTarget(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            Game.CurrentGame.DrawCards(owner, 1);
            eventArgs.ReadonlyCard[CardAttribute.Register("TanLao" + owner.Id)] = 1;
        }

        public TanLao()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Targets.Count > 1; },
                OnPlayerIsCardTarget,
                TriggerCondition.OwnerIsTarget
            );
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.ReadonlyCard != null && a.ReadonlyCard[CardAttribute.Register("TanLao" + p.Id)] == 1; },
                (p, e, a) => { throw new TriggerResultException(TriggerResult.End); },
                TriggerCondition.OwnerIsTarget
            ) { AskForConfirmation = false };
            Triggers.Add(GameEvent.CardUsageTargetValidating, trigger2);
            Triggers.Add(GameEvent.CardUsageTargetConfirming, trigger);
            IsAutoInvoked = null;
        }
    }
}
