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
using System.Diagnostics;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.OverKnightFame11.Skills
{
    /// <summary>
    /// 破军-每当你使用【杀】对目标角色造成一次伤害后，你可以令其摸X张牌（X为该角色当前的体力值且至多为5），然后该角色将其武将牌翻面。
    /// </summary>
    public class PoJun : TriggerSkill
    {
        public PoJun()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.ReadonlyCard != null && a.ReadonlyCard.Type is Sha && !a.Targets[0].IsDead && (a as DamageEventArgs).OriginalTarget == a.Targets[0]; },
                (p, e, a) => { Game.CurrentGame.DrawCards(a.Targets[0], Math.Min(5, a.Targets[0].Health)); a.Targets[0].IsImprisoned = !a.Targets[0].IsImprisoned; },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.AfterDamageCaused, trigger);
            IsAutoInvoked = false;
        }
    }
}
