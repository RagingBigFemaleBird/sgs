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
using Sanguosha.Expansions.Fire.Skills;
using Sanguosha.Expansions.Basic.Skills;

namespace Sanguosha.Expansions.Fire
{
    public class FireExpansion : Expansion
    {
        public FireExpansion()
        {
            CardSet = new List<Card>();

            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("DianWei", true, Allegiance.Wei, 4, new QiangXi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("XunYu", true, Allegiance.Wei, 3, new QuHu(), new JieMing()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("PangTong", true, Allegiance.Shu, 3, new LianHuan(), new NiePan()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("TaishiCi", true, Allegiance.Wu, 4, new TianYi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("WoLong", true, Allegiance.Shu, 3, new BaZhen(), new HuoJi(), new KanPo()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("YuanShao", true, Allegiance.Qun, 4, new LuanJi(), new XueYi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("YanLiangWenChou", true, Allegiance.Qun, 4, new ShuangXiong()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("PangDe", true, Allegiance.Qun, 4, new MaShu(), new MengJin()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ShenZhouYu", true, Allegiance.God, 4, new QinYin(), new YeYan()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ShenZhugeLiang", true, Allegiance.God, 3, new QiXing(), new KuangFeng(), new DaWu()))));
        }
    }
}
