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
using Sanguosha.Expansions.Assassin.Skills;

namespace Sanguosha.Expansions.Assassin
{
    public class AssassinExpansion : Expansion
    {
        public AssassinExpansion()
        {
            CardSet = new List<Card>();

            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("FuWan", true, Allegiance.Qun, 4, new MouKui()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("LiuXie", true, Allegiance.Qun, 3, new TianMing(), new MiZhao()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("LingJu", false, Allegiance.Qun, 3, new JieYuan(), new FenXin()))));
        }
    }
}
