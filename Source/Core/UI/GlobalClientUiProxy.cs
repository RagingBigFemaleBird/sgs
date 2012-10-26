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
        Prompt prompt;
        ICardUsageVerifier verifier;
        Game game;
        public GlobalClientUiProxy(Game g, ClientNetworkUiProxy p)
        {
            game = g;
            proxy = p;
        }

        public bool AskForCardUsage(Prompt prompt, ICardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players, out Player respondingPlayer)
        {
            this.prompt = prompt;
            this.verifier = verifier;
            Thread t = new Thread(AskUiThread) { IsBackground = true };
            t.Start();
            if (!proxy.TryAnswerForCardUsage(prompt, verifier, out skill, out cards, out players))
            {
                cards = null;
                skill = null;
            }
            t.Abort();
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
            if (verifier.Verify(skill, cards, players) == VerifierResult.Success)
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
            Game.CurrentGame.Decks[null, temp].AddRange(restDraw[proxy.HostPlayer]);
            List<DeckPlace> sourceDecks = new List<DeckPlace>();
            sourceDecks.Add(new DeckPlace(null, temp));
            List<string> resultDeckNames = new List<string>() { "HeroChoice" };
            List<int> resultDeckMaximums = new List<int>() { 1 };
            List<List<Card>> answer;
            proxy.AskForCardChoice(new CardChoicePrompt("HeroChoice"), sourceDecks, resultDeckNames, resultDeckMaximums, new SimpleCardChoiceVerifier(), out answer, null, null);
        }
    }
}
