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

        public bool AskForCardUsage(string prompt, CardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players)
        {
            Trace.TraceInformation("Asking Card Usage to {0}.", HostPlayer.Id);
            if (active)
            {
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
            else
            {
                Trace.TraceInformation("Not active player, defaulting.");
            }
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
            Trace.Assert(verifier.FastVerify(skill, cards, players) == VerifierResult.Success);
            return true;
        }

        public bool AskForCardChoice(string prompt, List<DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier, out List<List<Card>> answer)
        {
            answer = null;
            return false;
        }

        public bool AskForMultipleChoice(string prompt, List<string> questions, out int answer)
        {
            answer = 0;
            return true;
        }

        public void NotifyCardMovement(List<CardsMovement> m, List<IGameLog> notes)
        {
            proxy.NotifyCardMovement(m, notes);
        }
    }
}
