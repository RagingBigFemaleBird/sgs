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
    /// 奇袭-出牌阶段，你可以将一张黑色牌当【过河拆桥】使用。
    /// </summary>
    public class QiXi : OneToOneCardTransformSkill
    {
        public override bool VerifyInput(Card card, object arg)
        {
            return card.SuitColor == SuitColorType.Black;
        }

        public override CardHandler PossibleResult
        {
            get { return new GuoHeChaiQiao(); }
        }
    }
}
