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

namespace Sanguosha.Expansions.Battle.Cards
{
    public class RenWangDun : Armor
    {
        public class RenWangDunSkill : ArmorTriggerSkill
        {
            protected void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
            {
                eventArgs.IntArg3 = 1;
            }
            public RenWangDunSkill()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { return a.Card != null && a.Card.SuitColor == SuitColorType.Black && a.Card[Armor.IgnoreAllArmor] == 0 && a.Card[Armor.IgnorePlayerArmor] != Owner.Id + 1; },
                    Run,
                    TriggerCondition.OwnerIsTarget
                );
                Triggers.Add(Sha.PlayerShaTargetShanModifier, trigger);
            }

            public override bool IsEnforced
            {
                get
                {
                    return true;
                }
            }
        }

        public RenWangDun()
        {
            EquipmentSkill = new RenWangDunSkill();
        }

    }
}
