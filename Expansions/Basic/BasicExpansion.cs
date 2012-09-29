using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Heroes;

namespace Sanguosha.Expansions.Basic
{
    public class BasicExpansion : Expansion
    {
        public BasicExpansion()
        {
            CardHandlers = new Dictionary<string, CardHandler>();
            CardHandlers.Add(new GuoHeChaiQiao().CardType, new GuoHeChaiQiao());
            CardHandlers.Add(new ShunShouQianYang().CardType, new ShunShouQianYang());
            CardSet = new List<Card>();
        }
    }
}
