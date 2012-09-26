using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Players;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Cards.Battle;

namespace Sanguosha.Core.Games
{
    public enum Role
    {
        Unknown,
        Ruler,
        Rebel,
        Loyalist,
        Defector
    }

    public class RoleGame : Game
    {
        public class PlayerActionTrigger : Trigger
        {
            private class PlayerActionStageVerifier : ICardUsageVerifier
            {
                public VerifierResult Verify(Skill skill, List<Card> cards, List<Player> players)
                {
                    if (cards == null)
                    {
                        return VerifierResult.Fail;
                    }
                    return Game.CurrentGame.CardHandlers[cards[0].Type].Verify(skill, cards, players);
                }
            }
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Player currentPlayer = eventArgs.Game.CurrentPlayer;
                Trace.TraceInformation("Player {0} action.", currentPlayer.Id);
                while (true)
                {
                    IUiProxy proxy = new ConsoleUiProxy();
                    Skill skill;
                    List<Card> cards;
                    List<Player> players;
                    PlayerActionStageVerifier v = new PlayerActionStageVerifier();
                    proxy.AskForCardUsage("Player Action Stage", v, out skill, out cards, out players);
                    if (skill != null)
                    {
                    }
                    else
                    {
                        if (cards == null)
                        {
                            continue;
                        }
                        Trace.Assert(cards[0] != null && cards.Count == 1);
                        Trace.TraceInformation("Player used {0}", cards[0].Type);
                    }
                }
            }
        }

        public class RoleGameRuleTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Game game = eventArgs.Game;

                if (game.Players.Count == 0)
                {
                    return;
                }

                // Deal everyone 4 cards
                foreach (Player player in game.Players)
                {
                    game.DrawCards(player, 1);
                }
                game.Decks[game.Players[2], DeckType.Hand].Clear();
                game.CurrentPlayer = game.Players.First();
                game.CurrentPhase = TurnPhase.BeforeTurnStart;

                while (true)
                {
                    game.Advance();
                }
            }
        }

        public RoleGame()
        {
        }

        protected override void InitTriggers()
        {
            RegisterTrigger(GameEvent.GameStart, new RoleGameRuleTrigger());
            RegisterTrigger(GameEvent.PhaseProceedEvents[TurnPhase.Playing], new PlayerActionTrigger());
            HuoGong hg = new HuoGong();
            CardHandlers.Add(hg.CardType, hg.Verifier);
        }
    }

    
}
