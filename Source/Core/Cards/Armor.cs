using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Players;

namespace Sanguosha.Core.Cards
{
    public abstract class Armor : Equipment
    {
        public static readonly CardAttribute IgnoreAllArmor = CardAttribute.Register("IgnoreAllArmor");
        public static readonly CardAttribute IgnorePlayerArmor = CardAttribute.Register("IgnorePlayerArmor");
        public override CardCategory Category
        {
            get { return CardCategory.Armor; }
        }

        protected override void Process(Player source, Players.Player dest, ICard card)
        {
            throw new NotImplementedException();
        }
    }
}
