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
using Sanguosha.Expansions.Woods.Skills;


namespace Sanguosha.Expansions.Woods
{
    public class WoodsExpansion : Expansion
    {
        public WoodsExpansion()
        {
            CardSet = new List<Card>();

            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("JiaXu", true, Allegiance.Qun, 3, new WanSha(), new LuanWu()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("CaoPi", true, Allegiance.Wei, 3, new FangZhu()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ShenSiMaYi", true, Allegiance.God, 4, new RenJie(), new BaiYin(), new LianPo()))));
        }
    }
}
