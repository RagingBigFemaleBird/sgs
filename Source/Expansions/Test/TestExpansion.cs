using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Players;
using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Expansions.Fire.Skills;
using Sanguosha.Expansions.Basic.Skills;
using Sanguosha.Expansions.OverKnightFame11.Skills;
using Sanguosha.Expansions.SP.Skills;
using Sanguosha.Core.Heroes;


namespace Sanguosha.Expansions.Test
{
    public class TestExpansion : Expansion
    {
        public TestExpansion()
        {
            CardSet = new List<Card>();
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("Test", true, Allegiance.Shu, 4, new RenDe(), new PaoXiao(), new JiJiang(), new WuSheng(), new FanJian(), new QingNang(), new JiJiu(), new LiJian(), new KongCheng(), new FanKui(), new ZhiHeng(), new JiZhi(), new KuRou()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("Test2", true, Allegiance.Shu, 4, new LongDan(), new PaoXiao(), new FanKui(), new LiJian(), new QianXun(), new QingGuo(), new GangLie(), new GuiCai(), new LianYing(), new RenDe()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("Test3", true, Allegiance.Shu, 3, new RenDe(), new HuoJi(), new KanPo(), new JiLei(), new LuoYing(), new LongDan()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("Test4", true, Allegiance.Shu, 3, new RenDe(), new HuoJi(), new KanPo(), new JiLei(), new LongDan()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("Test5", true, Allegiance.Shu, 3, new RenDe(), new HuoJi(), new KanPo(), new JiLei(), new LongDan()))));
        }
    }
}
