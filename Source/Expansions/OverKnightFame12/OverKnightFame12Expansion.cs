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
using Sanguosha.Expansions.OverKnightFame12.Skills;
using Sanguosha.Expansions.Basic.Skills;

namespace OverKnightFame12
{
    public class OverKnightFame12Expansion : Expansion
    {
        public OverKnightFame12Expansion()
        {
            CardSet = new List<Card>();

            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("XunYou", true, Allegiance.Wei, 3, new ZhiYu(), new QiCe()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("LiaoHua", true, Allegiance.Shu, 4, new DangXian(), new FuLi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhongHui", true, Allegiance.Wei, 4, new QuanJi(), new ZiLi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("HanDang", true, Allegiance.Wu, 4, new GongQi(), new JieFan()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("BuLianShi", false, Allegiance.Wu, 3, new AnXu(), new ZhuiYi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("CaoZhang", true, Allegiance.Wei, 4, new JiangChi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("GuanXingZhangBao", true, Allegiance.Shu, 4, new FuHun()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("MaDai", true, Allegiance.Shu, 4, new MaShu(), new QianXi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("HuaXiong", true, Allegiance.Qun, 6, new ShiYong()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("LiuBiao", true, Allegiance.Qun, 4, new ZiShou(), new ZongShi()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ChengPu", true, Allegiance.Wu, 4, new LiHuo(), new ChunLao()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("WangYi", false, Allegiance.Wei, 3, new ZhenLie(), new MiJi()))));

            TriggerRegistration = new List<DelayedTriggerRegistration>();
            TriggerRegistration.Add(new DelayedTriggerRegistration() { key = GameEvent.AfterDamageCaused, trigger = new LiHuoShaCausedDamage() });
            TriggerRegistration.Add(new DelayedTriggerRegistration() { key = GameEvent.CardUsageDone, trigger = new LiHuoLoseHealth() });
        }
    }
}
