using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Core.Skills
{
    public abstract class OneToOneCardTransformSkill : CardTransformSkill
    {
        public override VerifierResult TryTransform(List<Card> cards, object arg, out CompositeCard card)
        {
            card = null;
            if (cards == null || cards.Count < 1)
            {
                return VerifierResult.Partial;
            }
            if (cards.Count > 1)
            {
                return VerifierResult.Fail;
            }
            if (cards[0].Place.DeckType != DeckType.None && cards[0].Owner != Owner && !(Helper.OtherDecksUsed.Count != 0 && Helper.OtherDecksUsed.Contains(cards[0].Place.DeckType)) )
            {
                return VerifierResult.Fail;
            }
            if (HandCardOnly)
            {
                if (cards[0].Place.DeckType != DeckType.Hand)
                {
                    return VerifierResult.Fail;
                }
            }
            if (VerifyInput(cards[0], arg))
            {
                card = new CompositeCard();
                card.Subcards = new List<Card>(cards);
                card.Type = PossibleResult;
                return VerifierResult.Success;
            }
            return VerifierResult.Fail;
        }

        public abstract bool VerifyInput(Card card, object arg);

        public bool HandCardOnly
        {
            get;
            protected set;
        }

        /// <summary>
        /// 卡牌转换技能可以转换成的卡牌类型。
        /// </summary>
        public abstract CardHandler PossibleResult { get; }

        public override List<CardHandler> PossibleResults
        {
            get { return new List<CardHandler>() { PossibleResult }; }
        }
    }
}
