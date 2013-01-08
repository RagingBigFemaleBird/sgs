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
//using Sanguosha.Expansions.Fire.Skills;
//using Sanguosha.Expansions.Woods.Skills;
//using Sanguosha.Expansions.Hills.Skills;
using Sanguosha.Expansions.Basic.Skills;

namespace Sanguosha.Expansions.SP
{
    public class SpExpansion : Expansion
    {
        public SpExpansion()
        {
            CardSet = new List<Card>();

            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("YangXiu", true, Allegiance.Wei, 3, new DanLao(), new JiLei()))));
            //CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("QunDiaoChan", false, Allegiance.Qun, 3, new LiJian(), new BiYue()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("GongsunZan", true, Allegiance.Qun, 4, new YiCong()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("YuanShu", true, Allegiance.Qun, 4, new YongSi(), new WeiDi()))));
            //CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ShuSunShangxiang", false, Allegiance.Shu, 3, new JieYin(), new XiaoJi()))));
            //CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("WeiPangDe", true, Allegiance.Wei, 4, new MaShu(), new MengJin()))));
            //CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("WeiGuanYu", true, Allegiance.Wei, 4, new WuSheng(), new DanQi()))));
            //CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("HuLaoLvBu", true, Allegiance.God, 8, new MaShu(), new WuShuang()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("HuLaoLvBu2", true, Allegiance.God, 4, new MaShu(), new WuShuang(), new XiuLuo(), new ShenWei(), new ShenJi()))));
            //CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("WeiCaiWenji", false, Allegiance.Wei, 3, new BeiGe(), new DuanChang()))));
            //CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("QunMaChao", true, Allegiance.Qun, 4, new MaShu(), new TieJi()))));
            //CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("WeiJiaXu", true, Allegiance.Wei, 3, new WanSha(), new LuanWu(), new WeiMu()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("CaoHong", true, Allegiance.Wei, 4, new YuanHu()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("GuanYinping", false, Allegiance.Shu, 3, new XueJi(), new HuXiao(), new WuJi()))));
        }
    }
}
