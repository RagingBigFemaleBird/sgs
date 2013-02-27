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
    public class GlobalClientUiProxy : IGlobalUiProxy
    {
        ClientNetworkUiProxy proxy;
        List<ClientNetworkUiProxy> inactiveProxies;
        Prompt prompt;
        List<OptionPrompt> questions;
        ICardUsageVerifier verifier;
        Game game;

        private class NullVerifier : ICardUsageVerifier
        {
            public VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                return VerifierResult.Fail;
            }

            public IList<CardHandler> AcceptableCardTypes
            {
                get { return null; }
            }

            public VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                return VerifierResult.Fail;
            }

            public UiHelper Helper
            {
                get { return new UiHelper(); }
            }
        }

        public GlobalClientUiProxy(Game g, ClientNetworkUiProxy p, List<ClientNetworkUiProxy> inactive)
        {
            game = g;
            proxy = p;
            inactiveProxies = inactive;
        }

        Thread pendingUiThread;

        public void Abort()
        {
            if (pendingUiThread != null)
            {
                pendingUiThread.Abort();
            }
        }

        public void AskForMultipleChoice(Prompt prompt, List<OptionPrompt> questions, List<Player> players, out Dictionary<Player, int> aanswer)
        {
            this.prompt = prompt;
            this.questions = questions;
            foreach (var inactiveProxy in inactiveProxies)
            {
                if (players.Contains(inactiveProxy.HostPlayer))
                {
                    inactiveProxy.TryAskForMultipleChoice(prompt, questions);
                }
            }            
            if (players.Contains(proxy.HostPlayer))
            {
                pendingUiThread = new Thread(AskMCQUiThread) { IsBackground = true };
                pendingUiThread.Start();
            }
            proxy.SimulateReplayDelay();
            aanswer = new Dictionary<Player, int>();
            foreach (var p in players)
            {
                int answer = 0;
                proxy.TryAnswerForMultipleChoice(out answer);
                aanswer.Add(p, answer);
            }
            if (players.Contains(proxy.HostPlayer))
            {
                pendingUiThread.Abort();                
                proxy.Freeze();
                proxy.NextQuestion();
            }
            pendingUiThread = null;
            foreach (var otherProxy in inactiveProxies)
            {
                if (players.Contains(otherProxy.HostPlayer))
                {
                    otherProxy.Freeze();
                }
            }
        }

        private void AskMCQUiThread()
        {
            proxy.TryAskForMultipleChoice(prompt, questions);
        }

        public void AskForMultipleCardUsage(Prompt prompt, ICardUsageVerifier verifier, List<Player> players, out Dictionary<Player, ISkill> askill, out Dictionary<Player, List<Card>> acards, out Dictionary<Player, List<Player>> aplayers)
        {
            this.prompt = prompt;
            this.verifier = verifier;
            foreach (var inactiveProxy in inactiveProxies)
            {
                if (players.Contains(inactiveProxy.HostPlayer))
                {
                    inactiveProxy.TryAskForCardUsage(new CardUsagePrompt(""), new NullVerifier());
                }
            }
            pendingUiThread = new Thread(AskUiThread) { IsBackground = true };
            if (players.Contains(proxy.HostPlayer))
            {
                pendingUiThread = new Thread(AskUiThread) { IsBackground = true };
                pendingUiThread.Start();
            }
            proxy.SimulateReplayDelay();
            askill = new Dictionary<Player, ISkill>();
            acards = new Dictionary<Player, List<Card>>();
            aplayers = new Dictionary<Player, List<Player>>();
            foreach (var p in players)
            {
                ISkill tempSkill;
                List<Card> tempCards;
                List<Player> tempPlayers;
                if (!proxy.TryAnswerForCardUsage(prompt, verifier, out tempSkill, out tempCards, out tempPlayers))
                {
                    tempCards = new List<Card>();
                    tempPlayers = new List<Player>();
                    tempSkill = null;
                }
                askill.Add(p, tempSkill);
                acards.Add(p, tempCards);
                aplayers.Add(p, tempPlayers);
            }
            if (players.Contains(proxy.HostPlayer))
            {
                pendingUiThread.Abort();
                proxy.Freeze();
                proxy.NextQuestion();
            }
            pendingUiThread = null;
            foreach (var otherProxy in inactiveProxies)
            {
                if (players.Contains(otherProxy.HostPlayer))
                {
                    otherProxy.Freeze();
                }
            }

        }

        public bool AskForCardUsage(Prompt prompt, ICardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players, out Player respondingPlayer)
        {
            this.prompt = prompt;
            this.verifier = verifier;
            foreach (var inactiveProxy in inactiveProxies)
            {
                inactiveProxy.TryAskForCardUsage(new CardUsagePrompt(""), new NullVerifier());
            }
            pendingUiThread = new Thread(AskUiThread) { IsBackground = true };
            pendingUiThread.Start();
            proxy.SimulateReplayDelay();
            if (!proxy.TryAnswerForCardUsage(prompt, verifier, out skill, out cards, out players))
            {
                cards = new List<Card>();
                players = new List<Player>();
                skill = null;
            }
            pendingUiThread.Abort();
            pendingUiThread = null;
            foreach (var otherProxy in inactiveProxies)
            {
               otherProxy.Freeze();
            }
            proxy.Freeze();
            proxy.NextQuestion();
            // try to determine who used this
            respondingPlayer = null;
            if (cards != null && cards.Count > 0)
            {
                respondingPlayer = cards[0].Place.Player;
            }
            else
            {
                foreach (var p in Game.CurrentGame.Players)
                {
                    if (p.ActionableSkills.Contains(skill))
                    {
                        respondingPlayer = p;
                        break;
                    }
                }
            }
            if (skill != null || (cards != null && cards.Count > 0))
            {
                Trace.Assert(respondingPlayer != null);
            }
            if (verifier.Verify(respondingPlayer, skill, cards, players) == VerifierResult.Success)
            {
                return true;
            }
            return false;
        }

        private void AskUiThread()
        {
            if (proxy.HostPlayer.IsDead) return;
            bool found = true;
            if (verifier.AcceptableCardTypes != null && verifier.AcceptableCardTypes.Count > 0)
            {
                found = false;
                foreach (var sk in proxy.HostPlayer.ActionableSkills)
                {
                    CardTransformSkill transformSkill = sk as CardTransformSkill;
                    if (transformSkill == null) continue;
                    if (transformSkill.PossibleResults == null) { found = true; break; }
                    var commonResult = from type1 in verifier.AcceptableCardTypes
                                       where transformSkill.PossibleResults.Any(ci => type1.GetType().IsAssignableFrom(ci.GetType()))
                                       select type1;
                    if (commonResult.Count() != 0)
                    {
                        found = true;
                    }
                }
                var commonResult2 = from type1 in verifier.AcceptableCardTypes
                                   where proxy.HostPlayer.HandCards().Any(ci => type1.GetType().IsAssignableFrom(ci.Type.GetType()))
                                   select type1;
                if (commonResult2.Count() != 0)
                {
                    found = true;
                }
            }
            if (!found) proxy.SkipAskForCardUsage();
            else proxy.TryAskForCardUsage(prompt, verifier);
        }

        public void AskForHeroChoice(Dictionary<Player, List<Card>> restDraw, Dictionary<Player, List<Card>> heroSelection, int numberOfHeroes, ICardChoiceVerifier verifier)
        {
            DeckType temp = new DeckType("Temp");
            if (!restDraw.ContainsKey(proxy.HostPlayer))
            {
                return;
            }
            Game.CurrentGame.Decks[proxy.HostPlayer, temp].Clear();
            Game.CurrentGame.Decks[proxy.HostPlayer, temp].AddRange(restDraw[proxy.HostPlayer]);
            List<DeckPlace> sourceDecks = new List<DeckPlace>();
            sourceDecks.Add(new DeckPlace(proxy.HostPlayer, temp));
            List<string> resultDeckNames = new List<string>() { "HeroChoice" };
            List<int> resultDeckMaximums = new List<int>() { numberOfHeroes };
            proxy.TryAskForCardChoice(new CardChoicePrompt("HeroChoice"), sourceDecks, resultDeckNames, resultDeckMaximums, verifier, null, null);
        }
    }
}
