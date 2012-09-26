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
using Sanguosha.Core.Exceptions;

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
                    IUiProxy proxy = Game.CurrentGame.UiProxies[currentPlayer];
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
                        try
                        {
                            Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, new Triggers.GameEventArgs() { Skill = skill, Source = Game.CurrentGame.CurrentPlayer, Targets = players, Cards = cards });
                        }
                        catch (TriggerResultException)
                        {
                        }
                    }
                }
            }
        }

        public class CommitActionToTargetsTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                List<Card> computeBackup;
                ICard c;
                if (eventArgs.Skill != null)
                {
                    c = eventArgs.Cards[0];
                    throw new NotImplementedException();
                }
                else
                {
                    c = eventArgs.Cards[0];
                }
                computeBackup = new List<Card>(Game.CurrentGame.Decks[DeckType.Compute]);
                Game.CurrentGame.Decks[DeckType.Compute].Clear();
                CardsMovement m;
                m.cards = eventArgs.Cards;
                m.to = new DeckPlace(null, DeckType.Compute);
                Game.CurrentGame.MoveCards(m);
                
                Game.CurrentGame.CardHandlers[c.Type].Process(eventArgs.Source, eventArgs.Targets);

                m.cards = Game.CurrentGame.Decks[DeckType.Compute];
                m.to = new DeckPlace(null, DeckType.Discard);
                Game.CurrentGame.MoveCards(m);
                Trace.Assert(Game.CurrentGame.Decks[DeckType.Compute].Count == 0);
                Game.CurrentGame.Decks[DeckType.Compute] = new List<Card>(computeBackup);
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
                    game.DrawCards(player, 2);
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
            RegisterTrigger(GameEvent.CommitActionToTargets, new CommitActionToTargetsTrigger());
            HuoGong hg = new HuoGong();
            CardHandlers.Add(hg.CardType, hg);
        }
    }

    
}
