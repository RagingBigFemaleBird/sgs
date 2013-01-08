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
using Sanguosha.Expansions.SP.Skills;
using Sanguosha.Expansions.Basic.Skills;

namespace Sanguosha.Expansions.SP
{
    public class SpExpansion : Expansion
    {
        public SpExpansion()
        {
            CardSet = new List<Card>();

            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("YangXiu", true, Allegiance.Wei, 3, new DanLao(), new JiLei()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("GuanYinPing", false, Allegiance.Shu, 3, new WuJi(), new HuXiao(), new XueJi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("YuanShu", true, Allegiance.Qun, 4, new YongSi(), new WeiDi()))));
        }
    }
}
