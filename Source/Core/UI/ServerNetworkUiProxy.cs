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

        public void SendMultipleCardUsageResponded()
        {
            server.SendMultipleCardUsageResponded(clientId);
        }

        public void SendNoAnswer()
        {
            int i;
            for (i = 0; i < server.MaxClients; i++)
            {
                server.SendObject(i, 0);
                server.Flush(i);
            }
        }

        public bool TryAskForCardUsage(Prompt prompt, ICardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players)
        {
            cards = null;
            skill = null;
            players = null;
            int timeOut = TimeOutSeconds + (verifier.Helper != null ? verifier.Helper.ExtraTimeOutSeconds : 0);
            Trace.TraceInformation("Asking Card Usage to {0}, timeout {1}.", HostPlayer.Id, timeOut);
            int? count;
            if (!server.ExpectNext(clientId, timeOut))
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
                if (!(skill is CheatSkill) && !HostPlayer.ActionableSkills.Contains(skill))
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
            cards = new List<Card>();
            while (count-- > 0)
            {
                Card item = server.GetCard(clientId, 0);
                if (item == null)
                {
                    return false;
                }
                if (item.Owner != HostPlayer)
                {
                    if (!(verifier.Helper.OtherDecksUsed.Any(dc => dc == item.Place.DeckType) ||
                        (skill != null && skill.Helper.OtherDecksUsed.Any(dc => dc == item.Place.DeckType))))
                    {                        
                        Trace.TraceWarning("Client hacking cards!");
                        return false;
                    }
                }
                cards.Add(item);
            }
            count = server.GetInt(clientId, 0);
            if (count == null)
            {
                return false;
            }
            players = new List<Player>();
            while (count-- > 0)
            {
                Player item = server.GetPlayer(clientId, 0);
                if (item == null)
                {
                    return false;
                }
                players.Add(item);
            }
            bool requireUnique = true;
            if (skill is ActiveSkill)
            {
                if ((skill as ActiveSkill).Helper != null && (skill as ActiveSkill).Helper.IsPlayerRepeatable) requireUnique = false;
            }
            if ((players != null && players.Distinct().Count() != players.Count && requireUnique) ||
                verifier.FastVerify(HostPlayer, skill, cards, players) != VerifierResult.Success)
            {
                Trace.TraceWarning("Client seems to be sending invalid answers at us. DDOS?");
                cards = new List<Card>();
                skill = null;
                players = new List<Player>();
                return false;
            }
            return true;
        }

        public void SendCardUsage(ISkill skill, List<Card> cards, List<Player> players, ICardUsageVerifier verifier)
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
                    foreach (Card c in cards)
                    {
                        if (!(skill != null && skill.Helper.NoCardReveal) && !(verifier.Helper.NoCardReveal))
                        {
                            if (c.Place.DeckType != DeckType.Equipment && c.Place.DeckType != DeckType.DelayedTools)
                            {
                                c.RevealOnce = true;
                            }
                        }
                        if (c.Place.DeckType == DeckType.Equipment || c.Place.DeckType == DeckType.DelayedTools)
                        {
                            c.RevealOnce = false;
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
                server.Flush(i);
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
                SendCardUsage(skill, cards, players, verifier);
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


        public bool TryAskForCardChoice(List<DeckPlace> sourceDecks, List<int> resultDeckMaximums, ICardChoiceVerifier verifier, out List<List<Card>> answer, AdditionalCardChoiceOptions options, CardChoiceRearrangeCallback callback)
        {
            answer = null;
            int timeOut = TimeOutSeconds + (verifier.Helper != null ? verifier.Helper.ExtraTimeOutSeconds : 0);
            Trace.TraceInformation("Asking Card Choice to {0}, timeout {1}.", HostPlayer.Id, timeOut);
            int? count;
            if (!server.ExpectNext(clientId, timeOut))
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
            answer = new List<List<Card>>();

            count = server.GetInt(clientId, 0);
            while (count-- > 0)
            {
                int? subCount = server.GetInt(clientId, 0); ;
                var theList = new List<Card>();
                answer.Add(theList);
                while (subCount-- > 0)
                {
                    Card item = server.GetCard(clientId, 0);
                    if (item == null)
                    {
                        return false;
                    }
                    bool exist = false;
                    foreach (var v in sourceDecks)
                    {
                        if (Game.CurrentGame.Decks[v].Contains(item))
                        {
                            exist = true;
                            break;
                        }
                    }
                    if (options != null && options.DefaultResult != null)
                    {
                        foreach (var dk in options.DefaultResult)
                        {
                            if (dk.Contains(item))
                            {
                                exist = true;
                                break;
                            }
                        }
                    }
                    if (!exist)
                    {
                        Trace.TraceWarning("Client DDOS!");
                        return false;
                    }
                    theList.Add(item);
                }
            }
            if (options != null && options.Options != null)
            {
                int? opr = server.GetInt(clientId, 0);
                if (opr == null) return false;
                if (opr < 0 || opr >= options.Options.Count) return false;
                options.OptionResult = (int)opr;
            }

            if (verifier.Verify(answer) != VerifierResult.Success)
            {
                Trace.TraceWarning("Client seems to be sending invalid answers at us. DDOS?");
                answer = null;
                return false;
            }
            return true;
        }

        public void SendCardChoice(ICardChoiceVerifier verifier, List<List<Card>> answer, AdditionalCardChoiceOptions options)
        {
            for (int i = 0; i < server.MaxClients; i++)
            {
                server.SendObject(i, 1);
                server.SendObject(i, answer.Count);
                int j = 0;
                foreach (var cards in answer)
                {
                    server.SendObject(i, cards.Count);
                    foreach (Card c in cards)
                    {
                        if (verifier.Helper != null && (verifier.Helper.RevealCards || (verifier.Helper.AdditionalFineGrainedCardChoiceRevealPolicy != null && verifier.Helper.AdditionalFineGrainedCardChoiceRevealPolicy[j])))
                        {
                            if (c.Place.DeckType != DeckType.Equipment && c.Place.DeckType != DeckType.DelayedTools)
                            {
                                c.RevealOnce = true;
                            }
                        }
                        server.SendObject(i, c);
                    }
                    j++;
                }
                if (options != null && options.Options != null) server.SendObject(i, options.OptionResult);
                server.Flush(i);
            }

        }
        public bool AskForCardChoice(Prompt prompt, List<DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier, out List<List<Card>> answer, AdditionalCardChoiceOptions options, CardChoiceRearrangeCallback callback)
        {
            answer = null;
            bool ret = true;
            if (!TryAskForCardChoice(sourceDecks, resultDeckMaximums, verifier, out answer, options, callback))
            {
                SendNoAnswer();
                ret = false;
            }
            else
            {
                SendCardChoice(verifier, answer, options);
            }
            NextQuestion();
            if (answer == null)
            {
                answer = new List<List<Card>>();
                foreach (var v in resultDeckMaximums)
                {
                    answer.Add(new List<Card>());
                }
            }
            return ret;
        }


        public bool AskForMultipleChoice(Prompt prompt, List<OptionPrompt> questions, out int answer)
        {
            bool ret = true;
            if (!TryAskForMultipleChoice(out answer))
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

        public void SendMultipleChoice(int answer)
        {
            for (int i = 0; i < server.MaxClients; i++)
            {
                server.SendObject(i, answer);
                server.Flush(i);
            }
        }

        public bool TryAskForMultipleChoice(out int answer)
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

        public int TimeOutSeconds { get; set; }
    }
}
