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
    public class WuSheng : CardTransformSkill
    {
        public override VerifierResult TryTransform(List<Card> cards, object arg, out CompositeCard card)
        {
            VerifierResult r = RequireCards(cards, 1);
            card = null;
            if (r == VerifierResult.Success)
            {
                if (cards[0].Owner != Owner)
                {
                    return VerifierResult.Fail;
                }
                if (cards[0].SuitColor == SuitColorType.Red)
                {
                    card = new CompositeCard();
                    card.Subcards = new List<Card>(cards);
                    card.Type = new Sha();
                    return VerifierResult.Success;
                }
                else
                {
                    return VerifierResult.Fail;
                }
            }
            return r;
        }
    }
}
