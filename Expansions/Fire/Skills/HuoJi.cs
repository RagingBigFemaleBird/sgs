using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Battle.Cards;

namespace Sanguosha.Expansions.Fire.Skills
{
    /// <summary>
    /// 火计–出牌阶段，你可以将你的任意一张红色手牌当【火攻】使用。
    /// </summary>
    public class HuoJi : OneToOneCardTransformSkill
    {

        public override CardHandler PossibleResult
        {
            get { return new HuoGong(); }
        }

        public override bool VerifyInput(Card card, object arg)
        {
            return card.SuitColor == SuitColorType.Red;
        }
    }
}
