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

            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("XuHuang", true, Allegiance.Wei, 4, new DuanLiang()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("CaoPi", true, Allegiance.Wei, 3, new XingShang(), new FangZhu(), new SongWei()))));
            //CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("MengHuo", true, Allegiance.Shu, 4, new HuoShou(), new ZaiQi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhuRong", false, Allegiance.Shu, 4, new JuXiang(), new LieRen()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("SunJian", true, Allegiance.Wu, 4, new YingHun()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("LuSu", true, Allegiance.Wu, 3, new HaoShi(), new DiMeng()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("DongZhuo", true, Allegiance.Qun, 8, new JiuChi(), new RouLin(), new BengHuai(), new BaoNve()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("JiaXu", true, Allegiance.Qun, 3, new WanSha(), new LuanWu(), new WeiMu()))));
            //CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ShenCaoCao", true, Allegiance.God, 3, new GuiXin(), new FeiYing()))));
            //CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ShenLvBu", true, Allegiance.God, 5, new KuangBao(), new WuMou(), new WuQian(), new ShenFen))));
        }
    }
}
