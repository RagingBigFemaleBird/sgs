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
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Core.Games
{
    public class RoleGame : Game
    {
        public class PlayerActionTrigger : Trigger
        {
            private class PlayerActionStageVerifier : ICardUsageVerifier
            {
                public VerifierResult Verify(ISkill skill, List<Card> cards, List<Player> players)
                {
                    if ((cards == null || cards.Count == 0) && skill == null)
                    {
                        return VerifierResult.Fail;
                    }
                    if (skill is ActiveSkill)
                    {
                        if (Game.CurrentGame.CurrentPlayer.Hero.Skills.IndexOf(skill) < 0)
                        {
                            return VerifierResult.Fail;
                        }
                        GameEventArgs arg = new GameEventArgs();
                        arg.Source = Game.CurrentGame.CurrentPlayer;
                        arg.Targets = players;
                        arg.Cards = cards;
                        return ((ActiveSkill)skill).Validate(arg);
                    }
                    else if (skill is CardTransformSkill)
                    {
                        CardTransformSkill s = (CardTransformSkill)skill;
                        CompositeCard result;
                        if (s.TryTransform(cards, null, out result) == VerifierResult.Fail)
                        {
                            return VerifierResult.Fail;
                        }
                        else
                        {
                            return result.Type.Verify(Game.CurrentGame.CurrentPlayer, skill, cards, players);
                        }
                    }
                    return cards[0].Type.Verify(Game.CurrentGame.CurrentPlayer, skill, cards, players);
                }
            }
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Player currentPlayer = eventArgs.Game.CurrentPlayer;
                Trace.TraceInformation("Player {0} action.", currentPlayer.Id);
                while (true)
                {
                    IUiProxy proxy = Game.CurrentGame.UiProxies[currentPlayer];
                    ISkill skill;
                    List<Card> cards;
                    List<Player> players;
                    PlayerActionStageVerifier v = new PlayerActionStageVerifier();
                    if (!proxy.AskForCardUsage("Player Action Stage", v, out skill, out cards, out players))
                    {
                        break;
                    }
                    if (skill != null)
                    {
                        if (skill is ActiveSkill)
                        {
                            GameEventArgs arg = new GameEventArgs();
                            arg.Source = Game.CurrentGame.CurrentPlayer;
                            arg.Targets = players;
                            arg.Cards = cards;
                            ((ActiveSkill)skill).Commit(arg);
                            continue;
                        }
                        CompositeCard c;
                        CardTransformSkill s = (CardTransformSkill)skill;
                        VerifierResult r = s.TryTransform(cards, null, out c);
                        Trace.TraceInformation("Player used {0}", c.Type);
                    }
                    else
                    {
                        Trace.Assert(cards[0] != null && cards.Count == 1);
                        Trace.TraceInformation("Player used {0}", cards[0].Type);
                    }
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

        public class CommitActionToTargetsTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                List<Card> computeBackup;
                ICard c;
                if (eventArgs.Skill != null)
                {
                    CompositeCard card;
                    CardTransformSkill s = (CardTransformSkill)eventArgs.Skill;                    
                    if (!s.Transform(eventArgs.Cards, null, out card))
                    {
                        return;
                    }
                    c = card;
                }
                else
                {
                    Trace.Assert(eventArgs.Cards.Count == 1);
                    c = eventArgs.Cards[0];
                }
                computeBackup = new List<Card>(Game.CurrentGame.Decks[DeckType.Compute]);
                Game.CurrentGame.Decks[DeckType.Compute].Clear();
                CardsMovement m;
                if (c is CompositeCard)
                {
                    m.cards = new List<Card>(((CompositeCard)c).Subcards);
                }
                else
                {
                    m.cards = new List<Card>(eventArgs.Cards);
                }
                m.to = new DeckPlace(null, DeckType.Compute);

                // if it's delayed tool, we can't move it to compute area. call handlers directly
                if (CardCategoryManager.IsCardCategory(c.Type.Category, CardCategory.DelayedTool)
                    || CardCategoryManager.IsCardCategory(c.Type.Category, CardCategory.Armor))
                {
                    c.Type.Process(eventArgs.Source, eventArgs.Targets, c);
                    return;
                }


                Game.CurrentGame.MoveCards(m, new CardUseLog() { Source = eventArgs.Source, Targets = eventArgs.Targets, Skill = eventArgs.Skill, Cards = eventArgs.Cards });
                
                c.Type.Process(eventArgs.Source, eventArgs.Targets, c);

                m.cards = Game.CurrentGame.Decks[DeckType.Compute];
                m.to = new DeckPlace(null, DeckType.Discard);
                Game.CurrentGame.MoveCards(m, null);
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
                    game.DrawCards(player, 4);
                }
                game.CurrentPlayer = game.Players.First();
                game.CurrentPhase = TurnPhase.BeforeStart;

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
            RegisterTrigger(GameEvent.PhaseProceedEvents[TurnPhase.Play], new PlayerActionTrigger());
            RegisterTrigger(GameEvent.CommitActionToTargets, new CommitActionToTargetsTrigger());
        }
    }

}
