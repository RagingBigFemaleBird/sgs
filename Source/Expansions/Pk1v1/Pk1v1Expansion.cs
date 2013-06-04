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
using Sanguosha.Expansions.Basic.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Expansions.Wind.Skills;
using Sanguosha.Expansions.Pk1v1.Skills;
using Sanguosha.Expansions.Hills.Skills;
using Sanguosha.Expansions.Woods.Skills;
using Sanguosha.Expansions.Pk1v1.Cards;

namespace Sanguosha.Expansions
{
    public class Pk1v1Expansion : Expansion
    {
        public Pk1v1Expansion()
        {
            CardSet.Add(new Card(SuitType.Spade, 1, new JueDou()));
            CardSet.Add(new Card(SuitType.Heart, 1, new WanJianQiFa()));
            CardSet.Add(new Card(SuitType.Club, 1, new JueDou()));
            CardSet.Add(new Card(SuitType.Diamond, 1, new ZhuGeLianNu()));

            CardSet.Add(new Card(SuitType.Spade, 2, new BaGuaZhen()));
            CardSet.Add(new Card(SuitType.Heart, 2, new Shan()));
            CardSet.Add(new Card(SuitType.Club, 2, new RenWangDun()));
            CardSet.Add(new Card(SuitType.Diamond, 2, new Shan()));

            CardSet.Add(new Card(SuitType.Spade, 3, new GuoHeChaiQiao2()));
            CardSet.Add(new Card(SuitType.Heart, 3, new Tao()));
            CardSet.Add(new Card(SuitType.Club, 3, new GuoHeChaiQiao2()));
            CardSet.Add(new Card(SuitType.Diamond, 3, new Shan()));

            CardSet.Add(new Card(SuitType.Spade, 4, new ShunShouQianYang()));
            CardSet.Add(new Card(SuitType.Heart, 4, new Tao()));
            CardSet.Add(new Card(SuitType.Club, 4, new RegularSha()));
            CardSet.Add(new Card(SuitType.Diamond, 4, new ShunShouQianYang()));

            CardSet.Add(new Card(SuitType.Spade, 5, new RegularSha()));
            CardSet.Add(new Card(SuitType.Heart, 5, new Shan()));
            CardSet.Add(new Card(SuitType.Club, 5, new RegularSha()));
            CardSet.Add(new Card(SuitType.Diamond, 5, new GuanShiFu()));


            CardSet.Add(new Card(SuitType.Spade, 6, new QingGangJian()));
            CardSet.Add(new Card(SuitType.Heart, 6, new LeBuSiShu()));
            CardSet.Add(new Card(SuitType.Club, 6, new RegularSha()));
            CardSet.Add(new Card(SuitType.Diamond, 6, new RegularSha()));

            CardSet.Add(new Card(SuitType.Spade, 7, new RegularSha()));
            CardSet.Add(new Card(SuitType.Heart, 7, new WuZhongShengYou()));
            CardSet.Add(new Card(SuitType.Club, 7, new ShuiYanQiJun()));
            CardSet.Add(new Card(SuitType.Diamond, 7, new Shan()));

            CardSet.Add(new Card(SuitType.Spade, 8, new RegularSha()));
            CardSet.Add(new Card(SuitType.Heart, 8, new WuZhongShengYou()));
            CardSet.Add(new Card(SuitType.Club, 8, new RegularSha()));
            CardSet.Add(new Card(SuitType.Diamond, 8, new Shan()));

            CardSet.Add(new Card(SuitType.Spade, 9, new HanBingJian()));
            CardSet.Add(new Card(SuitType.Heart, 9, new Tao()));
            CardSet.Add(new Card(SuitType.Club, 9, new RegularSha()));
            CardSet.Add(new Card(SuitType.Diamond, 9, new RegularSha()));

            CardSet.Add(new Card(SuitType.Spade, 10, new RegularSha()));
            CardSet.Add(new Card(SuitType.Heart, 10, new RegularSha()));
            CardSet.Add(new Card(SuitType.Club, 10, new RegularSha()));
            CardSet.Add(new Card(SuitType.Diamond, 10, new Shan()));

            CardSet.Add(new Card(SuitType.Spade, 11, new ShunShouQianYang()));
            CardSet.Add(new Card(SuitType.Heart, 11, new RegularSha()));
            CardSet.Add(new Card(SuitType.Club, 11, new RegularSha()));
            CardSet.Add(new Card(SuitType.Diamond, 11, new Shan()));

            CardSet.Add(new Card(SuitType.Spade, 12, new ZhangBaSheMao()));
            CardSet.Add(new Card(SuitType.Heart, 12, new GuoHeChaiQiao2()));
            CardSet.Add(new Card(SuitType.Club, 12, new BingLiangCunDuan()));
            CardSet.Add(new Card(SuitType.Diamond, 12, new Tao()));

            CardSet.Add(new Card(SuitType.Spade, 13, new NanManRuQin()));
            CardSet.Add(new Card(SuitType.Heart, 13, new WuXieKeJi()));
            CardSet.Add(new Card(SuitType.Club, 13, new WuXieKeJi()));
            CardSet.Add(new Card(SuitType.Diamond, 13, new RegularSha()));

            for (int i = 0; i < 5; i++)
            {
                CardSet.Add(new Card(SuitType.None, 0, new RoleCardHandler(Role.Defector)));
                CardSet.Add(new Card(SuitType.None, 0, new RoleCardHandler(Role.Ruler)));
            }

            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("CaoCao", true, Allegiance.Wei, 4, new JianXiong()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("SimaYi", true, Allegiance.Wei, 3, new FanKui(), new GuiCai()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("XiahouDun", true, Allegiance.Wei, 4, new GangLie()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhangLiao", true, Allegiance.Wei, 4, new TuXi2()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("XuChu", true, Allegiance.Wei, 4, new LuoYi(), new XieChan()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("GuoJia", true, Allegiance.Wei, 3, new TianDu(), new YiJi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhenJi", false, Allegiance.Wei, 3, new QingGuo2(), new LuoShen()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("GuanYu", true, Allegiance.Shu, 4, new WuSheng(), new HuWei()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhangFei", true, Allegiance.Shu, 4, new PaoXiao()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhugeLiang", true, Allegiance.Shu, 3, new GuanXing(), new KongCheng()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhaoYun", true, Allegiance.Shu, 4, new LongDan()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("MaChao", true, Allegiance.Shu, 4, new XiaoXi(), new TieJi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("HuangYueying", false, Allegiance.Shu, 3, new JiZhi(), new QiCai()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("SunQuan", true, Allegiance.Wu, 4, new ZhiHeng2()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("GanNing", true, Allegiance.Wu, 4, new QiXi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("HuangGai", true, Allegiance.Wu, 4, new KuRou()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhouYu", true, Allegiance.Wu, 3, new YingZi(), new FanJian()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("LuXun", true, Allegiance.Wu, 3, new QianXun(), new LianYing()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("SunShangxiang", false, Allegiance.Wu, 3, new YinLi(), new XiaoJi2()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("LvBu", true, Allegiance.Qun, 4, new WuShuang()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("DiaoChan", false, Allegiance.Qun, 3, new BiYue(), new PianYi()))));

            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("XiahouYuan", true, Allegiance.Wei, 4, new ShenSu(), new SuZi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("CaoRen", true, Allegiance.Wei, 4, new JuShou()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("HuangZhong", true, Allegiance.Shu, 4, new LieGong()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("XiaoQiao", false, Allegiance.Wu, 3, new TianXiang(), new HongYan()))));

            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("JiangWei", true, Allegiance.Shu, 4, new TiaoXin()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("MengHuo", true, Allegiance.Shu, 4, new ManYi(), new ZaiQi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhuRong", false, Allegiance.Shu, 4, new ManYi(), new LieRen()))));

            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("HeJin", true, Allegiance.Qun, 4, new YanHuo(), new MouZhu()))));

            TriggerRegistration = new List<DelayedTriggerRegistration>();
            TriggerRegistration.Add(new DelayedTriggerRegistration() { key = GameEvent.CardUsageBeforeEffected, trigger = new WuXieKeJiTrigger() });
            TriggerRegistration.Add(new DelayedTriggerRegistration() { key = GameEvent.CardUsageBeforeEffected, trigger = new ShaCancelling() });
            TriggerRegistration.Add(new DelayedTriggerRegistration() { key = GameEvent.PlayerDying, trigger = new PlayerDying() { Priority = int.MinValue } });
            TriggerRegistration.Add(new DelayedTriggerRegistration() { key = GameEvent.DamageElementConfirmed, trigger = new JiuDamage() });
            TriggerRegistration.Add(new DelayedTriggerRegistration() { key = GameEvent.PlayerUsedCard, trigger = new JiuSha() });
        }
    }
}
