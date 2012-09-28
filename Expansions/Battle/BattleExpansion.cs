using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Heroes;

namespace Sanguosha.Expansions.Battle
{
    public class BattleExpansion : Expansion
    {
        public BattleExpansion()
        {
            CardHandlers = new Dictionary<string,CardHandler>();
            CardHandlers.Add(new HuoGong().CardType, new HuoGong());
            CardSet = new List<Card>();
        }
    }
}
