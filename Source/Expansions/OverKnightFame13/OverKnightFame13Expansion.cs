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
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("ZhuRan", false, Allegiance.Wu, 3, new DanShou()))));
            CardSet.Add(new Card(SuitType.None, -1, new HeroCardHandler(new Hero("YuFan", false, Allegiance.Wu, 3, new ZongXuan(), new ZhiYan()))));
        }
    }
}
