using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Network;
using System.Diagnostics;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Core.UI
{
    public class ServerNetworkUiProxy : IUiProxy
    {

        public Player HostPlayer
        {
            get;
            set;
        }

        private Server server;
        private int clientId;

        public ServerNetworkUiProxy(Server s, int id)
        {
            server = s;
            clientId = id;
        }

        private void SendNoAnswer()
        {
            int i;
            for (i = 0; i < server.MaxClients; i++)
            {
                server.SendObject(i, 0);
            }
        }

        public bool AskForCardUsage(string prompt, CardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players)
        {
            cards = null;
            skill = null;
            players = null;
            try
            {
                Trace.TraceInformation("Asking Card Usage to {0}.", HostPlayer.Id);
                int? count;
                server.ExpectNext(clientId, 0);
                count = server.GetInt(clientId, 0);
                if (count == null || count == 0)
                {
                    throw new InvalidAnswerException();
                }
                if (count == null)
                {
                    throw new InvalidAnswerException();
                }
                count = server.GetInt(clientId, 0);
                if (count == 1)
                {
                    skill = server.GetSkill(clientId, 0);
                    if (skill == null)
                    {
                        throw new InvalidAnswerException();
                    }
                }
                count = server.GetInt(clientId, 0);
                if (count == null)
                {
                    throw new InvalidAnswerException();
                }
                if (count == 0)
                {
                    cards = null;
                }
                else
                {
                    cards = new List<Card>();
                }
                while (count-- > 0)
                {
                    Card item = server.GetCard(clientId, 0);
                    if (item == null)
                    {
                        throw new InvalidAnswerException();
                    }
                    cards.Add(item);
                }
                count = server.GetInt(clientId, 0);
                if (count == null)
                {
                    throw new InvalidAnswerException();
                }
                if (count == 0)
                {
                    players = null;
                }
                else
                {
                    players = new List<Player>();
                }
                while (count-- > 0)
                {
                    Player item = server.GetPlayer(clientId, 0);
                    if (item == null)
                    {
                        throw new InvalidAnswerException();
                    }
                    players.Add(item);
                }
                if (verifier.FastVerify(skill, cards, players) != VerifierResult.Success)
                {
                    Trace.TraceWarning("Client seems to be sending invalid answers at us. DDOS?");
                    throw new InvalidAnswerException();
                }
                for (int i = 0; i < server.MaxClients; i++)
                {
                    server.SendObject(i, 1);
                    if (skill != null)
                    {
                        server.SendObject(i, 1);
                        server.SendObject(i, skill);
                    }
                    else
                    {
                        server.SendObject(i, 0);
                    }
                    if (cards != null)
                    {
                        server.SendObject(i, cards.Count);
                        if (skill is ActiveSkill)
                        {
                            (skill as ActiveSkill).CardRevealPolicy(Game.CurrentGame.Players[i], cards, players);
                        }
                        foreach (Card c in cards)
                        {
                            if (!(skill is ActiveSkill))
                            {
                                c.RevealOnce = true;
                            }
                            server.SendObject(i, c);
                        }
                    }
                    else
                    {
                        server.SendObject(i, 0);
                    }
                    if (players != null)
                    {
                        server.SendObject(i, players.Count);
                        foreach (Player p in players)
                        {
                            server.SendObject(i, p);
                        }
                    }
                    else
                    {
                        server.SendObject(i, 0);
                    }
                }
                return true;
            }
            catch (InvalidAnswerException)
            {
                SendNoAnswer();
                return false;
            }
        }

        public bool AskForCardChoice(string prompt, List<DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier, out List<List<Card>> answer)
        {
            throw new NotImplementedException();
        }

        public bool AskForMultipleChoice(string prompt, List<string> questions, out int answer)
        {
            answer = 0;
            return true;
        }

        public void NotifyCardMovement(List<CardsMovement> m, List<IGameLog> notes)
        {
            return;
        }
    }
}
