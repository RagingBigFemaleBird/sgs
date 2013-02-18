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
using Sanguosha.Expansions.OverKnightFame11.Skills;

namespace Sanguosha.Expansions.OverKnightFame11
{
    public class OverKnightFame11Expansion : Expansion
    {
        public OverKnightFame11Expansion()
        {
            CardSet = new List<Card>();

            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("CaoZhi", true, Allegiance.Wei, 3, new LuoYing(), new JiuShi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ChenGong", true, Allegiance.Qun, 3, new MingCe(), new ZhiChi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("FaZheng", true, Allegiance.Shu, 3, new EnYuan(), new XuanHuo()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("GaoShun", true, Allegiance.Qun, 4, new XianZhen(), new JinJiu()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("LingTong", true, Allegiance.Wu, 4, new XuanFeng()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("MaSu", true, Allegiance.Shu, 3, new XinZhan(), new HuiLei()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("WuGuotai", false, Allegiance.Wu, 3, new GanLu(), new BuYi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("XuSheng", true, Allegiance.Wu, 4, new PoJun()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("XuShu", true, Allegiance.Shu, 3, new WuYan(), new JuJian()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("YuJin", true, Allegiance.Wei, 4, new YiZhong()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhangChunhua", false, Allegiance.Wei, 3, new JueQing(), new ShangShi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhongHui", true, Allegiance.Wei, 4, new QuanJi(), new ZiLi()))));
        }
    }
}
