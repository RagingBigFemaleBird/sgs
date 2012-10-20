using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;

namespace Sanguosha.Expansions.Basic.Skills
{
    /// <summary>
    /// 急救-你的回合外，你可以将一张红色牌当【桃】使用。
    /// </summary>
    public class JiJiu : OneToOneCardTransformSkill
    {
        public override bool VerifyInput(Card card, object arg)
        {
            return card.SuitColor == SuitColorType.Red;
        }

        public override CardHandler PossibleResult
        {
            get { return new Tao(); }
        }
    }
}
