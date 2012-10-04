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
    public class SingleCardUsageVerifier : ICardUsageVerifier
    {
        public delegate bool CardMatcher(ICard card);
        private CardMatcher match;

        public CardMatcher Match
        {
            get { return match; }
            set { match = value; }
        }

        public SingleCardUsageVerifier(CardMatcher m)
        {
            Match = m;
        }

        public VerifierResult Verify(ISkill skill, List<Card> cards, List<Player> players)
        {
            if (cards == null || cards.Count != 1 || (players != null && players.Count != 0))
            {
                return VerifierResult.Fail;
            }
            if (skill != null)
            {
                CompositeCard card;
                CardTransformSkill s = (CardTransformSkill)skill;
                VerifierResult r = s.TryTransform(cards, null, out card);
                if (!Match(card))
                {
                    return VerifierResult.Fail;
                }
                return VerifierResult.Success;
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
    }
}
