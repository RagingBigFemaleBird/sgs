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
    /// 国色-出牌阶段，你可以将一张方块牌当【乐不思蜀】使用。
    /// </summary>
    public class GuoSe : CardTransformSkill
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
                if (cards[0].Suit == SuitType.Diamond)
                {
                    card = new CompositeCard();
                    card.Subcards = new List<Card>(cards);
                    card.Type = new LeBuSiShu();
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
