using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Core.Cards
{
    public class SingleCardUsageVerifier : CardUsageVerifier
    {
        public delegate bool CardMatcher(ICard card);
        private CardMatcher match;

        public CardMatcher Match
        {
            get { return match; }
            set { match = value; }
        }

        IList<CardHandler> possibleMatch;

        public SingleCardUsageVerifier(CardMatcher m, CardHandler handler = null)
        {
            Match = m;
            if (handler != null)
            {
                possibleMatch = new List<CardHandler>();
                possibleMatch.Add(handler);
            }
            else
            {
                possibleMatch = null;
            }
        }

        public override VerifierResult FastVerify(ISkill skill, List<Card> cards, List<Player> players)
        {
            if (players != null && players.Count != 0)
            {
                return VerifierResult.Fail;
            }
            if (skill != null)
            {
                CompositeCard card;
                if (!(skill is CardTransformSkill))
                {
                    return VerifierResult.Fail;
                }
                CardTransformSkill s = (CardTransformSkill)skill;
                VerifierResult r = s.TryTransform(cards, null, out card);
                if (r != VerifierResult.Success)
                {
                    return r;
                }
                if (!Match(card))
                {
                    return VerifierResult.Fail;
                }
                return VerifierResult.Success;
            }
            if (cards != null && cards.Count > 1)
            {
                return VerifierResult.Fail;
            }
            if (cards == null || cards.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (cards[0].Place.DeckType != DeckType.Hand)
            {
                return VerifierResult.Fail;
            }
            if (!Match(cards[0]))
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }


        public override IList<CardHandler> AcceptableCardType
        {
            get { return possibleMatch; }
        }
    }
}
