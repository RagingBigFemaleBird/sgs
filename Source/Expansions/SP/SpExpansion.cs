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
using Sanguosha.Expansions.Fire.Skills;
using Sanguosha.Expansions.Woods.Skills;
using Sanguosha.Expansions.Hills.Skills;
using Sanguosha.Expansions.Basic.Skills;
using Sanguosha.Expansions.Assassin.Skills;

namespace Sanguosha.Expansions.SP
{
    public class SpExpansion : Expansion
    {
        public SpExpansion()
        {
            CardSet = new List<Card>();

            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("YangXiu", true, Allegiance.Wei, 3, new DanLao(), new JiLei()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("QunDiaoChan", false, Allegiance.Qun, 3, new LiJian(), new BiYue()) { HeroConvertFrom = "DiaoChan" })));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("GongsunZan", true, Allegiance.Qun, 4, new YiCong()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("YuanShu", true, Allegiance.Qun, 4, new YongSi(), new WeiDi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ShuSunShangxiang", false, Allegiance.Shu, 3, new JieYin(), new XiaoJi()) { HeroConvertFrom = "SunShangxiang" })));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("WeiPangDe", true, Allegiance.Wei, 4, new MaShu(), new MengJin()) { HeroConvertFrom = "PangDe" })));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("WeiGuanYu", true, Allegiance.Wei, 4, new WuSheng(), new DanJi()) { HeroConvertFrom = "GuanYu" })));
            //CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("HuLaoLvBu", true, Allegiance.God, 8, new MaShu(), new WuShuang()))));
            //CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("HuLaoLvBu2", true, Allegiance.God, 4, new MaShu(), new WuShuang(), new XiuLuo(), new ShenWei(), new ShenJi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("WeiCaiWenji", false, Allegiance.Wei, 3, new BeiGe(), new DuanChang()) { HeroConvertFrom = "CaiWenji" })));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("QunMaChao", true, Allegiance.Qun, 4, new MaShu(), new TieJi()) { HeroConvertFrom = "MaChao" })));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("WeiJiaXu", true, Allegiance.Wei, 3, new WanSha(), new LuanWu(), new WeiMu()) { HeroConvertFrom = "JiaXu" })));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("CaoHong", true, Allegiance.Wei, 4, new YuanHu()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("GuanYinping", false, Allegiance.Shu, 3, new XueJi(), new HuXiao(), new WuJi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ChenLin", true, Allegiance.Wei, 3, new BiFa(), new SongCi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("XiahouBa", true, Allegiance.Shu, 4, new BaoBian()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("QunLingJu", false, Allegiance.Qun, 3, new JieYuan(), new FenXin()) { HeroConvertFrom = "LingJu" })));
        }
    }
}
