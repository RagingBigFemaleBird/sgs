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

        public void Freeze()
        {
        }
        
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

        public void SendNoAnswer()
        {
            int i;
            for (i = 0; i < server.MaxClients; i++)
            {
                server.SendObject(i, 0);
            }
        }

        public bool TryAskForCardUsage(Prompt prompt, ICardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players)
        {
            cards = null;
            skill = null;
            players = null;
            Trace.TraceInformation("Asking Card Usage to {0}, timeout {1}.", HostPlayer.Id, TimeOutSeconds);
            int? count;
            if (!server.ExpectNext(clientId, TimeOutSeconds))
            {
                return false;
            }
            count = server.GetInt(clientId, 0);
            if (count == null || count == 0)
            {
                return false;
            }
            if (count == null)
            {
                return false;
            }
            count = server.GetInt(clientId, 0);
            if (count == 1)
            {
                skill = server.GetSkill(clientId, 0);
                if (skill == null)
                {
                    return false;
                }
                if (!HostPlayer.ActionableSkills.Contains(skill))
                {
                    Trace.TraceWarning("Client DDOS!");
                    return false;
                }
            }
            count = server.GetInt(clientId, 0);
            if (count == null)
            {
                return false;
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
                    return false;
                }
                if (item.Owner != HostPlayer)
                {
                    Trace.TraceWarning("Client DDOS!");
                    return false;
                }
                cards.Add(item);
            }
            count = server.GetInt(clientId, 0);
            if (count == null)
            {
                return false;
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
                    return false;
                }
                players.Add(item);
            }
            if (verifier.FastVerify(skill, cards, players) != VerifierResult.Success)
            {
                Trace.TraceWarning("Client seems to be sending invalid answers at us. DDOS?");
                cards = null;
                skill = null;
                players = null;
                return false;
            }
            return true;
        }

        public void SendCardUsage(ISkill skill, List<Card> cards, List<Player> players)
        {
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

        }

        public void NextQuestion()
        {
            server.CommIdInc(clientId);
        }

        public bool AskForCardUsage(Prompt prompt, ICardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players)
        {
            bool ret = true;
            if (!TryAskForCardUsage(prompt, verifier, out skill, out cards, out players))
            {
                SendNoAnswer();
                ret = false;
            }
            else
            {
                SendCardUsage(skill, cards, players);
            }
            NextQuestion();
            if (cards == null)
            {
                cards = new List<Card>();
            }
            if (players == null)
            {
                players = new List<Player>();
            }
            return ret;
        }

        public bool AskForCardChoice(Prompt prompt, List<DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier, out List<List<Card>> answer)
        {
            answer = null;
            return false;
        }



        public bool AskForMultipleChoice(Prompt prompt, List<string> questions, out int answer)
        {
            bool ret = true;
            if (!TryAskForMultipleChoice(prompt, questions, out answer))
            {
                SendNoAnswer();
                ret = false;
            }
            else
            {
                SendMultipleChoice(answer);
            }
            NextQuestion();
            return ret;
        }

        private void SendMultipleChoice(int answer)
        {
            for (int i = 0; i < server.MaxClients; i++)
            {
                server.SendObject(i, answer);
            }
        }

        private bool TryAskForMultipleChoice(Prompt prompt, List<string> questions, out int answer)
        {
            answer = 0;
            Trace.TraceInformation("Asking Multiple choice to {0}, timeout {1}.", HostPlayer.Id, TimeOutSeconds);
            int? count;
            if (!server.ExpectNext(clientId, TimeOutSeconds))
            {
                return false;
            }
            count = server.GetInt(clientId, 0);
            if (count == null)
            {
                return false;
            }
            answer = (int)count;
            return true;
        }

        public void NotifyCardMovement(List<CardsMovement> m, List<IGameLog> notes)
        {
            return;
        }
        public int TimeOutSeconds { get; set; }
    }
}
