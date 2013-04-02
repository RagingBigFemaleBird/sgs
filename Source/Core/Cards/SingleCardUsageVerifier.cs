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
        bool isUseCard;

        public SingleCardUsageVerifier(CardMatcher m, bool isUseCard, CardHandler handler = null)
        {
            Match = m;
            if (handler != null)
            {
                possibleMatch = new List<CardHandler>() { handler };
            }
            else
            {
                possibleMatch = null;
            }
            this.isUseCard = isUseCard;
        }

        public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
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
                VerifierResult r = s.TryTransform(cards, players, out card, !isUseCard);
                if (r != VerifierResult.Success)
                {
                    return r;
                }
                if (!Match(card))
                {
                    return VerifierResult.Fail;
                }
                if (isUseCard)
                {
                    if (!Game.CurrentGame.PlayerCanUseCard(source, card))
                    {
                        return VerifierResult.Fail;
                    }
                }
                else
                {
                    if (!Game.CurrentGame.PlayerCanPlayCard(source, card))
                    {
                        return VerifierResult.Fail;
                    }
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
            if (isUseCard)
            {
                if (!Game.CurrentGame.PlayerCanUseCard(source, cards[0]))
                {
                    return VerifierResult.Fail;
                }
            }
            else
            {
                if (!Game.CurrentGame.PlayerCanPlayCard(source, cards[0]))
                {
                    return VerifierResult.Fail;
                }
            }
            return VerifierResult.Success;
        }


        public override IList<CardHandler> AcceptableCardTypes
        {
            get { return possibleMatch; }
        }
    }
}
