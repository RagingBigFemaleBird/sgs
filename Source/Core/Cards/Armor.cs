using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Players;

namespace Sanguosha.Core.Cards
{
    public abstract class Armor : Equipment
    {
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
