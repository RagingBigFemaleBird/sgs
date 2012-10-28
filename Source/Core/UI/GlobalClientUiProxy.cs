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
        ICardUsageVerifier verifier;
        Game game;

        private class NullVerifier : ICardUsageVerifier
        {
            public VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                return VerifierResult.Fail;
            }

            public IList<CardHandler> AcceptableCardType
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

        public bool AskForCardUsage(Prompt prompt, ICardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players, out Player respondingPlayer)
        {
            this.prompt = prompt;
            this.verifier = verifier;
            foreach (var z in inactiveProxies)
            {
                z.TryAskForCardUsage(new CardUsagePrompt(""), new NullVerifier());
            }
            Thread t = new Thread(AskUiThread) { IsBackground = true };
            t.Start();
            if (!proxy.TryAnswerForCardUsage(prompt, verifier, out skill, out cards, out players))
            {
                cards = null;
                skill = null;
            }
            t.Abort();
            foreach (var otherProxy in inactiveProxies)
            {
               otherProxy.Freeze();
            }
            proxy.Freeze();
            proxy.NextQuestion();
            //try to determine who used this
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
            game.RegisterCurrentThread();
            proxy.TryAskForCardUsage(prompt, verifier);
        }

        public void AskForHeroChoice(Dictionary<Player, List<Card>> restDraw, Dictionary<Player, Card> heroSelection)
        {
            DeckType temp = new DeckType("Temp");
            if (!restDraw.ContainsKey(proxy.HostPlayer))
            {
                return;
            }
            Game.CurrentGame.Decks[proxy.HostPlayer, temp].AddRange(restDraw[proxy.HostPlayer]);
            List<DeckPlace> sourceDecks = new List<DeckPlace>();
            sourceDecks.Add(new DeckPlace(proxy.HostPlayer, temp));
            List<string> resultDeckNames = new List<string>() { "HeroChoice" };
            List<int> resultDeckMaximums = new List<int>() { 1 };
            proxy.TryAskForCardChoice(new CardChoicePrompt("HeroChoice"), sourceDecks, resultDeckNames, resultDeckMaximums, new AlwaysTrueChoiceVerifier(), new List<bool>() {false}, null);
        }
    }
}
