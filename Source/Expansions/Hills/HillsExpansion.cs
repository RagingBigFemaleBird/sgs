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
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhangZhaoZhangHong", true, Allegiance.Wu, 3, new ZhiJian(), new GuZheng()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZuoCi", true, Allegiance.Qun, 3, new HuaShen(), new XinSheng()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("CaiWenJi", true, Allegiance.Qun, 3, new BeiGe()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("LiuChan", true, Allegiance.Shu, 3, new XiangLe(), new FangQuan()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("JiangWei", true, Allegiance.Shu, 4, new TiaoXin(), new ZhiJi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ShenZhaoYun", true, Allegiance.God, 2, new JueJing(), new LongHun()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("DengAi", true, Allegiance.Wei, 4, new TunTian(), new ZaoXian()))));
        }
    }
}
