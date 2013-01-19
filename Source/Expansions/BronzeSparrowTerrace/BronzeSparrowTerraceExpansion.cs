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
using Sanguosha.Expansions.BronzeSparrowTerrace.Skills;

namespace Sanguosha.Expansions.BronzeSparrowTerrace
{
    public class BronzeSparrowTerraceExpansion : Expansion
    {
        public BronzeSparrowTerraceExpansion()
        {
            CardSet = new List<Card>();

            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("LingJu", false, Allegiance.Qun, 3, new JieYuan(), new FenXin()))));
        }
    }
}
