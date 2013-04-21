using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Heroes;

namespace Sanguosha.Expansions
{
    public class BattleExpansion : Expansion
    {
        public BattleExpansion()
        {
            CardSet.Add(new Card(SuitType.Spade, 1, new GuDingDao()));
            CardSet.Add(new Card(SuitType.Heart, 1, new WuXieKeJi()));
            CardSet.Add(new Card(SuitType.Club, 1, new BaiYinShiZi()));
            CardSet.Add(new Card(SuitType.Diamond, 1, new ZhuQueYuShan()));

            CardSet.Add(new Card(SuitType.Spade, 2, new TengJia()));
            CardSet.Add(new Card(SuitType.Heart, 2, new HuoGong()));
            CardSet.Add(new Card(SuitType.Club, 2, new TengJia()));
            CardSet.Add(new Card(SuitType.Diamond, 2, new Tao()));

            CardSet.Add(new Card(SuitType.Spade, 3, new Jiu()));
            CardSet.Add(new Card(SuitType.Heart, 3, new HuoSha()));
            CardSet.Add(new Card(SuitType.Club, 3, new Jiu()));
            CardSet.Add(new Card(SuitType.Diamond, 3, new Tao()));

            CardSet.Add(new Card(SuitType.Spade, 4, new LeiSha()));
            CardSet.Add(new Card(SuitType.Heart, 4, new HuoGong()));
            CardSet.Add(new Card(SuitType.Club, 4, new BingLiangCunDuan()));
            CardSet.Add(new Card(SuitType.Diamond, 4, new HuoSha()));

            CardSet.Add(new Card(SuitType.Spade, 5, new LeiSha()));
            CardSet.Add(new Card(SuitType.Heart, 5, new Tao()));
            CardSet.Add(new Card(SuitType.Club, 5, new LeiSha()));
            CardSet.Add(new Card(SuitType.Diamond, 5, new HuoSha()));

            CardSet.Add(new Card(SuitType.Spade, 6, new LeiSha()));
            CardSet.Add(new Card(SuitType.Heart, 6, new Tao()));
            CardSet.Add(new Card(SuitType.Club, 6, new LeiSha()));
            CardSet.Add(new Card(SuitType.Diamond, 6, new Shan()));

            CardSet.Add(new Card(SuitType.Spade, 7, new LeiSha()));
            CardSet.Add(new Card(SuitType.Heart, 7, new HuoSha()));
            CardSet.Add(new Card(SuitType.Club, 7, new LeiSha()));
            CardSet.Add(new Card(SuitType.Diamond, 7, new Shan()));

            CardSet.Add(new Card(SuitType.Spade, 8, new LeiSha()));
            CardSet.Add(new Card(SuitType.Heart, 8, new Shan()));
            CardSet.Add(new Card(SuitType.Club, 8, new LeiSha()));
            CardSet.Add(new Card(SuitType.Diamond, 8, new Shan()));

            CardSet.Add(new Card(SuitType.Spade, 9, new Jiu()));
            CardSet.Add(new Card(SuitType.Heart, 9, new Shan()));
            CardSet.Add(new Card(SuitType.Club, 9, new Jiu()));
            CardSet.Add(new Card(SuitType.Diamond, 9, new Jiu()));

            CardSet.Add(new Card(SuitType.Spade, 10, new BingLiangCunDuan()));
            CardSet.Add(new Card(SuitType.Heart, 10, new HuoSha()));
            CardSet.Add(new Card(SuitType.Club, 10, new TieSuoLianHuan()));
            CardSet.Add(new Card(SuitType.Diamond, 10, new Shan()));

            CardSet.Add(new Card(SuitType.Spade, 11, new TieSuoLianHuan()));
            CardSet.Add(new Card(SuitType.Heart, 11, new Shan()));
            CardSet.Add(new Card(SuitType.Club, 11, new TieSuoLianHuan()));
            CardSet.Add(new Card(SuitType.Diamond, 11, new Shan()));

            CardSet.Add(new Card(SuitType.Spade, 12, new TieSuoLianHuan()));
            CardSet.Add(new Card(SuitType.Heart, 12, new Shan()));
            CardSet.Add(new Card(SuitType.Club, 12, new TieSuoLianHuan()));
            CardSet.Add(new Card(SuitType.Diamond, 12, new HuoGong()));

            CardSet.Add(new Card(SuitType.Spade, 13, new WuXieKeJi()));
            CardSet.Add(new Card(SuitType.Heart, 13, new WuXieKeJi()));
            CardSet.Add(new Card(SuitType.Club, 13, new TieSuoLianHuan()));
            CardSet.Add(new Card(SuitType.Diamond, 13, new DefensiveHorse("HuaLiu")));

            TriggerRegistration = new List<DelayedTriggerRegistration>();
            TriggerRegistration.Add(new DelayedTriggerRegistration() { key = GameEvent.DamageElementConfirmed, trigger = new JiuDamage() });
            TriggerRegistration.Add(new DelayedTriggerRegistration() { key = GameEvent.PlayerUsedCard, trigger = new JiuSha() });
        }
    }
}
