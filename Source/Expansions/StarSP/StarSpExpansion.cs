using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Heroes;
using Sanguosha.Expansions.StarSP.Skills;

namespace Sanguosha.Expansions.StarSP
{
    public class StarSpExpansion : Expansion
    {
        public StarSpExpansion()
        {
            CardSet = new List<Card>();

            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("StarSPPangTong", true, Allegiance.Qun, 3, new ManJuan(), new ZuiXiang()))));
        }
    }
}