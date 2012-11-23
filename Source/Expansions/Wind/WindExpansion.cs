using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Wind.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Heroes;

namespace Sanguosha.Expansions.Wind
{
    public class WindExpansion : Expansion
    {
        public WindExpansion()
        {
            CardSet = new List<Card>();

            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("XiahouYuan", true, Allegiance.Wei, 4, new ShenSu()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("CaoRen", true, Allegiance.Wei, 4, new JuShou()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("HuangZhong", true, Allegiance.Shu, 4, new LieGong()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("WeiYan", true, Allegiance.Shu, 4, new KuangGu()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("XiaoQiao", false, Allegiance.Wu, 3, new TianXiang(), new HongYan()))));
            //CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhouTai", true, Allegiance.Wu, 4, new BuQu()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhangJiao", true, Allegiance.Qun, 3, new LeiJi(), new GuiDao()/*, new HuangTian()*/))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("YuJi", true, Allegiance.Qun, 3, new GuHuo()))));

            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ShenGuanYu", true, Allegiance.God, 5, new WuShen(), new WuHun()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ShenLvMeng", true, Allegiance.God, 3, new SheLie(), new GongXin()))));
        }
    }
}
