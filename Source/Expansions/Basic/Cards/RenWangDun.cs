using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;
using System.Diagnostics;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.Basic.Cards
{
    
    public class RenWangDun : Armor
    {
        
        public class RenWangDunSkill : ArmorTriggerSkill
        {
            public RenWangDunSkill()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { return a.ReadonlyCard != null && (a.ReadonlyCard.Type is Sha) && a.ReadonlyCard.SuitColor == SuitColorType.Black && a.ReadonlyCard[Armor.IgnoreAllArmor] == 0 && a.ReadonlyCard[Armor.IgnorePlayerArmor] != Owner.Id + 1; },
                    (p, e, a) => { throw new TriggerResultException(TriggerResult.End);},
                    TriggerCondition.OwnerIsTarget
                );
                Triggers.Add(GameEvent.CardUsageTargetValidating, trigger);
                IsEnforced = true;
            }
        }

        public RenWangDun()
        {
            EquipmentSkill = new RenWangDunSkill();
        }

    }
}
