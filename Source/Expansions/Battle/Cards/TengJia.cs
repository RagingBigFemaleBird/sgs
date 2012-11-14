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
    [Serializable]
    public class TengJia : Armor
    {
        public class TengJiaSkill : ArmorTriggerSkill
        {
            void RunBurnToDeath(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
            {
                eventArgs.IntArg3--;
            }

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
                    (p, e, a) => { return (DamageElement)a.IntArg2 == DamageElement.Fire && a.Card[Armor.IgnoreAllArmor] == 0 && a.Card[Armor.IgnorePlayerArmor] != Owner.Id + 1; },
                    RunBurnToDeath,
                    TriggerCondition.OwnerIsTarget
                );
                Triggers.Add(GameEvent.PlayerIsCardTargetInvalidated, trigger);
                Triggers.Add(GameEvent.DamageInflicted, trigger2);
            }

            public override bool IsEnforced
            {
                get
                {
                    return true;
                }
            }
        }

        public TengJia()
        {
            EquipmentSkill = new TengJiaSkill();
        }

    }
}
