using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Expansions.Battle.Cards;

namespace Sanguosha.Expansions.Woods.Skills
{
    /// <summary>
    /// 酒池-你可以将一张黑桃手牌当【酒】使用。
    /// </summary>
    public class JiuChi : OneToOneCardTransformSkill
    {
        public override bool VerifyInput(Card card, object arg)
        {
            return card.Suit == SuitType.Spade;
        }

        public override CardHandler PossibleResult
        {
            get { return new Jiu(); }
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
