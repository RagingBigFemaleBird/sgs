using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using System.Threading;
using System.Diagnostics;

namespace Sanguosha.Core.UI
{
    public class GlobalServerUiProxy : IGlobalUiProxy
    {
        Dictionary<Player, ServerNetworkUiProxy> proxy;
        Dictionary<Player, Thread> proxyListener;
        Semaphore semAccess;
        Semaphore semWake;
        Semaphore semDone;
        ISkill answerSkill;
        List<Card> answerCard;
        List<Player> answerPlayer;
        Player responder;
        Game game;

        private struct UsageListenerThreadParameters
        {
            public ServerNetworkUiProxy proxy;
            public Prompt prompt;
            public ICardUsageVerifier verifier;
            public Player player;
        }

        private struct ChoiceListenerThreadParameters
        {
            public ServerNetworkUiProxy proxy;
            public ICardChoiceVerifier verifier;
            public Player player;
            public List<DeckPlace> places;
            public List<int> resultMax;
            public AdditionalCardChoiceOptions options;
        }

        private struct MCQListenerThreadParameters
        {
            public ServerNetworkUiProxy proxy;
            public Prompt prompt;
            public Player player;
        }
        Dictionary<Player, int> manswerMCQ;

        public void AskForMultipleChoice(Prompt prompt, List<OptionPrompt> questions, List<Player> players, out Dictionary<Player, int> aanswer)
        {
            proxyListener = new Dictionary<Player, Thread>();
            semAccess = new Semaphore(1, 1);
            semWake = new Semaphore(0, 2);
            semDone = new Semaphore(players.Count - 1, players.Count - 1);
            manswerMCQ = new Dictionary<Player,int>();
            foreach (var player in players)
            {
                if (!proxy.ContainsKey(player))
                {
                    continue;
                }
                MCQListenerThreadParameters para = new MCQListenerThreadParameters();
                para.player = player;
                para.prompt = prompt;
                para.proxy = proxy[player];
                Thread t = new Thread(
                    (ParameterizedThreadStart)
                    ((p) =>
                    {
                        MultiMCQProxyListenerThread((MCQListenerThreadParameters)p);
                    })) { IsBackground = true };
                t.Start(para);
                proxyListener.Add(player, t);
            }
            semWake.WaitOne(TimeOutSeconds * 1000);
            semAccess.WaitOne(100);

            foreach (var pair in proxyListener)
            {
                pair.Value.Abort();
                proxy[pair.Key].NextQuestion();
            }

            foreach (var player in players)
            {
                if (!manswerMCQ.ContainsKey(player))
                {
                    manswerMCQ.Add(player, 0);
                }
            }

            foreach (var player in Game.CurrentGame.Players)
            {
                if (!proxy.ContainsKey(player))
                {
                    continue;
                }
                else
                {
                    foreach (var p in players)
                    {
                        proxy[player].SendMultipleChoice(manswerMCQ[p]);
                    }
                    break;
                }
            }
            aanswer = manswerMCQ;
        }

        private void MultiMCQProxyListenerThread(MCQListenerThreadParameters para)
        {
            game.RegisterCurrentThread();
            int answer = 0;
            if (para.proxy.TryAskForMultipleChoice(out answer))
            {

                semAccess.WaitOne();
                manswerMCQ.Add(para.player, answer);
                semAccess.Release(1);
            }
            if (!semDone.WaitOne(0))
            {
                Trace.TraceInformation("All done");
                semWake.Release(1);
            }
        }

        Dictionary<Player, ISkill> manswerSkill;
        Dictionary<Player, List<Card>> manswerCards;
        Dictionary<Player, List<Player>> manswerPlayers;

        public void AskForMultipleCardUsage(Prompt prompt, ICardUsageVerifier verifier, List<Player> players, out Dictionary<Player, ISkill> askill, out Dictionary<Player, List<Card>> acards, out Dictionary<Player, List<Player>> aplayers)
        {
            proxyListener = new Dictionary<Player, Thread>();
            semAccess = new Semaphore(1, 1);
            semWake = new Semaphore(0, 2);
            semDone = new Semaphore(players.Count - 1, players.Count - 1);
            manswerSkill = new Dictionary<Player,ISkill>();
            manswerCards = new Dictionary<Player,List<Card>>();
            manswerPlayers = new Dictionary<Player,List<Player>>();
            foreach (var player in players)
            {
                if (!proxy.ContainsKey(player))
                {
                    continue;
                }
                UsageListenerThreadParameters para = new UsageListenerThreadParameters();
                para.player = player;
                para.prompt = prompt;
                para.proxy = proxy[player];
                para.verifier = verifier;
                Thread t = new Thread(
                    (ParameterizedThreadStart)
                    ((p) =>
                    {
                        MultiUsageProxyListenerThread((UsageListenerThreadParameters)p);
                    })) { IsBackground = true };
                t.Start(para);
                proxyListener.Add(player, t);
            }
            semWake.WaitOne(TimeOutSeconds * 1000);
            semAccess.WaitOne(100);

            foreach (var pair in proxyListener)
            {
                pair.Value.Abort();
                proxy[pair.Key].NextQuestion();
            }

            foreach (var player in players)
            {
                if (!manswerSkill.ContainsKey(player))
                {
                    manswerSkill.Add(player, null);
                }
                if (!manswerCards.ContainsKey(player))
                {
                    manswerCards.Add(player, new List<Card>());
                }
                if (!manswerPlayers.ContainsKey(player))
                {
                    manswerPlayers.Add(player, new List<Player>());
                }
            }

            foreach (var player in Game.CurrentGame.Players)
            {
                if (!proxy.ContainsKey(player))
                {
                    continue;
                }
                else
                {
                    foreach (var p in players)
                    {
                        proxy[player].SendCardUsage(manswerSkill[p], manswerCards[p], manswerPlayers[p], verifier);
                    }
                    break;
                }
            }

            askill = manswerSkill;
            acards = manswerCards;
            aplayers = manswerPlayers;

        }

        private void MultiUsageProxyListenerThread(UsageListenerThreadParameters para)
        {
            game.RegisterCurrentThread();
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (para.proxy.TryAskForCardUsage(para.prompt, para.verifier, out skill, out cards, out players))
            {

                semAccess.WaitOne();
                manswerSkill.Add(para.player, skill);
                manswerCards.Add(para.player, cards);
                manswerPlayers.Add(para.player, players);
                semAccess.Release(1);
                para.proxy.SendMultipleCardUsageResponded();
            }
            if (!semDone.WaitOne(0))
            {
                Trace.TraceInformation("All done");
                semWake.Release(1);
            }
        }


        public bool AskForCardUsage(Prompt prompt, ICardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players, out Player respondingPlayer)
        {
            proxyListener = new Dictionary<Player, Thread>();
            semAccess = new Semaphore(1, 1);
            semWake = new Semaphore(0, 2);
            semDone = new Semaphore(Game.CurrentGame.AlivePlayers.Count - 1, Game.CurrentGame.AlivePlayers.Count - 1);
            answerSkill = null;
            answerCard = null;
            answerPlayer = null;
            respondingPlayer = null;
            foreach (var player in Game.CurrentGame.AlivePlayers)
            {
                if (!proxy.ContainsKey(player))
                {
                    continue;
                }
                UsageListenerThreadParameters para = new UsageListenerThreadParameters();
                para.prompt = prompt;
                para.proxy = proxy[player];
                para.verifier = verifier;
                Thread t = new Thread(
                    (ParameterizedThreadStart)
                    ((p) => { 
                        UsageProxyListenerThread((UsageListenerThreadParameters)p);
                    })) { IsBackground = true };
                t.Start(para);
                proxyListener.Add(player, t);
            }
            bool ret = true;
            if (!semWake.WaitOne(TimeOutSeconds * 1000))
            {
                semAccess.WaitOne(0);
                skill = null;
                cards = null;
                players = null;
                respondingPlayer = null;
                ret = false;
            }
            else
            {
                skill = answerSkill;
                cards = answerCard;
                players = answerPlayer;
                respondingPlayer = responder;
            }
            //if it didn't change, then semDone was triggered
            if (skill == null && cards == null && players == null)
            {
                ret = false;
            }
            if (cards == null) cards = new List<Card>();
            if (players == null) players = new List<Player>();
            foreach (var pair in proxyListener)
            {
                pair.Value.Abort();
                proxy[pair.Key].NextQuestion();
            }
            foreach (var player in Game.CurrentGame.Players)
            {
                if (!proxy.ContainsKey(player))
                {
                    continue;
                }
                else
                {
                    proxy[player].SendCardUsage(skill, cards, players, verifier);
                    break;
                }
            }

            return ret;
        }

        private void UsageProxyListenerThread(UsageListenerThreadParameters para)
        {
            game.RegisterCurrentThread();
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (para.proxy.TryAskForCardUsage(para.prompt, para.verifier, out skill, out cards, out players))
            {

                semAccess.WaitOne();
                answerSkill = skill;
                answerCard = cards;
                answerPlayer = players;
                responder = para.proxy.HostPlayer;
                semWake.Release(1);
            }
            if (!semDone.WaitOne(0))
            {
                Trace.TraceInformation("All done");
                semWake.Release(1);
            }
        }

        public GlobalServerUiProxy(Game g, Dictionary<Player, IUiProxy> p)
        {
            game = g;
            proxy = new Dictionary<Player, ServerNetworkUiProxy>();
            foreach (var v in p)
            {
                if (!(v.Value is ServerNetworkUiProxy))
                {
                    Trace.TraceWarning("Some of my proxies are not server network proxies!");
                    continue;
                }
                proxy.Add(v.Key, v.Value as ServerNetworkUiProxy);
            }
        }

        public int TimeOutSeconds { get; set; }

        Dictionary<Player, List<Card>> answerHero;

        public void AskForHeroChoice(Dictionary<Player, List<Card>> restDraw, Dictionary<Player, List<Card>> heroSelection, int numberOfHeroes, ICardChoiceVerifier verifier)
        {
            proxyListener = new Dictionary<Player, Thread>();
            semAccess = new Semaphore(1, 1);
            semWake = new Semaphore(0, 2);
            semDone = new Semaphore(proxy.Count - 2, proxy.Count - 1);
            answerHero = heroSelection;
            DeckType temp = new DeckType("Temp");
            foreach (var player in Game.CurrentGame.Players)
            {
                if (!proxy.ContainsKey(player) || player.Role == Role.Ruler)
                {
                    continue;
                }
                ChoiceListenerThreadParameters para = new ChoiceListenerThreadParameters();
                para.proxy = proxy[player];
                para.verifier = verifier;
                para.player = player;
                para.places = new List<DeckPlace>() { new DeckPlace(player, temp) };
                para.options = null;
                para.resultMax = new List<int> { numberOfHeroes };
                Game.CurrentGame.Decks[player, temp].Clear();
                Game.CurrentGame.Decks[player, temp].AddRange(restDraw[player]);
                Thread t = new Thread(
                    (ParameterizedThreadStart)
                    ((p) =>
                    {
                        ChoiceProxyListenerThread((ChoiceListenerThreadParameters)p);
                    })) { IsBackground = true };
                t.Start(para);
                proxyListener.Add(player, t);
            }
            if (!semWake.WaitOne(TimeOutSeconds * 1000))
            {
                semAccess.WaitOne(0);
            }

            foreach (var pair in proxyListener)
            {
                pair.Value.Abort();
            }

        }

        private void ChoiceProxyListenerThread(ChoiceListenerThreadParameters para)
        {
            game.RegisterCurrentThread();
            List<List<Card>> answer;
            if (para.proxy.TryAskForCardChoice(para.places, para.resultMax, para.verifier, out answer, para.options, null))
            {
                semAccess.WaitOne();
                if (answer != null && answer.Count != 0 && answer[0] != null && answer[0].Count == para.resultMax[0])
                {
                    answerHero.Add(para.player, answer[0]);
                }
                semAccess.Release();
            }
            if (!semDone.WaitOne(0))
            {
                Trace.TraceInformation("All done");
                semWake.Release(1);
            }
        }

        public void Abort()
        {
            // throw new NotImplementedException();
        }
    }
}
