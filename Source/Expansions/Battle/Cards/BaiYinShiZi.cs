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
    public class BaiYinShiZi : Armor
    {
        public class BaiYinShiZiSkill : ArmorTriggerSkill
        {
            void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
            {
                eventArgs.IntArg3 = 0;
            }

            public BaiYinShiZiSkill()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { return a.IntArg3 < 0 && a.Card[Armor.IgnoreAllArmor] == 0 && a.Card[Armor.IgnorePlayerArmor] != Owner.Id + 1; },
                    Run,
                    TriggerCondition.OwnerIsTarget
                );
                Triggers.Add(GameEvent.DamageInflicted, trigger);
            }

            public override bool IsEnforced
            {
                get
                {
                    return true;
                }
            }
        }

        public BaiYinShiZi()
        {
            EquipmentSkill = new BaiYinShiZiSkill();
        }

    }
}
