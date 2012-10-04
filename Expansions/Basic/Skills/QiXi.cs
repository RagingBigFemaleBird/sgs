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
    public class QiXi : CardTransformSkill
    {
        public override VerifierResult TryTransform(List<Card> cards, object arg, out CompositeCard card)
        {
            VerifierResult r = RequireCards(cards, 1);
            card = null;
            if (r == VerifierResult.Success)
            {
                if (cards[0].Owner != Owner || cards[0].Place.DeckType != DeckType.Hand)
                {
                    return VerifierResult.Fail;
                } 
                if (cards[0].SuitColor == SuitColorType.Black)
                {
                    card = new CompositeCard();
                    card.Subcards = new List<Card>(cards);
                    card.Type = new GuoHeChaiQiao();
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
