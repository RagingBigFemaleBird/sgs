using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Network;
using System.Diagnostics;

namespace Sanguosha.Core.UI
{
    public class ClientNetworkUiProxy : IUiProxy
    {

        public void Freeze()
        {
            proxy.Freeze();
        }

        public Player HostPlayer
        {
            get;
            set;
        }

        IUiProxy proxy;
        Client client;
        bool active;
        public ClientNetworkUiProxy(IUiProxy p, Client c, bool a)
        {
            proxy = p;
            client = c;
            active = a;
        }

        public void TryAskForCardUsage(Prompt prompt, ICardUsageVerifier verifier)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (!active)
            {
                proxy.AskForCardUsage(prompt, verifier, out skill, out cards, out players);
                return;
            }
            if (!proxy.AskForCardUsage(prompt, verifier, out skill, out cards, out players))
            {
                Trace.TraceInformation("Invalid answer");
                client.AnswerNext();
                client.AnswerItem(0);
            }
            else
            {
                client.AnswerNext();
                client.AnswerItem(1);
                if (skill == null)
                {
                    client.AnswerItem(0);
                }
                else
                {
                    client.AnswerItem(1);
                    client.AnswerItem(skill);
                }
                if (cards == null)
                {
                    client.AnswerItem(0);
                }
                else
                {
                    client.AnswerItem(cards.Count);
                    foreach (Card c in cards)
                    {
                        client.AnswerItem(c);
                    }
                }
                if (players == null)
                {
                    client.AnswerItem(0);
                }
                else
                {
                    client.AnswerItem(players.Count);
                    foreach (Player p in players)
                    {
                        client.AnswerItem(p);
                    }
                }
            }
        }
        public bool TryAnswerForCardUsage(Prompt prompt, ICardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players)
        {
            skill = null;
            cards = null;
            players = null;
            object o = client.Receive();
            if (o == null)
            {
                return false;
            }
            if ((int)o == 0)
            {
                return false;
            }
            cards = new List<Card>();
            o = client.Receive();
            int count = (int)o;
            if (count == 1)
            {
                skill = (ISkill)client.Receive();
            }
            o = client.Receive();
            count = (int)o;
            while (count-- > 0)
            {
                o = client.Receive();
                cards.Add((Card)o);
            }
            players = new List<Player>();
            o = client.Receive();
            count = (int)o;
            while (count-- > 0)
            {
                o = client.Receive();
                players.Add((Player)o);
            }
            return true;
        }

        public void NextQuestion()
        {
            client.NextComm();
        }

        public bool AskForCardUsage(Prompt prompt, ICardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players)
        {
            Trace.TraceInformation("Asking Card Usage to {0}.", HostPlayer.Id);
            TryAskForCardUsage(prompt, verifier);
            if (active)
            {
                NextQuestion();
            }
            else
            {
                Trace.TraceInformation("Not active player, defaulting.");
            }
            if (TryAnswerForCardUsage(prompt, verifier, out skill, out cards, out players))
            {
                proxy.Freeze();
                Trace.Assert(verifier.FastVerify(HostPlayer, skill, cards, players) == VerifierResult.Success);
                return true;
            }
            proxy.Freeze();
            return false;
        }

        public bool AskForCardChoice(Prompt prompt, List<DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier, out List<List<Card>> answer, List<bool> rearrangeable, CardChoiceRearrangeCallback callback)
        {
            Trace.TraceInformation("Asking Card Choice to {0}.", HostPlayer.Id);
            TryAskForCardChoice(prompt, sourceDecks, resultDeckNames, resultDeckMaximums, verifier, rearrangeable, callback);
            if (active)
            {
                NextQuestion();
            }
            else
            {
                Trace.TraceInformation("Not active player, defaulting.");
            }
            if (TryAnswerForCardChoice(prompt, verifier, out answer, callback))
            {
                proxy.Freeze();
                Trace.Assert(verifier.Verify(answer) == VerifierResult.Success);
                return true;
            }
            proxy.Freeze();
            return false;
        }

        public bool AskForMultipleChoice(Prompt prompt, List<string> questions, out int answer)
        {
            Trace.TraceInformation("Asking Multiple choice to {0}.", HostPlayer.Id);
            TryAskForMultipleChoice(prompt, questions);
            if (active)
            {
                NextQuestion();
            }
            else
            {
                Trace.TraceInformation("Not active player, defaulting.");
            }
            if (TryAnswerForMultipleChoice(questions, out answer))
            {
                Game.CurrentGame.NotificationProxy.NotifyMultipleChoiceResult(HostPlayer, questions[answer]);
                proxy.Freeze();
                return true;
            }
            Game.CurrentGame.NotificationProxy.NotifyMultipleChoiceResult(HostPlayer, questions[answer]);
            proxy.Freeze();
            return false;
        }

        private bool TryAnswerForMultipleChoice(List<string> questions, out int answer)
        {
            answer = 0;
            object o = client.Receive();
            if (o == null)
            {
                return false;
            }
            answer = (int)o;
            return true;
        }

        private void TryAskForMultipleChoice(Prompt prompt, List<string> questions)
        {
            int answer;
            if (!active)
            {
                proxy.AskForMultipleChoice(prompt, questions, out answer);
                return;
            }
            if (!proxy.AskForMultipleChoice(prompt, questions, out answer))
            {
                Trace.TraceInformation("Invalid answer");
                client.AnswerNext();
                client.AnswerItem(0);
            }
            else
            {
                client.AnswerNext();
                client.AnswerItem(answer);
            }
        }

        public void TryAskForCardChoice(Prompt prompt, List<DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier, List<bool> rearrangeable, CardChoiceRearrangeCallback callback)
        {
            List<List<Card>> answer;
            if (!active)
            {
                proxy.AskForCardChoice(prompt, sourceDecks, resultDeckNames, resultDeckMaximums, verifier, out answer, rearrangeable, callback);
                return;
            }
            if (!proxy.AskForCardChoice(prompt, sourceDecks, resultDeckNames, resultDeckMaximums, verifier, out answer, rearrangeable, callback) ||
                answer == null)
            {
                Trace.TraceInformation("Invalid answer");
                client.AnswerNext();
                client.AnswerItem(0);
            }
            else
            {
                client.AnswerNext();
                client.AnswerItem(1);
                client.AnswerItem(answer.Count);
                foreach (var subList in answer)
                {
                    client.AnswerItem(subList.Count);
                    foreach (Card c in subList)
                    {
                        client.AnswerItem(c);
                    }
                }
            }
        }

        public bool TryAnswerForCardChoice(Prompt prompt, ICardChoiceVerifier verifier, out List<List<Card>> answer, CardChoiceRearrangeCallback callback)
        {
            answer = null;
            object o = client.Receive();
            if (o == null)
            {
                return false;
            }
            if ((int)o == 0)
            {
                return false;
            }
            answer = new List<List<Card>>();
            o = client.Receive();
            int count = (int)o;
            while (count-- > 0)
            {
                o = client.Receive();
                if (o == null)
                {
                    return false;
                }
                int subCount = (int)o;
                var theList = new List<Card>();
                answer.Add(theList);
                while (subCount-- > 0)
                {
                    o = client.Receive();
                    if (o == null)
                    {
                        return false;
                    }
                    theList.Add((Card)o);
                }
            }
            return true;
        }

        int timeOutSeconds;
        public int TimeOutSeconds
        {
            get
            {
                return TimeOutSeconds;
            }
            set
            {
                proxy.TimeOutSeconds = value;
                timeOutSeconds = value;
            }
        }
    }
}
