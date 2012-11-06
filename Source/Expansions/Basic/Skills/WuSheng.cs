using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.Basic.Skills
{
    /// <summary>
    /// 武圣-你可以将一张红色牌当【杀】使用或打出。
    /// </summary>
    public class WuSheng : OneToOneCardTransformSkill
    {
        public override bool VerifyInput(Card card, object arg)
        {
            return card.SuitColor == SuitColorType.Red;
        }

        public override CardHandler PossibleResult
        {
            get { return new RegularSha(); }
        }
    }
}
