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
using Sanguosha.Expansions.Fire.Skills;

namespace Sanguosha.Expansions.Fire
{
    public class FireExpansion : Expansion
    {
        public FireExpansion()
        {
            CardHandlers = new Dictionary<string,CardHandler>();
            CardHandlers.Add("XiaoZhuGe", new HeroCardHandler(new Hero(Allegiance.Shu, new HuoJi())));
            CardSet = new List<Card>();
        }
    }
}
