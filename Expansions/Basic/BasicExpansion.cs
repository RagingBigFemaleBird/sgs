using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Expansions.Basic.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Heroes;

namespace Sanguosha.Expansions.Basic
{
    public class BasicExpansion : Expansion
    {
        public BasicExpansion()
        {
            CardSet = new List<Card>();
            CardSet.Add(new Card(SuitType.Spade, 1, new JueDou()));
            CardSet.Add(new Card(SuitType.Spade, 1, new ShanDian()));
            CardSet.Add(new Card(SuitType.Heart, 1, new WanJianQiFa()));
            CardSet.Add(new Card(SuitType.Heart, 1, new TaoYuanJieYi()));
            CardSet.Add(new Card(SuitType.Club, 1, new JueDou()));
            CardSet.Add(new Card(SuitType.Club, 1, new ZhuGeLianNu()));
            CardSet.Add(new Card(SuitType.Diamond, 1, new JueDou()));
            CardSet.Add(new Card(SuitType.Diamond, 1, new ZhuGeLianNu()));

//            CardSet.Add(new Card(SuitType.Spade, 2, BAGUAZHEN()));
//            CardSet.Add(new Card(SuitType.Spade, 2, CIXIONGSHUANGGUJIAN()));
            CardSet.Add(new Card(SuitType.Heart, 2, new Shan()));
            CardSet.Add(new Card(SuitType.Heart, 2, new Shan()));
            CardSet.Add(new Card(SuitType.Club, 2, new Sha()));
            CardSet.Add(new Card(SuitType.Diamond, 2, new Shan()));
            CardSet.Add(new Card(SuitType.Diamond, 2, new Shan()));

            CardSet.Add(new Card(SuitType.Spade, 3, new GuoHeChaiQiao()));
            CardSet.Add(new Card(SuitType.Spade, 3, new ShunShouQianYang()));
            CardSet.Add(new Card(SuitType.Heart, 3, new Tao()));
//            CardSet.Add(new Card(SuitType.Heart, 3, WUGUFENGDENG()));
            CardSet.Add(new Card(SuitType.Club, 3, new Sha()));
            CardSet.Add(new Card(SuitType.Club, 3, new GuoHeChaiQiao()));
            CardSet.Add(new Card(SuitType.Diamond, 3, new Shan()));
            CardSet.Add(new Card(SuitType.Diamond, 3, new ShunShouQianYang()));

            CardSet.Add(new Card(SuitType.Spade, 4, new GuoHeChaiQiao()));
            CardSet.Add(new Card(SuitType.Spade, 4, new ShunShouQianYang()));
            CardSet.Add(new Card(SuitType.Heart, 4, new Tao()));
//            CardSet.Add(new Card(SuitType.Heart, 4, WUGUFENGDENG()));
            CardSet.Add(new Card(SuitType.Club, 4, new Sha()));
            CardSet.Add(new Card(SuitType.Club, 4, new GuoHeChaiQiao()));
            CardSet.Add(new Card(SuitType.Diamond, 4, new Shan()));
            CardSet.Add(new Card(SuitType.Diamond, 4, new ShunShouQianYang()));

//            CardSet.Add(new Card(SuitType.Spade, 5, QINGLONGYANYUEDAO()));
//            CardSet.Add(new Card(SuitType.Spade, 5, JIAYIMA()));
//            CardSet.Add(new Card(SuitType.Heart, 5, QILINGONG()));
//            CardSet.Add(new Card(SuitType.Heart, 5, JIANYIMA()));
            CardSet.Add(new Card(SuitType.Club, 5, new Sha()));
//            CardSet.Add(new Card(SuitType.Club, 5, JIAYIMA()));
            CardSet.Add(new Card(SuitType.Diamond, 5, new Shan()));
//            CardSet.Add(new Card(SuitType.Diamond, 5, GUANSHIFU()));


            CardSet.Add(new Card(SuitType.Spade, 6, new LeBuSiShu()));
//            CardSet.Add(new Card(SuitType.Spade, 6, QINGGANGJIAN()));
            CardSet.Add(new Card(SuitType.Heart, 6, new Tao()));
            CardSet.Add(new Card(SuitType.Heart, 6, new LeBuSiShu()));
            CardSet.Add(new Card(SuitType.Club, 6, new Sha()));
            CardSet.Add(new Card(SuitType.Club, 6, new LeBuSiShu()));
            CardSet.Add(new Card(SuitType.Diamond, 6, new Sha()));
            CardSet.Add(new Card(SuitType.Diamond, 6, new Shan()));

            CardSet.Add(new Card(SuitType.Spade, 7, new Sha()));
            CardSet.Add(new Card(SuitType.Spade, 7, new NanManRuQin()));
            CardSet.Add(new Card(SuitType.Heart, 7, new Tao()));
            CardSet.Add(new Card(SuitType.Heart, 7, new WuZhongShengYou()));
            CardSet.Add(new Card(SuitType.Club, 7, new Sha()));
            CardSet.Add(new Card(SuitType.Club, 7, new NanManRuQin()));
            CardSet.Add(new Card(SuitType.Diamond, 7, new Sha()));
            CardSet.Add(new Card(SuitType.Diamond, 7, new Shan()));

            CardSet.Add(new Card(SuitType.Spade, 8, new Sha()));
            CardSet.Add(new Card(SuitType.Spade, 8, new Sha()));
            CardSet.Add(new Card(SuitType.Heart, 8, new Tao()));
            CardSet.Add(new Card(SuitType.Heart, 8, new WuZhongShengYou()));
            CardSet.Add(new Card(SuitType.Club, 8, new Sha()));
            CardSet.Add(new Card(SuitType.Club, 8, new Sha()));
            CardSet.Add(new Card(SuitType.Diamond, 8, new Sha()));
            CardSet.Add(new Card(SuitType.Diamond, 8, new Shan()));

            CardSet.Add(new Card(SuitType.Spade, 9, new Sha()));
            CardSet.Add(new Card(SuitType.Spade, 9, new Sha()));
            CardSet.Add(new Card(SuitType.Heart, 9, new Tao()));
            CardSet.Add(new Card(SuitType.Heart, 9, new WuZhongShengYou()));
            CardSet.Add(new Card(SuitType.Club, 9, new Sha()));
            CardSet.Add(new Card(SuitType.Club, 9, new Sha()));
            CardSet.Add(new Card(SuitType.Diamond, 9, new Sha()));
            CardSet.Add(new Card(SuitType.Diamond, 9, new Shan()));

            CardSet.Add(new Card(SuitType.Spade, 10, new Sha()));
            CardSet.Add(new Card(SuitType.Spade, 10, new Sha()));
            CardSet.Add(new Card(SuitType.Heart, 10, new Sha()));
            CardSet.Add(new Card(SuitType.Heart, 10, new Sha()));
            CardSet.Add(new Card(SuitType.Club, 10, new Sha()));
            CardSet.Add(new Card(SuitType.Club, 10, new Sha()));
            CardSet.Add(new Card(SuitType.Diamond, 10, new Sha()));
            CardSet.Add(new Card(SuitType.Diamond, 10, new Shan()));

            CardSet.Add(new Card(SuitType.Spade, 11, new ShunShouQianYang()));
//            CardSet.Add(new Card(SuitType.Spade, 11, WUXIEKEJI()));
            CardSet.Add(new Card(SuitType.Heart, 11, new Sha()));
            CardSet.Add(new Card(SuitType.Heart, 11, new WuZhongShengYou()));
            CardSet.Add(new Card(SuitType.Club, 11, new Sha()));
            CardSet.Add(new Card(SuitType.Club, 11, new Sha()));
            CardSet.Add(new Card(SuitType.Diamond, 11, new Shan()));
            CardSet.Add(new Card(SuitType.Diamond, 11, new Shan()));

            CardSet.Add(new Card(SuitType.Spade, 12, new GuoHeChaiQiao()));
//            CardSet.Add(new Card(SuitType.Spade, 12, ZHANGBASHEMAO()));
            CardSet.Add(new Card(SuitType.Heart, 12, new Tao()));
            CardSet.Add(new Card(SuitType.Heart, 12, new GuoHeChaiQiao()));
//            CardSet.Add(new Card(SuitType.Club, 12, JIEDAOSHAREN()));
//            CardSet.Add(new Card(SuitType.Club, 12, WUXIEKEJI()));
            CardSet.Add(new Card(SuitType.Diamond, 12, new Tao()));
//            CardSet.Add(new Card(SuitType.Diamond, 12, FANGTIANHUAJI()));

            CardSet.Add(new Card(SuitType.Spade, 13, new NanManRuQin()));
//            CardSet.Add(new Card(SuitType.Spade, 13, JIANYIMA()));
            CardSet.Add(new Card(SuitType.Heart, 13, new Shan()));
//            CardSet.Add(new Card(SuitType.Heart, 13, JIAYIMA()));
//            CardSet.Add(new Card(SuitType.Club, 13, JIEDAOSHAREN()));
//            CardSet.Add(new Card(SuitType.Club, 13, WUXIEKEJI()));
            CardSet.Add(new Card(SuitType.Diamond, 13, new Sha()));
//            CardSet.Add(new Card(SuitType.Diamond, 13, JIANYIMA()));

            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("LiuBei", Allegiance.Shu, 4, new RenDe(), new JiJiang()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhangFei", Allegiance.Shu, 4, new PaoXiao()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhaoYun", Allegiance.Shu, 4, new LongDan()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("GuanYu", Allegiance.Shu, 4, new WuSheng()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("DaQiao", Allegiance.Wu, 3, new GuoSe()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("GanNing", Allegiance.Wu, 4, new QiXi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhenJi", Allegiance.Wei, 3, new QingGuo()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("HuaTuo", Allegiance.Qun, 3, new QingNang(), new JiJiu()))));

        }
    }
}
