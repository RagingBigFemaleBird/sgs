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
using Sanguosha.Expansions.StarSP.Skills;
using Sanguosha.Expansions.Basic.Skills;

namespace Sanguosha.Expansions.StarSP
{
    public class StarSpExpansion : Expansion
    {
        public StarSpExpansion()
        {
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("SPZhaoYun", true, Allegiance.Qun, 3, new LongDan(), new ChongZhen()) { HeroConvertFrom = "ZhaoYun"})));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("SPDiaoChan", false, Allegiance.Qun, 3, new LiHun(), new BiYue()) { HeroConvertFrom = "DiaoChan" })));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("SPCaoRen", true, Allegiance.Wei, 4, new KuiWei(), new YanZheng()) { HeroConvertFrom = "CaoRen" })));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("SPPangTong", true, Allegiance.Qun, 3, new ManJuan(), new ZuiXiang()) { HeroConvertFrom = "PangTong" })));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("SPZhangFei", true, Allegiance.Shu, 4, new JiE(), new DaHe()) { HeroConvertFrom = "ZhangFei" })));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("SPLvMeng", true, Allegiance.Wu, 3, new TanHu(), new MouDuan()) { HeroConvertFrom = "LvMeng" })));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("SPLiuBei", true, Allegiance.Shu, 4, new ZhaoLie(), new ShiChou()) { HeroConvertFrom = "LiuBei" })));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("SPDaQiao", false, Allegiance.Wu, 3, new YanXiao(), new AnXian()) { HeroConvertFrom = "DaQiao" })));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("SPGanNing", true, Allegiance.Qun, 4, new YinLing(), new JunWei()) { HeroConvertFrom = "GanNing" })));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("SPXiahouDun", true, Allegiance.Wei, 4, new FenYong(), new XueHen()) { HeroConvertFrom = "XiahouDun" })));

            TriggerRegistration = new List<DelayedTriggerRegistration>();
            TriggerRegistration.Add(new DelayedTriggerRegistration() { key = GameEvent.PhaseBeginEvents[TurnPhase.Judge], trigger = new YanXiaoTrigger() });
        }
    }
}