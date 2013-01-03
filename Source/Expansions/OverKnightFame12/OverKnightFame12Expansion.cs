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
using Sanguosha.Expansions.OverKnightFame12.Skills;

namespace OverKnightFame12
{
    public class OverKnightFame12Expansion : Expansion
    {
        public OverKnightFame12Expansion()
        {
            CardSet = new List<Card>();

            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("XunYou", true, Allegiance.Wei, 3, new ZhiYu(), new QiCe()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("LiaoHua", true, Allegiance.Shu, 4, new DangXian(), new FuLi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhongHui", true, Allegiance.Wei, 4, new QuanJi(), new ZiLi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("HanDang", true, Allegiance.Wu, 4, new GongQi(), new JieFan()))));
        }
    }
}
