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
using Sanguosha.Expansions.OverKnightFame13.Skills;
using Sanguosha.Expansions.Basic.Skills;

namespace OverKnightFame12
{
    public class OverKnightFame13Expansion : Expansion
    {
        public OverKnightFame13Expansion()
        {
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhuRan", true, Allegiance.Wu, 3, new DanShou()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("YuFan", true, Allegiance.Wu, 3, new ZongXuan(), new ZhiYan()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("GuanPing", true, Allegiance.Shu, 4, new LongYin()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("LiRu", true, Allegiance.Qun, 3, new JueCe(), new MieJi(), new FenCheng()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("LiuFeng", true, Allegiance.Shu, 4, new XianSi()))));
        }
    }
}
