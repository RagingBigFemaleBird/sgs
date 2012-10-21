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
        string prompt;
        CardUsageVerifier verifier;
        Game game;
        public GlobalClientUiProxy(Game g, ClientNetworkUiProxy p)
        {
            game = g;
            proxy = p;
        }

        public bool AskForCardUsage(string prompt, CardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players, out Player respondingPlayer)
        {
            this.prompt = prompt;
            this.verifier = verifier;
            Thread t = new Thread(AskUiThread);
            t.Start();
            if (!proxy.TryAnswerForCardUsage(prompt, verifier, out skill, out cards, out players))
            {
                cards = null;
                skill = null;
            }
            t.Abort();
            proxy.NextComm();
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
    }
}
