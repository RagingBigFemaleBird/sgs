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
    public class QingGuo : CardTransformSkill
    {
        public override VerifierResult Transform(List<Card> cards, object arg, out CompositeCard card)
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
                    card.Type = new Shan();
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
