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

            CardSet.Add(new Card(SuitType.Spade, 2, new BaGuaZhen()));
            CardSet.Add(new Card(SuitType.Spade, 2, new CiXiongShuangGuJian()));
            CardSet.Add(new Card(SuitType.Heart, 2, new Shan()));
            CardSet.Add(new Card(SuitType.Heart, 2, new Shan()));
            CardSet.Add(new Card(SuitType.Club, 2, new RegularSha()));
            CardSet.Add(new Card(SuitType.Club, 2, new BaGuaZhen()));
            CardSet.Add(new Card(SuitType.Diamond, 2, new Shan()));
            CardSet.Add(new Card(SuitType.Diamond, 2, new Shan()));

            CardSet.Add(new Card(SuitType.Spade, 3, new GuoHeChaiQiao()));
            CardSet.Add(new Card(SuitType.Spade, 3, new ShunShouQianYang()));
            CardSet.Add(new Card(SuitType.Heart, 3, new Tao()));
            CardSet.Add(new Card(SuitType.Heart, 3, new WuGuFengDeng()));
            CardSet.Add(new Card(SuitType.Club, 3, new RegularSha()));
            CardSet.Add(new Card(SuitType.Club, 3, new GuoHeChaiQiao()));
            CardSet.Add(new Card(SuitType.Diamond, 3, new Shan()));
            CardSet.Add(new Card(SuitType.Diamond, 3, new ShunShouQianYang()));

            CardSet.Add(new Card(SuitType.Spade, 4, new GuoHeChaiQiao()));
            CardSet.Add(new Card(SuitType.Spade, 4, new ShunShouQianYang()));
            CardSet.Add(new Card(SuitType.Heart, 4, new Tao()));
            CardSet.Add(new Card(SuitType.Heart, 4, new WuGuFengDeng()));
            CardSet.Add(new Card(SuitType.Club, 4, new RegularSha()));
            CardSet.Add(new Card(SuitType.Club, 4, new GuoHeChaiQiao()));
            CardSet.Add(new Card(SuitType.Diamond, 4, new Shan()));
            CardSet.Add(new Card(SuitType.Diamond, 4, new ShunShouQianYang()));

            CardSet.Add(new Card(SuitType.Spade, 5, new QingLongYanYueDao()));
            CardSet.Add(new Card(SuitType.Spade, 5, new DefensiveHorse("JueYing")));
            CardSet.Add(new Card(SuitType.Heart, 5, new QiLinGong()));
            CardSet.Add(new Card(SuitType.Heart, 5, new OffensiveHorse("ChiTu")));
            CardSet.Add(new Card(SuitType.Club, 5, new RegularSha()));
            CardSet.Add(new Card(SuitType.Club, 5, new DefensiveHorse("DiLu")));
            CardSet.Add(new Card(SuitType.Diamond, 5, new Shan()));
            CardSet.Add(new Card(SuitType.Diamond, 5, new GuanShiFu()));


            CardSet.Add(new Card(SuitType.Spade, 6, new LeBuSiShu()));
            CardSet.Add(new Card(SuitType.Spade, 6, new QingGangJian()));
            CardSet.Add(new Card(SuitType.Heart, 6, new Tao()));
            CardSet.Add(new Card(SuitType.Heart, 6, new LeBuSiShu()));
            CardSet.Add(new Card(SuitType.Club, 6, new RegularSha()));
            CardSet.Add(new Card(SuitType.Club, 6, new LeBuSiShu()));
            CardSet.Add(new Card(SuitType.Diamond, 6, new RegularSha()));
            CardSet.Add(new Card(SuitType.Diamond, 6, new Shan()));

            CardSet.Add(new Card(SuitType.Spade, 7, new RegularSha()));
            CardSet.Add(new Card(SuitType.Spade, 7, new NanManRuQin()));
            CardSet.Add(new Card(SuitType.Heart, 7, new Tao()));
            CardSet.Add(new Card(SuitType.Heart, 7, new WuZhongShengYou()));
            CardSet.Add(new Card(SuitType.Club, 7, new RegularSha()));
            CardSet.Add(new Card(SuitType.Club, 7, new NanManRuQin()));
            CardSet.Add(new Card(SuitType.Diamond, 7, new RegularSha()));
            CardSet.Add(new Card(SuitType.Diamond, 7, new Shan()));

            CardSet.Add(new Card(SuitType.Spade, 8, new RegularSha()));
            CardSet.Add(new Card(SuitType.Spade, 8, new RegularSha()));
            CardSet.Add(new Card(SuitType.Heart, 8, new Tao()));
            CardSet.Add(new Card(SuitType.Heart, 8, new WuZhongShengYou()));
            CardSet.Add(new Card(SuitType.Club, 8, new RegularSha()));
            CardSet.Add(new Card(SuitType.Club, 8, new RegularSha()));
            CardSet.Add(new Card(SuitType.Diamond, 8, new RegularSha()));
            CardSet.Add(new Card(SuitType.Diamond, 8, new Shan()));

            CardSet.Add(new Card(SuitType.Spade, 9, new RegularSha()));
            CardSet.Add(new Card(SuitType.Spade, 9, new RegularSha()));
            CardSet.Add(new Card(SuitType.Heart, 9, new Tao()));
            CardSet.Add(new Card(SuitType.Heart, 9, new WuZhongShengYou()));
            CardSet.Add(new Card(SuitType.Club, 9, new RegularSha()));
            CardSet.Add(new Card(SuitType.Club, 9, new RegularSha()));
            CardSet.Add(new Card(SuitType.Diamond, 9, new RegularSha()));
            CardSet.Add(new Card(SuitType.Diamond, 9, new Shan()));

            CardSet.Add(new Card(SuitType.Spade, 10, new RegularSha()));
            CardSet.Add(new Card(SuitType.Spade, 10, new RegularSha()));
            CardSet.Add(new Card(SuitType.Heart, 10, new RegularSha()));
            CardSet.Add(new Card(SuitType.Heart, 10, new RegularSha()));
            CardSet.Add(new Card(SuitType.Club, 10, new RegularSha()));
            CardSet.Add(new Card(SuitType.Club, 10, new RegularSha()));
            CardSet.Add(new Card(SuitType.Diamond, 10, new RegularSha()));
            CardSet.Add(new Card(SuitType.Diamond, 10, new Shan()));

            CardSet.Add(new Card(SuitType.Spade, 11, new ShunShouQianYang()));
            CardSet.Add(new Card(SuitType.Spade, 11, new WuXieKeJi()));
            CardSet.Add(new Card(SuitType.Heart, 11, new RegularSha()));
            CardSet.Add(new Card(SuitType.Heart, 11, new WuZhongShengYou()));
            CardSet.Add(new Card(SuitType.Club, 11, new RegularSha()));
            CardSet.Add(new Card(SuitType.Club, 11, new RegularSha()));
            CardSet.Add(new Card(SuitType.Diamond, 11, new Shan()));
            CardSet.Add(new Card(SuitType.Diamond, 11, new Shan()));

            CardSet.Add(new Card(SuitType.Spade, 12, new GuoHeChaiQiao()));
            CardSet.Add(new Card(SuitType.Spade, 12, new ZhangBaSheMao()));
            CardSet.Add(new Card(SuitType.Heart, 12, new Tao()));
            CardSet.Add(new Card(SuitType.Heart, 12, new GuoHeChaiQiao()));
            CardSet.Add(new Card(SuitType.Club, 12, new JieDaoShaRen()));
            CardSet.Add(new Card(SuitType.Club, 12, new WuXieKeJi()));
            CardSet.Add(new Card(SuitType.Diamond, 12, new Tao()));
            CardSet.Add(new Card(SuitType.Diamond, 12, new FangTianHuaJi()));

            CardSet.Add(new Card(SuitType.Spade, 13, new NanManRuQin()));
            CardSet.Add(new Card(SuitType.Spade, 13, new OffensiveHorse("DaYuan")));
            CardSet.Add(new Card(SuitType.Heart, 13, new Shan()));
            CardSet.Add(new Card(SuitType.Heart, 13, new DefensiveHorse("ZhuaHuangFeiDian")));
            CardSet.Add(new Card(SuitType.Club, 13, new JieDaoShaRen()));
            CardSet.Add(new Card(SuitType.Club, 13, new WuXieKeJi()));
            CardSet.Add(new Card(SuitType.Diamond, 13, new RegularSha()));
            CardSet.Add(new Card(SuitType.Diamond, 13, new OffensiveHorse("ZiXing")));

            // the following are EX cards
            CardSet.Add(new Card(SuitType.Spade, 2, new HanBingJian()));
            CardSet.Add(new Card(SuitType.Club, 2, new RenWangDun()));
            CardSet.Add(new Card(SuitType.Heart, 12, new ShanDian()));
            CardSet.Add(new Card(SuitType.Diamond, 12, new WuXieKeJi()));

            for (int i = 0; i < 5; i++)
            {
                CardSet.Add(new Card(SuitType.None, 0, new RoleCardHandler(Role.Rebel)));
                CardSet.Add(new Card(SuitType.None, 0, new RoleCardHandler(Role.Loyalist)));
                CardSet.Add(new Card(SuitType.None, 0, new RoleCardHandler(Role.Defector)));
                CardSet.Add(new Card(SuitType.None, 0, new RoleCardHandler(Role.Ruler)));
            }

            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("CaoCao", true, Allegiance.Wei, 4, new JianXiong(), new HuJia()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("SimaYi", true, Allegiance.Wei, 3, new FanKui(), new GuiCai()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("XiahouDun", true, Allegiance.Wei, 4, new GangLie()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhangLiao", true, Allegiance.Wei, 4, new TuXi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("XuChu", true, Allegiance.Wei, 4, new LuoYi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("GuoJia", true, Allegiance.Wei, 3, new TianDu(), new YiJi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhenJi", false, Allegiance.Wei, 3, new QingGuo(), new LuoShen()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("LiuBei", true, Allegiance.Shu, 4, new RenDe(), new JiJiang()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("GuanYu", true, Allegiance.Shu, 4, new WuSheng()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhangFei", true, Allegiance.Shu, 4, new PaoXiao()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhugeLiang", true, Allegiance.Shu, 3, new GuanXing(), new KongCheng()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhaoYun", true, Allegiance.Shu, 4, new LongDan()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("MaChao", true, Allegiance.Shu, 4, new MaShu(), new TieJi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("HuangYueying", false, Allegiance.Shu, 3, new JiZhi(), new QiCai()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("SunQuan", true, Allegiance.Wu, 4, new ZhiHeng(), new JiuYuan()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("GanNing", true, Allegiance.Wu, 4, new QiXi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("LvMeng", true, Allegiance.Wu, 4, new KeJi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("HuangGai", true, Allegiance.Wu, 4, new KuRou()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhouYu", true, Allegiance.Wu, 3, new YingZi(), new FanJian()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("DaQiao", false, Allegiance.Wu, 3, new GuoSe(), new LiuLi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("LuXun", true, Allegiance.Wu, 3, new QianXun(), new LianYing()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("SunShangxiang", false, Allegiance.Wu, 3, new JieYin(), new XiaoJi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("HuaTuo", true, Allegiance.Qun, 3, new JiJiu(), new QingNang()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("LvBu", true, Allegiance.Qun, 4, new WuShuang()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("DiaoChan", false, Allegiance.Qun, 3, new LiJian(), new BiYue()))));

            TriggerRegistration = new List<DelayedTriggerRegistration>();
            TriggerRegistration.Add(new DelayedTriggerRegistration() { key = GameEvent.PlayerUsedCard, trigger = new WuGuFengDengTrigger() });
            TriggerRegistration.Add(new DelayedTriggerRegistration() { key = GameEvent.CardUsageBeforeEffected, trigger = new WuXieKeJiTrigger() });
            TriggerRegistration.Add(new DelayedTriggerRegistration() { key = GameEvent.CardUsageBeforeEffected, trigger = new ShaCancelling() });
            TriggerRegistration.Add(new DelayedTriggerRegistration() { key = GameEvent.PlayerDying, trigger = new PlayerDying() { Priority = int.MinValue } });
        }
    }
}
