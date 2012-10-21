using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Players;

namespace Sanguosha.Core.Cards
{
    public abstract class Weapon : Equipment
    {
        protected override void RegisterEquipmentTriggers(Player p)
        {
            p[PlayerAttribute.AttackRange] += (AttackRange - 1);
            RegisterWeaponTriggers(p);
        }

        protected abstract void RegisterWeaponTriggers(Player p);

        protected override void UnregisterEquipmentTriggers(Player p)
        {
            p[PlayerAttribute.AttackRange] -= (AttackRange - 1);
            UnregisterWeaponTriggers(p);
        }

        protected abstract void UnregisterWeaponTriggers(Player p);

        public abstract int AttackRange { get; }
        public override CardCategory Category
        {
            get { return CardCategory.Weapon; }
        }

        protected override void Process(Player source, Players.Player dest, ICard card)
        {
            throw new NotImplementedException();
        }
    }
}
