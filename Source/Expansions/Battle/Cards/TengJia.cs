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
            void RunSha(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
            {
                eventArgs.IntArg3 = 1;
            }

            void RunAoe(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
            {
                throw new TriggerResultException(TriggerResult.Fail);
            }

            void RunBurnToDeath(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
            {
                eventArgs.IntArg3--;
            }

            public TengJiaSkill()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { return a.Card != null && (a.Card.Type is RegularSha) && a.Card[Armor.IgnoreAllArmor] == 0 && a.Card[Armor.IgnorePlayerArmor] != Owner.Id + 1; },
                    RunSha,
                    TriggerCondition.OwnerIsTarget
                );
                var trigger2 = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { return a.Card != null && a.Card.Type is Aoe && a.Card[Armor.IgnoreAllArmor] == 0 && a.Card[Armor.IgnorePlayerArmor] != Owner.Id + 1; },
                    RunAoe,
                    TriggerCondition.OwnerIsTarget
                );
                var trigger3 = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { return (DamageElement)a.IntArg2 == DamageElement.Fire && a.Card[Armor.IgnoreAllArmor] == 0 && a.Card[Armor.IgnorePlayerArmor] != Owner.Id + 1; },
                    RunBurnToDeath,
                    TriggerCondition.OwnerIsTarget
                );
                Triggers.Add(Sha.PlayerShaTargetShanModifier, trigger);
                Triggers.Add(GameEvent.PlayerIsCardTarget, trigger2);
                Triggers.Add(GameEvent.DamageInflicted, trigger3);
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
