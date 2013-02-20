using Sanguosha.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Cards;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.Basic.Cards
{
    class DummyShaVerifier : CardUsageVerifier
    {
        public override VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> players)
        {
            return FastVerify(source, skill, cards, players);
        }

        public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
        {
            if (players != null && players.Any(p => p.IsDead))
            {
                return VerifierResult.Fail;
            }
            if (players == null)
            {
                players = new List<Player>();
            }
            List<Player> newList = new List<Player>(players);
            if (!newList.Contains(target))
            {
                newList.Add(target);
            }
            else
            {
                return VerifierResult.Fail;
            }
            if (cards != null && cards.Count > 0)
            {
                return VerifierResult.Fail;
            }
            CompositeCard sha = new CompositeCard() { Type = type };
            if (skill is CardTransformSkill)
            {
                CardTransformSkill sk = skill as CardTransformSkill;
                if (sk.TryTransform(dummyCards, null, out sha) != VerifierResult.Success)
                {
                    return VerifierResult.Fail;
                }
                return sha.Type.Verify(source, skill, dummyCards, newList);
            }
            else if (skill != null)
            {
                return VerifierResult.Fail;
            }
            return sha.Type.Verify(source, new CardWrapper(source, new RegularSha()), cards, newList);
        }

        public override IList<CardHandler> AcceptableCardTypes
        {
            get { return null; }
        }

        Player target;
        CardHandler type;
        List<Card> dummyCards;

        public DummyShaVerifier(Player t, CardHandler shaType)
        {
            target = t;
            type = shaType;
            dummyCards = new List<Card>() { new Card() { Type = shaType, Place = new DeckPlace(null, DeckType.None) } };
        }
    }
}
