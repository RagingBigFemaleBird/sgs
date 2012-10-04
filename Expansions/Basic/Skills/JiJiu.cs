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
    public class JiJiu : CardTransformSkill
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
                if (Game.CurrentGame.CurrentPlayer == Owner)
                {
                    return VerifierResult.Fail;
                }
                else if (cards[0].SuitColor == SuitColorType.Red)
                {
                    card = new CompositeCard();
                    card.Subcards = new List<Card>(cards);
                    card.Type = new Tao();
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
