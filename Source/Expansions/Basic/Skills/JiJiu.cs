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
            card = new CompositeCard();
            card.Type = new Tao();
            if (Owner == Game.CurrentGame.PhasesOwner)
            {
                return VerifierResult.Fail;
            }
            if (cards == null || cards.Count < 1)
            {
                return VerifierResult.Partial;
            }
            if (cards.Count > 1)
            {
                return VerifierResult.Fail;
            }
            if (cards[0].SuitColor != SuitColorType.Red || cards[0].Owner != Owner)
            {
                return VerifierResult.Fail;
            }
            card.Subcards = new List<Card>(cards);
            return VerifierResult.Success;
        }

        public override List<CardHandler> PossibleResults
        {
            get { return new List<CardHandler>() { new Tao() }; }
        }
    }
}
