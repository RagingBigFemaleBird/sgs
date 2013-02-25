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
                    (p, e, a) => { return a.ReadonlyCard != null && ((a.ReadonlyCard.Type is Aoe) || (a.ReadonlyCard.Type is RegularSha)) && ArmorIsValid(Owner, a.Source, a.ReadonlyCard); },
                    (p, e, a) => { throw new TriggerResultException(TriggerResult.End); },
                    TriggerCondition.OwnerIsTarget
                );
                Triggers.Add(GameEvent.CardUsageTargetValidating, trigger);
                IsEnforced = true;
            }
        }

        public class TengJiaHurtSkill : ArmorTriggerSkill
        {
            public TengJiaHurtSkill()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) =>
                    {
                        var args = a as DamageEventArgs;
                        return (DamageElement)args.Element == DamageElement.Fire && ArmorIsValid(Owner, args.Source, a.ReadonlyCard);
                    },
                    (p, e, a) =>
                    {
                        var args = a as DamageEventArgs;
                        args.Magnitude++;
                    },
                    TriggerCondition.OwnerIsTarget
                );
                Triggers.Add(GameEvent.DamageInflicted, trigger);
                IsEnforced = true;
            }
        }

        TengJiaHurtSkill hurtTrigger;
        public TengJia()
        {
            EquipmentSkill = new TengJiaSkill() { ParentEquipment = this };
            hurtTrigger = new TengJiaHurtSkill() { ParentEquipment = this };
        }

        protected override void RegisterEquipmentTriggers(Player p)
        {
            hurtTrigger.Owner = p;
        }

        protected override void UnregisterEquipmentTriggers(Player p)
        {
            hurtTrigger.Owner = null;
        }

    }
}
