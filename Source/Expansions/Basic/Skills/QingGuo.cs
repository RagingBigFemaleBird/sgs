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
    /// 倾国-你可以将一张黑色手牌当【闪】使用或打出。
    /// </summary>
    public class QingGuo : OneToOneCardTransformSkill
    {
        public override bool VerifyInput(Card card, object arg)
        {
            return card.SuitColor == SuitColorType.Black;
        }

        public override CardHandler PossibleResult
        {
            get { return new Shan(); }
        }

        public override bool HandCardOnly
        {
            get
            {
                return true;
            }
        }

    }
}
