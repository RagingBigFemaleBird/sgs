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

namespace Sanguosha.Expansions.Basic.Cards
{
    public class JianYiMa : Equipment
    {
        public override void RegisterTriggers(Player p)
        {
            p[PlayerAttribute.RangeMinus]++;
        }

        public override void UnregisterTriggers(Player p)
        {
            p[PlayerAttribute.RangeMinus]--;
        }

        public override CardCategory Category
        {
            get { return CardCategory.OffsensiveHorse; }
        }

        protected override void Process(Player source, Player dest, ICard card)
        {
            throw new NotImplementedException();
        }
    }
}
