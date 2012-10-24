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
    }

    public interface ICardUsageVerifier
    {
        VerifierResult FastVerify(ISkill skill, List<Card> cards, List<Player> players);
        IList<CardHandler> AcceptableCardType { get; }
        VerifierResult Verify(ISkill skill, List<Card> cards, List<Player> players);
        UiHelper Helper { get; }
    }

    public abstract class CardUsageVerifier : ICardUsageVerifier
    {
        public virtual VerifierResult Verify(ISkill skill, List<Card> cards, List<Player> players)
        {
            CardTransformSkill transformSkill = skill as CardTransformSkill;

            if (AcceptableCardType == null)
            {
                return SlowVerify(skill, cards, players);
            }

            if (transformSkill != null)
            {
                if (transformSkill.PossibleResult == null)
                {
                    return SlowVerify(skill, cards, players);
                }
                else
                {
                    foreach (var type in AcceptableCardType)
                    {
                        if (type.CardType == transformSkill.PossibleResult.CardType)
                        {
                            return SlowVerify(skill, cards, players);
                        }
                    }
                }
                return VerifierResult.Fail;
            }

            if (skill is ActiveSkill)
            {
                if (SlowVerify(skill, null, null) == VerifierResult.Fail)
                {
                    return VerifierResult.Fail;
                }
            }

            return SlowVerify(skill, cards, players);
        }

        private VerifierResult SlowVerify(ISkill skill, List<Card> cards, List<Player> players)
        {
            VerifierResult initialResult = FastVerify(skill, cards, players);

            if (Game.CurrentGame.CurrentActingPlayer != null && skill != null)
            {
                if (initialResult == VerifierResult.Success)
                {
                    return VerifierResult.Success;
                }
                bool NothingWorks = true;
                Player player = Game.CurrentGame.CurrentActingPlayer;
                List<Card> tryList = new List<Card>();
                if (cards != null)
                {
                    tryList.AddRange(cards);
                }
                foreach (Card c in (Game.CurrentGame.Decks[player, DeckType.Hand].Concat(Game.CurrentGame.Decks[player, DeckType.Equipment])))
                {
                    tryList.Add(c);
                    if (FastVerify(skill, tryList, players) != VerifierResult.Fail)
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
                    if (FastVerify(skill, cards, tryList2) != VerifierResult.Fail)
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

        public abstract VerifierResult FastVerify(ISkill skill, List<Card> cards, List<Player> players);

        public abstract IList<CardHandler> AcceptableCardType { get; }


        public virtual UiHelper Helper
        {
            get { return new UiHelper(); }
        }
    }
}
