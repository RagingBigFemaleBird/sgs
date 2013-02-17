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
    public class AsyncUiAdapter : IUiProxy
    {
        public void Freeze()
        {
            proxy.Freeze();
        }
        
        private Semaphore answerPending;
        private ISkill answerSkill;
        private List<Card> answerCards;
        private List<Player> answerPlayers;
        private List<List<Card>> answerCardsOfCards;
        private int answerMultipleChoice;

        private IAsyncUiProxy proxy;
        public AsyncUiAdapter(IAsyncUiProxy asyncProxy)
        {
            proxy = asyncProxy;
            proxy.CardUsageAnsweredEvent += proxy_CardUsageAnsweredEvent;
            proxy.CardChoiceAnsweredEvent += proxy_CardChoiceAnsweredEvent;
            proxy.MultipleChoiceAnsweredEvent += proxy_MultipleChoiceAnsweredEvent;
        }

        void proxy_MultipleChoiceAnsweredEvent(int answer)
        {
            answerMultipleChoice = answer;
            answerPending.Release(1);
        }

        void proxy_CardChoiceAnsweredEvent(List<List<Card>> cards)
        {
            answerCardsOfCards = cards;
            answerPending.Release(1);
        }

        void proxy_CardUsageAnsweredEvent(ISkill skill, List<Card> cards, List<Player> players)
        {
            answerSkill = skill;
            answerCards = cards;
            answerPlayers = players;
            answerPending.Release(1);
        }

        public Player HostPlayer
        {
            get
            {
                return proxy.HostPlayer;
            }
            set
            {
                proxy.HostPlayer = value;
            }
        }

        public bool AskForCardUsage(Prompt prompt, ICardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players)
        {
            answerPending = new Semaphore(0, 1);
            int timeOut = TimeOutSeconds + (verifier.Helper != null ? verifier.Helper.ExtraTimeOutSeconds : 0);
            proxy.AskForCardUsage(prompt, verifier, timeOut);
            skill = null;
            cards = null;
            players = null;
            if (answerPending.WaitOne(timeOut * 1000))
            {
                skill = answerSkill;
                cards = answerCards;
                players = answerPlayers;
            }
            cards = cards ?? new List<Card>();
            players = players ?? new List<Player>();
            return (verifier.FastVerify(HostPlayer, answerSkill, cards, players) == VerifierResult.Success);
        }

        public bool AskForCardChoice(Prompt prompt, List<DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier, out List<List<Card>> answer, AdditionalCardChoiceOptions options, CardChoiceRearrangeCallback callback)
        {
            answerPending = new Semaphore(0, 1);
            int timeOut = TimeOutSeconds + (verifier.Helper != null ? verifier.Helper.ExtraTimeOutSeconds : 0);
            proxy.AskForCardChoice(prompt, sourceDecks, resultDeckNames, resultDeckMaximums, verifier, timeOut, options, callback);
            answer = null;
            if (answerPending.WaitOne(timeOut * 1000))
            {
                answer = answerCardsOfCards;                
            }
            if (answer == null)
            {
                return false;
            }
            else
            {
                return (verifier.Verify(answer) == VerifierResult.Success);
            }
            
        }

        public bool AskForMultipleChoice(Prompt prompt, List<OptionPrompt> questions, out int answer)
        {
            answerPending = new Semaphore(0, 1);
            proxy.AskForMultipleChoice(prompt, questions, TimeOutSeconds);
            if (answerPending.WaitOne(TimeOutSeconds * 1000))
            {
                answer = answerMultipleChoice;
            }
            else
            {
                answer = 0;
            }
            return true;
        }

        public int TimeOutSeconds { get; set; }
    }
}
