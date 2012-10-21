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

        public void TryAskForCardUsage(string prompt, ICardUsageVerifier verifier)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
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
        public bool TryAnswerForCardUsage(string prompt, ICardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players)
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

        public bool AskForCardUsage(string prompt, ICardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players)
        {
            Trace.TraceInformation("Asking Card Usage to {0}.", HostPlayer.Id);
            if (active)
            {
                TryAskForCardUsage(prompt, verifier);
                NextQuestion();
            }
            else
            {
                Trace.TraceInformation("Not active player, defaulting.");
            }
            if (TryAnswerForCardUsage(prompt, verifier, out skill, out cards, out players))
            {
                Trace.Assert(verifier.FastVerify(skill, cards, players) == VerifierResult.Success);
                return true;
            }
            return false;
        }

        public bool AskForCardChoice(string prompt, List<DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier, out List<List<Card>> answer)
        {
            answer = null;
            return false;
        }

        public bool AskForMultipleChoice(string prompt, List<string> questions, out int answer)
        {
            Trace.TraceInformation("Asking Multiple choice to {0}.", HostPlayer.Id);
            if (active)
            {
                TryAskForMultipleChoice(prompt, questions);
                NextQuestion();
            }
            else
            {
                Trace.TraceInformation("Not active player, defaulting.");
            }
            if (TryAnswerForMultipleChoice(prompt, questions, out answer))
            {
                return true;
            }
            return false;
        }

        private bool TryAnswerForMultipleChoice(string prompt, List<string> questions, out int answer)
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

        private void TryAskForMultipleChoice(string prompt, List<string> questions)
        {
            int answer;
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

        public void NotifyCardMovement(List<CardsMovement> m, List<IGameLog> notes)
        {
            proxy.NotifyCardMovement(m, notes);
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
