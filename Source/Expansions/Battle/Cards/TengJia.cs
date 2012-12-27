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
using Sanguosha.Expansions.Basic.Cards;
using System.Diagnostics;

namespace Sanguosha.Expansions.Battle.Cards
{
    
    public class TengJia : Armor
    {
        
        public class TengJiaSkill : ArmorTriggerSkill
        {            
            public TengJiaSkill()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { return a.ReadonlyCard != null && ((a.ReadonlyCard.Type is Aoe) || (a.ReadonlyCard.Type is RegularSha)) && a.ReadonlyCard[Armor.IgnoreAllArmor] == 0 && a.ReadonlyCard[Armor.IgnorePlayerArmor] != Owner.Id + 1; },
                    (p, e, a) => { throw new TriggerResultException(TriggerResult.End); },
                    TriggerCondition.OwnerIsTarget
                );
                var trigger2 = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) =>
                    {
                        var args = a as DamageEventArgs;
                        return (DamageElement)args.Element == DamageElement.Fire && a.ReadonlyCard[Armor.IgnoreAllArmor] == 0 && a.ReadonlyCard[Armor.IgnorePlayerArmor] != Owner.Id + 1;
                    },
                    (p, e, a) =>
                    {
                        var args = a as DamageEventArgs;
                        args.Magnitude++;
                    },
                    TriggerCondition.OwnerIsTarget
                );
                Triggers.Add(GameEvent.CardUsageTargetValidating, trigger);
                Triggers.Add(GameEvent.DamageInflicted, trigger2);
                IsEnforced = true;
            }
        }

        public TengJia()
        {
            EquipmentSkill = new TengJiaSkill() { ParentEquipment = this };
        }

    }
}
