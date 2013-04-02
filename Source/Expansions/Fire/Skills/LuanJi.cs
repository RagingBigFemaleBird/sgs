using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Players;

namespace Sanguosha.Expansions.Fire.Skills
{
    /// <summary>
    /// 乱击-出牌阶段，你可以将两张相同花色的手牌当【万箭齐发】使用。
    /// </summary>
    public class LuanJi : CardTransformSkill
    {
        public override List<CardHandler> PossibleResults
        {
            get { return new List<CardHandler>() { new WanJianQiFa() }; }
        }

        public override VerifierResult TryTransform(List<Card> cards, List<Player> arg, out CompositeCard card)
        {
            card = null;
            if (cards == null || cards.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (cards.Count > 2)
            {
                return VerifierResult.Fail;
            }
            if (cards[0].Owner != Owner || cards[0].Place.DeckType != DeckType.Hand)
            {
                return VerifierResult.Fail;
            }
            if (cards.Count == 2 && ((cards[1].Owner != Owner || cards[1].Place.DeckType != DeckType.Hand) || cards[1].Suit != cards[0].Suit))
            {
                return VerifierResult.Fail;
            }
            if (cards.Count == 1)
            {
                return VerifierResult.Partial;
            }
            card = new CompositeCard();
            card.Subcards = new List<Card>(cards);
            card.Type = new WanJianQiFa();
            return VerifierResult.Success;
        }
    }
}
