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
using Sanguosha.Expansions.Hills.Skills;

namespace Hills
{
    public class HillsExpansion : Expansion
    {
        public HillsExpansion()
        {
            CardSet = new List<Card>();

            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ShenSiMaYi", true, Allegiance.God, 4, new RenJie(), new BaiYin(), new LianPo()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhangHe", true, Allegiance.Wei, 4, new QiaoBian()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("SunCe", true, Allegiance.Wu, 4, new ZhiBa(), new JiAng(), new HunZi()))));
        }
    }
}
