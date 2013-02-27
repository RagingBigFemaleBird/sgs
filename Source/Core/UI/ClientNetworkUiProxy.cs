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
using System.Threading;
using Sanguosha.Core.Utils;

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
        public bool Suppressed { get; set; }
        public ClientNetworkUiProxy(IUiProxy p, Client c, bool a)
        {
            proxy = p;
            client = c;
            active = a;
            lastTS = 0;
        }

        public void SkipAskForCardUsage()
        {
            client.AnswerNext();
            client.AnswerItem(0);
            client.Flush();
        }

        public void TryAskForCardUsage(Prompt prompt, ICardUsageVerifier verifier)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (!active)
            {
                if (Suppressed) return;
                proxy.AskForCardUsage(prompt, verifier, out skill, out cards, out players);
                return;
            }
            if (Suppressed || !proxy.AskForCardUsage(prompt, verifier, out skill, out cards, out players))
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
            client.Flush();
        }

        public bool TryAnswerForCardUsage(Prompt prompt, ICardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players)
        {
            skill = null;
            cards = new List<Card>();
            players = new List<Player>();
            object o = client.Receive();
            if (o == null)
            {
                return false;
            }
            if ((int)o == 0)
            {
                return false;
            }
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

        private static DateTime? startTimeStamp;

        void RecordTimeStamp()
        {            
            if (client.RecordStream == null) return;
            Trace.Assert(Game.CurrentGame.ReplayController == null);
            if (startTimeStamp == null) startTimeStamp = DateTime.Now;
            TimeSpan t = DateTime.Now - (DateTime)startTimeStamp;
            Int64 msSinceEpoch = (Int64)t.TotalMilliseconds;
            client.RecordStream.Write(BitConverter.GetBytes(msSinceEpoch), 0, 8);
        }

        private static Int64 lastTS;

        void GetTimeStamp()
        {
            if (Game.CurrentGame.ReplayController == null) return;
            byte[] ts = new byte[8];
            client.ReplayStream.Read(ts, 0, 8);
            Int64 last = BitConverter.ToInt64(ts, 0);
            if (lastTS != 0)
            {
                Int64 toSleep = last - lastTS;
                ReplayController controller = Game.CurrentGame.ReplayController;
                if (!controller.NoDelays)
                {
                    if (controller.EvenDelays) toSleep = ReplayController.EvenReplayBaseSpeedInMs;
                    if (controller.Speed != 0) toSleep = (Int64)(((double)toSleep) / controller.Speed);
                    controller.Lock();
                    controller.Unlock();
                    Thread.Sleep((int)toSleep);
                }
            }
            lastTS = last;
        }

        public void SimulateReplayDelay()
        {
            RecordTimeStamp();
            GetTimeStamp();
        }

        public bool AskForCardUsage(Prompt prompt, ICardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players)
        {
            Trace.TraceInformation("Asking Card Usage to {0}.", HostPlayer.Id);
            TryAskForCardUsage(prompt, verifier);
            SimulateReplayDelay();
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
#if DEBUG
                Trace.Assert(verifier.FastVerify(HostPlayer, skill, cards, players) == VerifierResult.Success);
#endif
                return true;
            }
            proxy.Freeze();
            return false;
        }

        public bool AskForCardChoice(Prompt prompt, List<DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier, out List<List<Card>> answer, AdditionalCardChoiceOptions options = null, CardChoiceRearrangeCallback callback = null)
        {
            Trace.TraceInformation("Asking Card Choice to {0}.", HostPlayer.Id);
            TryAskForCardChoice(prompt, sourceDecks, resultDeckNames, resultDeckMaximums, verifier, options, callback);
            SimulateReplayDelay();
            if (active)
            {
                NextQuestion();
            }
            else
            {
                Trace.TraceInformation("Not active player, defaulting.");
            }
            if (TryAnswerForCardChoice(prompt, verifier, out answer, options, callback))
            {
                proxy.Freeze();
#if DEBUG
                Trace.Assert(verifier.Verify(answer) == VerifierResult.Success);
#endif
                return true;
            }
            proxy.Freeze();
            return false;
        }

        public bool AskForMultipleChoice(Prompt prompt, List<OptionPrompt> questions, out int answer)
        {
            Trace.TraceInformation("Asking Multiple choice to {0}.", HostPlayer.Id);
            TryAskForMultipleChoice(prompt, questions);
            SimulateReplayDelay();
            if (active)
            {
                NextQuestion();
            }
            else
            {
                Trace.TraceInformation("Not active player, defaulting.");
            }
            if (TryAnswerForMultipleChoice(out answer))
            {
                Game.CurrentGame.NotificationProxy.NotifyMultipleChoiceResult(HostPlayer, questions[answer]);
                proxy.Freeze();
                return true;
            }
            Game.CurrentGame.NotificationProxy.NotifyMultipleChoiceResult(HostPlayer, questions[answer]);
            proxy.Freeze();
            return false;
        }

        public bool TryAnswerForMultipleChoice(out int answer)
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

        public void TryAskForMultipleChoice(Prompt prompt, List<OptionPrompt> questions)
        {
            int answer;
            if (!active)
            {
                if (Suppressed) return;
                proxy.AskForMultipleChoice(prompt, questions, out answer);
                return;
            }
            if (Suppressed || !proxy.AskForMultipleChoice(prompt, questions, out answer))
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
            client.Flush();
        }

        public void TryAskForCardChoice(Prompt prompt, List<DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier, AdditionalCardChoiceOptions options, CardChoiceRearrangeCallback callback)
        {
            List<List<Card>> answer;
            if (!active)
            {
                if (Suppressed) return;
                proxy.AskForCardChoice(prompt, sourceDecks, resultDeckNames, resultDeckMaximums, verifier, out answer, options, callback);
                return;
            }
            if (Suppressed || !proxy.AskForCardChoice(prompt, sourceDecks, resultDeckNames, resultDeckMaximums, verifier, out answer, options, callback) ||
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
                if (options != null && options.Options != null)
                {
                    client.AnswerItem(options.OptionResult);
                }
            }
            client.Flush();
        }

        public bool TryAnswerForCardChoice(Prompt prompt, ICardChoiceVerifier verifier, out List<List<Card>> answer, AdditionalCardChoiceOptions options, CardChoiceRearrangeCallback callback)
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
            if (options != null && options.Options != null)
            {
                options.OptionResult = (int)client.Receive();
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
