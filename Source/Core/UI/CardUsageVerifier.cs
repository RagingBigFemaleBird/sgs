using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;

namespace Sanguosha.Core.UI
{
    public struct UiHelper
    {
        public bool isPlayerRepeatable;
        public bool isActionStage;
        public bool hasNoConfirmation;
        public bool isGuHuo;
    }

    public interface ICardUsageVerifier
    {
        VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players);
        IList<CardHandler> AcceptableCardType { get; }
        VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> players);
        UiHelper Helper { get; }
    }

    public abstract class CardUsageVerifier : ICardUsageVerifier
    {
        public virtual VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> players)
        {
            CardTransformSkill transformSkill = skill as CardTransformSkill;

            if (skill is PassiveSkill)
            {
                return VerifierResult.Fail;
            }

            if (AcceptableCardType == null)
            {
                return SlowVerify(source, skill, cards, players);
            }

            if (transformSkill != null)
            {
                if (transformSkill.PossibleResults == null)
                {
                    return SlowVerify(source, skill, cards, players);
                }
                else
                {
                    var commonResult = from type1 in AcceptableCardType join type2 in transformSkill.PossibleResults
                                       on type1 equals type2 select type1;
                    if (commonResult.Count() == 0)
                    {
                        return SlowVerify(source, skill, cards, players);
                    }
                }
                return VerifierResult.Fail;
            }

            if (skill is ActiveSkill)
            {
                if (SlowVerify(source, skill, null, null) == VerifierResult.Fail)
                {
                    return VerifierResult.Fail;
                }
            }

            return SlowVerify(source, skill, cards, players);
        }

        private VerifierResult SlowVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
        {
            VerifierResult initialResult = FastVerify(source, skill, cards, players);

            if (skill != null)
            {
                if (initialResult == VerifierResult.Success)
                {
                    return VerifierResult.Success;
                }
                bool NothingWorks = true;
                List<Card> tryList = new List<Card>();
                if (cards != null)
                {
                    tryList.AddRange(cards);
                }
                var cardsToTry = new List<Card>(Game.CurrentGame.Decks[source, DeckType.Hand].Concat(Game.CurrentGame.Decks[source, DeckType.Equipment]));
                foreach (Card c in cardsToTry)
                {
                    tryList.Add(c);
                    if (FastVerify(source, skill, tryList, players) != VerifierResult.Fail)
                    {
                        NothingWorks = false;
                        break;
                    }
                    tryList.Remove(c);
                }
                List<Player> tryList2 = new List<Player>();
                if (players != null)
                {
                    tryList2.AddRange(players);
                }
                foreach (Player p in Game.CurrentGame.Players)
                {
                    tryList2.Add(p);
                    if (FastVerify(source, skill, cards, tryList2) != VerifierResult.Fail)
                    {
                        NothingWorks = false;
                        break;
                    }
                    tryList2.Remove(p);
                }
                if (NothingWorks)
                {
                    return VerifierResult.Fail;
                }
            }
            return initialResult;
        }

        public abstract VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players);

        public abstract IList<CardHandler> AcceptableCardType { get; }


        public virtual UiHelper Helper
        {
            get { return new UiHelper(); }
        }

    }
}
