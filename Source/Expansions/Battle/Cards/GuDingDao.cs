using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.Battle.Cards
{
    
    public class GuDingDao : Weapon
    {
        public GuDingDao()
        {
            EquipmentSkill = new GuDingDaoSkill() { ParentEquipment = this };
        }

        
        public class GuDingDaoSkill : TriggerSkill, IEquipmentSkill
        {
            public Equipment ParentEquipment { get; set; }
            public GuDingDaoSkill()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) =>
                    {
                        return a.ReadonlyCard != null && a.ReadonlyCard.Type is Sha && Game.CurrentGame.Decks[a.Targets[0], DeckType.Hand].Count == 0 && (a as DamageEventArgs).OriginalTarget == a.Targets[0];
                    },
                    (p, e, a) => { (a as DamageEventArgs).Magnitude++; },
                    TriggerCondition.OwnerIsSource
                );
                Triggers.Add(GameEvent.DamageCaused, trigger);
                IsEnforced = true;
            }
        }

        public override int AttackRange
        {
            get { return 2; }
        }

        protected override void RegisterWeaponTriggers(Player p)
        {
            return;
        }

        protected override void UnregisterWeaponTriggers(Player p)
        {
            return;
        }

    }
}
