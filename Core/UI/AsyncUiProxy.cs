using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;

namespace Sanguosha.Core.UI
{
    public abstract class AsyncUiProxy : IUiProxy
    {
        public Player HostPlayer {get; set;}

        Semaphore answerPending;

        public Semaphore AnswerPending
        {
            get { return answerPending; }
            set { answerPending = value; }
        }

        private ISkill answerSkill;
        private List<Card> answerCards;
        private List<Player> answerPlayers;
        private List<List<Card>> answerCardsOfCards;

        public bool AskForCardUsage(string prompt, ICardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players)
        {
            answerPending = new Semaphore(0, 1);
            AskForCardUsageAsync(prompt, verifier);
            answerPending.WaitOne();
            skill = answerSkill;
            cards = answerCards;
            players = answerPlayers;
            if (verifier.Verify(answerSkill, answerCards, answerPlayers) == VerifierResult.Success)
            {
                return true;
            }
            return false;
        }

        public abstract void AskForCardUsageAsync(string prompt, ICardUsageVerifier verifier);
        public void AnswerCardUsageAsync(ISkill skill, List<Card> cards, List<Player> players)
        {
            answerSkill = skill;
            answerCards = cards;
            answerPlayers = players;
            answerPending.Release(1);
        }

        public bool AskForCardChoice(string prompt, List<DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier, out List<List<Card>> answer)
        {
            answerPending = new Semaphore(0, 1);
            AskForCardChoiceAsync(prompt, sourceDecks, resultDeckNames, resultDeckMaximums, verifier);
            answerPending.WaitOne();
            answer = answerCardsOfCards;
            if (verifier.Verify(answer) == VerifierResult.Success)
            {
                return true;
            }
            return false;
        }

        public abstract void AskForCardChoiceAsync(string prompt, List<DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier);
        public void AnswerCardChoiceAsync(List<List<Card>> cards)
        {
            answerCardsOfCards = cards;
            answerPending.Release(1);
        }




        public abstract void NotifyUiLog(CardsMovement m, List<IGameLog> notes);
    }
}
