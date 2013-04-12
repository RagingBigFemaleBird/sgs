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
using Sanguosha.Expansions.OverKnightFame13.Skills;
using Sanguosha.Expansions.Basic.Skills;

namespace OverKnightFame13
{
    public class OverKnightFame13Expansion : Expansion
    {
        public OverKnightFame13Expansion()
        {
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhuRan", true, Allegiance.Wu, 4, new DanShou()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("YuFan", true, Allegiance.Wu, 3, new ZongXuan(), new ZhiYan()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("GuanPing", true, Allegiance.Shu, 4, new LongYin()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("LiRu", true, Allegiance.Qun, 3, new JueCe(), new MieJi(), new FenCheng()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("LiuFeng", true, Allegiance.Shu, 4, new XianSi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("PanZhangMaZhong", true, Allegiance.Wu, 4, new DuoDao(), new AnJian()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("GuoHuai", true, Allegiance.Wei, 4, new JingCe()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ManChong", true, Allegiance.Wei, 3, new JunXing(), new YuCe()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("CaoChong", true, Allegiance.Wei, 3, new ChengXiang(), new BingXin()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("FuHuangHou", true, Allegiance.Qun, 3, new ZhuiKong(), new QiuYuan()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("JianYong", true, Allegiance.Shu, 3, new QiaoShui(), new ZongShi2()))));
        }
    }
}
