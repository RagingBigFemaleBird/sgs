using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;

namespace Sanguosha.Core.Skills.Fire
{
    /// <summary>
    /// 火鸡
    /// </summary>
    class HuoJi : CardTransformationSkill
    {
        public override VerifierResult Transform(List<Card> cards, object arg, out CompositeCard card)
        {
            VerifierResult r = RequireCards(cards, 1);
            card = null;
            if (r == VerifierResult.Success)
            {
                if (cards[0].SuitColor == SuitColorType.Red)
                {
                    card = new CompositeCard();
                    card.Subcards = new List<Card>(cards);
                    card.Type = new Cards.Battle.HuoGong().CardType;
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
