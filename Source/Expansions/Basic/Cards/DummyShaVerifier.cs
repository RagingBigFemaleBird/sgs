using Sanguosha.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Cards;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;

namespace Sanguosha.Expansions.Basic.Cards
{
    public class DummyShaVerifier : CardUsageVerifier
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
            CompositeCard sha = new CompositeCard() { Type = type };
            if (!Game.CurrentGame.PlayerCanBeTargeted(source, players, sha))
            {
                return VerifierResult.Fail;
            }
            List<Player> newList = new List<Player>(players);
            if (target != null)
            {
                if (!newList.Contains(target))
                {
                    newList.Insert(0, target);
                }
                else
                {
                    return VerifierResult.Fail;
                }
            }
            if (cards != null && cards.Count > 0)
            {
                return VerifierResult.Fail;
            }
            if (skill is CardTransformSkill)
            {
                CardTransformSkill sk = skill as CardTransformSkill;
                if (sk.TryTransform(dummyCards, null, out sha) != VerifierResult.Success)
                {
                    return VerifierResult.Fail;
                }
                if (helper != null) sha[helper] = 1;
                return new Sha().VerifyCore(source, sha, newList);
            }
            else if (skill != null)
            {
                return VerifierResult.Fail;
            }
            if (helper != null) sha[helper] = 1;
            return new Sha().VerifyCore(source, sha, newList);
        }

        public override IList<CardHandler> AcceptableCardTypes
        {
            get { return null; }
        }

        Player target;
        CardHandler type;
        List<Card> dummyCards;
        CardAttribute helper;

        public DummyShaVerifier(Player t, CardHandler shaType, CardAttribute helper = null)
        {
            target = t;
            type = shaType;
            this.helper = helper;
            dummyCards = new List<Card>() { new Card() { Type = shaType, Place = new DeckPlace(null, DeckType.None) } };
        }
    }
}
