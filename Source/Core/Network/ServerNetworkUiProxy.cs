using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using System.Diagnostics;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.UI;

namespace Sanguosha.Core.Network
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

        private ServerAsyncUiProxy proxy;
        private Server server;
        private int clientId;

        public ServerNetworkUiProxy(Server s, int id)
        {
            proxy = new ServerAsyncUiProxy();
            proxy.Gamer = s.Gamers[id];
            proxy.PlayerId = id;
            server = s;
            clientId = id;
        }

        public void SendMultipleCardUsageResponded()
        {
            server.SendObject(clientId, new MultiCardUsageResponded());
        }

        ISkill skillAnswer;
        List<Card> cardsAnswer;
        List<Player> playersAnswer;
        List<List<Card>> choiceAnswer;
        int multiAnswer;

        public bool TryAskForCardUsage(Prompt prompt, ICardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players)
        {
            cards = null;
            skill = null;
            players = null;
            int timeOut = TimeOutSeconds + (verifier.Helper != null ? verifier.Helper.ExtraTimeOutSeconds : 0);
            Trace.TraceInformation("Asking Card Usage to {0}, timeout {1}.", HostPlayer.Id, timeOut);
            var answerReady = new Semaphore(0, Int16.MaxValue);
            CardUsageAnsweredEventHandler handler = (s, c, p) =>
                {
                    skillAnswer = s;
                    cardsAnswer = c;
                    playersAnswer = p;
                    answerReady.Release(1);
                };
            proxy.CardUsageAnsweredEvent += handler;
            proxy.AskForCardUsage(prompt, verifier, timeOut);
            bool noAnswer = false;
            if (!answerReady.WaitOne(timeOut * 1000)) noAnswer = true;
            proxy.CardUsageAnsweredEvent -= handler;
            proxy.Freeze();
            if (noAnswer) return false;

            skill = skillAnswer;
            cards = cardsAnswer;
            players = playersAnswer;

            if (skill != null && !(skill is CheatSkill) && !HostPlayer.ActionableSkills.Contains(skill))
            {
                Trace.TraceWarning("Client DDOS!");
                return false;
            }
            if (cards != null)
            {
                foreach (var item in cards)
                {
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
                }
            }
            else
            {
                cards = new List<Card>();
            }
            if (players != null)
            {
                foreach (var item in players)
                {
                    if (item == null)
                    {
                        return false;
                    }
                }
            }
            else
            {
                players = new List<Player>();
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
                if (cards != null)
                {
                    foreach (var cd in cards)
                    {
                        cd.RevealOnce = true;
                    }
                }
                server.Gamers[i].Send(AskForCardUsageResponse.Parse(proxy.QuestionId, skill, cards, players, i));
            }

        }

        public void NextQuestion()
        {
            proxy.QuestionId++;
        }

        public bool AskForCardUsage(Prompt prompt, ICardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players)
        {
            bool ret = true;
            if (!TryAskForCardUsage(prompt, verifier, out skill, out cards, out players))
            {
                SendCardUsage(null, null, null, null);
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
            var answerReady = new Semaphore(0, Int16.MaxValue);
            CardChoiceAnsweredEventHandler handler = (c) =>
            {
                choiceAnswer = c;
                answerReady.Release(1);
            };
            proxy.CardChoiceAnsweredEvent += handler;
            proxy.AskForCardChoice(new Prompt(), sourceDecks, new List<string>(), resultDeckMaximums, verifier, timeOut, options, callback);
            bool noAnswer = false;
            if (!answerReady.WaitOne(timeOut * 1000)) noAnswer = true;
            proxy.CardChoiceAnsweredEvent -= handler;
            proxy.Freeze();
            if (noAnswer) return false;

            answer = choiceAnswer;
            if (answer != null)
            {
                foreach (var list in answer)
                {
                    foreach (var item in list)
                    {
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
                    }
                }
            }
            else
            {
                answer = new List<List<Card>>();
            }
            while (answer.Count < resultDeckMaximums.Count)
            {
                answer.Add(new List<Card>());
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
                int j = 0;
                if (answer != null)
                {
                    foreach (var cards in answer)
                    {
                        foreach (Card c in cards)
                        {
                            if (verifier.Helper != null && (verifier.Helper.RevealCards || (verifier.Helper.AdditionalFineGrainedCardChoiceRevealPolicy != null && verifier.Helper.AdditionalFineGrainedCardChoiceRevealPolicy[j])))
                            {
                                if (c.Place.DeckType != DeckType.Equipment && c.Place.DeckType != DeckType.DelayedTools)
                                {
                                    c.RevealOnce = true;
                                }
                            }
                        }
                        j++;
                    }
                }
                server.Gamers[i].Send(AskForCardChoiceResponse.Parse(proxy.QuestionId, answer, options == null? 0 : options.OptionResult, i));
            }
        }
        public bool AskForCardChoice(Prompt prompt, List<DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier, out List<List<Card>> answer, AdditionalCardChoiceOptions options, CardChoiceRearrangeCallback callback)
        {
            answer = null;
            bool ret = true;
            if (!TryAskForCardChoice(sourceDecks, resultDeckMaximums, verifier, out answer, options, callback))
            {
                SendCardChoice(null, null, null);
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
                SendMultipleChoice(0);
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
                server.Gamers[i].Send(new AskForMultipleChoiceResponse() { ChoiceIndex = answer, Id = proxy.QuestionId });
            }
        }

        public bool TryAskForMultipleChoice(out int answer)
        {
            answer = 0;
            Trace.TraceInformation("Asking Card Usage to {0}, timeout {1}.", HostPlayer.Id, TimeOutSeconds);
            var answerReady = new Semaphore(0, Int16.MaxValue);
            MultipleChoiceAnsweredEventHandler handler = (c) =>
            {
                multiAnswer = c;
                answerReady.Release(1);
            };
            proxy.MultipleChoiceAnsweredEvent += handler;
            proxy.AskForMultipleChoice(new Prompt(), new List<OptionPrompt>(), TimeOutSeconds);
            bool noAnswer = false;
            if (!answerReady.WaitOne(TimeOutSeconds * 1000)) noAnswer = true;
            proxy.MultipleChoiceAnsweredEvent -= handler;
            proxy.Freeze();
            if (noAnswer) return false;

            answer = multiAnswer;
            return true;
        }

        public int TimeOutSeconds { get; set; }
    }
}
