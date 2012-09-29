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
    class HuoJi : CardTransformSkill
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
                    card.Type = new HuoGong().CardType;
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
