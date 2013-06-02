using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions._1v1.Skills
{
    /// <summary>
    /// 倾国2-你可以将一张装备当【闪】使用或打出。
    /// </summary>
    public class QingGuo2 : OneToOneCardTransformSkill
    {
        public QingGuo2()
        {
        }

        public override bool VerifyInput(Card card, object arg)
        {
            return card.Place.DeckType == DeckType.Equipment;
        }

        public override CardHandler PossibleResult
        {
            get { return new Shan(); }
        }
    }
}
