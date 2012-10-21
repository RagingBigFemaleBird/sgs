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

namespace Sanguosha.Expansions.Fire.Skills
{
    /// <summary>
    /// 火计–你可以将你的任意一张黑色手牌当【无懈可击】使用。
    /// </summary>
    public class KanPo : OneToOneCardTransformSkill
    {
        public override CardHandler PossibleResult
        {
            get { return new WuXieKeJi(); }
        }

        public override bool VerifyInput(Card card, object arg)
        {
            return card.SuitColor == SuitColorType.Black;
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
