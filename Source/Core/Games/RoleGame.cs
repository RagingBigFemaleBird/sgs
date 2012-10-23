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
            private class PlayerActionStageVerifier : CardUsageVerifier
            {
                public override VerifierResult FastVerify(ISkill skill, List<Card> cards, List<Player> players)
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
                        VerifierResult ret = s.TryTransform(cards, null, out result);
                        if (ret == VerifierResult.Success)
                        {
                            return result.Type.Verify(Game.CurrentGame.CurrentPlayer, skill, cards, players);
                        }
                        if (ret == VerifierResult.Partial && players != null && players.Count != 0)
                        {
                            return VerifierResult.Fail;
                        }
                        return ret;
                    }
                    else if (skill != null)
                    {
                        return VerifierResult.Fail;
                    }
                    return cards[0].Type.Verify(Game.CurrentGame.CurrentPlayer, skill, cards, players);
                }


                public override IList<CardHandler> AcceptableCardType
                {
                    get { return null; }
                }
            }

            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Player currentPlayer = eventArgs.Game.CurrentPlayer;
                Trace.TraceInformation("Player {0} action.", currentPlayer.Id);
                while (true)
                {
                    Trace.Assert(Game.CurrentGame.UiProxies.ContainsKey(currentPlayer));
                    IUiProxy proxy = Game.CurrentGame.UiProxies[currentPlayer];
                    ISkill skill;
                    List<Card> cards;
                    List<Player> players;
                    PlayerActionStageVerifier v = new PlayerActionStageVerifier();
                    if (!proxy.AskForCardUsage(new Prompt(Prompt.PlayingPhasePrompt), v, out skill, out cards, out players))
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

        public class PlayerDealStageTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Player currentPlayer = eventArgs.Game.CurrentPlayer;
                Trace.TraceInformation("Player {0} deal.", currentPlayer.Id);
                Game.CurrentGame.DrawCards(currentPlayer, 2);
            }
        }

        public class PlayerDiscardStageTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Player currentPlayer = eventArgs.Game.CurrentPlayer;
                Trace.TraceInformation("Player {0} discard.", currentPlayer.Id);
                int cannotBeDiscarded = 0;
                // Have we finished discarding everything?
                // We finish if 
                //      玩家手牌数 小于等于 玩家体力值
                //  或者玩家手牌数 小于等于 不可弃的牌的数目
                while (true)
                {
                    int handCardCount = Game.CurrentGame.Decks[currentPlayer, DeckType.Hand].Count; // 玩家手牌数                    
                    int cardKept = Math.Max(cannotBeDiscarded, currentPlayer.Health);
                    if (handCardCount <= cardKept)
                    {
                        break;
                    }
                    Trace.Assert(Game.CurrentGame.UiProxies.ContainsKey(currentPlayer));
                    IUiProxy proxy = Game.CurrentGame.UiProxies[currentPlayer];
                    ISkill skill;
                    List<Card> cards;
                    List<Player> players;
                    PlayerDiscardStageVerifier v = new PlayerDiscardStageVerifier();
                    v.Owner = currentPlayer;
                    cannotBeDiscarded = 0;
                    foreach (Card c in Game.CurrentGame.Decks[currentPlayer, DeckType.Hand])
                    {
                        if (!Game.CurrentGame.PlayerCanDiscardCard(currentPlayer, c))
                        {
                            cannotBeDiscarded++;
                        }
                    }
                    //如果玩家体力 小于 不可弃的牌数 则 摊牌
                    bool status = currentPlayer.Health >= cannotBeDiscarded;
                    Game.CurrentGame.SyncConfirmationStatus(ref status);
                    if (!status)
                    {
                        Game.CurrentGame.SyncCardsAll(Game.CurrentGame.Decks[currentPlayer, DeckType.Hand]);
                    }

                    int promptCount = handCardCount - currentPlayer.Health;
                    if (!proxy.AskForCardUsage(new Prompt(Prompt.DiscardPhasePrompt, promptCount),
                                               v, out skill, out cards, out players))
                    {
                        //玩家没有回应(default)
                        //如果玩家有不可弃掉的牌(这个只有服务器知道） 则通知所有客户端该玩家手牌
                        status = (cannotBeDiscarded == 0);
                        Game.CurrentGame.SyncConfirmationStatus(ref status);
                        if (!status)
                        {
                            Game.CurrentGame.SyncCardsAll(Game.CurrentGame.Decks[currentPlayer, DeckType.Hand]);
                        }
                        cannotBeDiscarded = 0;
                        foreach (Card c in Game.CurrentGame.Decks[currentPlayer, DeckType.Hand])
                        {
                            if (!Game.CurrentGame.PlayerCanDiscardCard(currentPlayer, c))
                            {
                                cannotBeDiscarded++;
                            }
                        }

                        Trace.TraceInformation("Invalid answer, choosing for you");
                        cards = new List<Card>();
                        int cardsDiscarded = 0;
                        foreach (Card c in Game.CurrentGame.Decks[currentPlayer, DeckType.Hand])
                        {
                            if (Game.CurrentGame.PlayerCanDiscardCard(currentPlayer, c))
                            {
                                cards.Add(c);
                                cardsDiscarded++;
                            }
                            int cardsRemaining = Game.CurrentGame.Decks[currentPlayer, DeckType.Hand].Count - cardsDiscarded;
                            if (cardsRemaining <= Math.Max(currentPlayer.Health, cannotBeDiscarded))
                            {
                                break;
                            }
                        }
                    }
                    Game.CurrentGame.HandleCardDiscard(currentPlayer, cards);
                }
            }

            private class PlayerDiscardStageVerifier : ICardUsageVerifier
            {
                public Player Owner { get; set; }

                public VerifierResult FastVerify(ISkill skill, List<Card> cards, List<Player> players)
                {
                    if (skill != null)
                    {
                        return VerifierResult.Fail;
                    }
                    if (players != null && players.Count > 0)
                    {
                        return VerifierResult.Fail;
                    }
                    if (cards == null || cards.Count == 0)
                    {
                        return VerifierResult.Partial;
                    }
                    foreach (Card c in cards)
                    {
                        if (!Game.CurrentGame.PlayerCanDiscardCard(Owner, c))
                        {
                            return VerifierResult.Fail;
                        }
                    }
                    int cannotBeDiscarded = 0;
                    foreach (Card c in Game.CurrentGame.Decks[Owner, DeckType.Hand])
                    {
                        if (!Game.CurrentGame.PlayerCanDiscardCard(Owner, c))
                        {
                            cannotBeDiscarded++;
                        }
                    }
                    int remainingCards = (Owner.Health > cannotBeDiscarded) ? (Owner.Health) : cannotBeDiscarded;
                    if (Game.CurrentGame.Decks[Owner, DeckType.Hand].Count - cards.Count < remainingCards)
                    {
                        return VerifierResult.Fail;
                    }
                    return VerifierResult.Success;
                }

                public IList<CardHandler> AcceptableCardType
                {
                    get { throw new NotImplementedException(); }
                }

                public VerifierResult Verify(ISkill skill, List<Card> cards, List<Player> players)
                {
                    return FastVerify(skill, cards, players);
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
                    if (!s.Transform(eventArgs.Cards, null, out card, eventArgs.Targets))
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

                // if it's delayed tool or equipment, we can't move it to compute area. call handlers directly
                if (CardCategoryManager.IsCardCategory(c.Type.Category, CardCategory.DelayedTool)
                    || CardCategoryManager.IsCardCategory(c.Type.Category, CardCategory.Equipment))
                {
                    c.Type.Process(eventArgs.Source, eventArgs.Targets, c);
                    return;
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
                bool runTrigger = c.Type.NotReforging(eventArgs.Source, eventArgs.Skill, m.cards, eventArgs.Targets);

                Game.CurrentGame.MoveCards(m, new CardUseLog() { Source = eventArgs.Source, Targets = eventArgs.Targets, Skill = eventArgs.Skill, Cards = eventArgs.Cards });
                if (runTrigger)
                {
                    try
                    {
                        GameEventArgs arg = new GameEventArgs();
                        arg.Source = eventArgs.Source;
                        arg.Targets = null;
                        arg.Card = c;

                        Game.CurrentGame.Emit(GameEvent.PlayerUsedCard, arg);
                    }
                    catch (TriggerResultException)
                    {
                        throw new NotImplementedException();
                    }
                }
                c.Type.Process(eventArgs.Source, eventArgs.Targets, c);

                m.cards = Game.CurrentGame.Decks[DeckType.Compute];
                m.to = new DeckPlace(null, DeckType.Discard);
                Game.CurrentGame.MoveCards(m, null);
                Trace.Assert(Game.CurrentGame.Decks[DeckType.Compute].Count == 0);
                Game.CurrentGame.Decks[DeckType.Compute] = new List<Card>(computeBackup);
            }
        }

        private static void StartGameDeal(Game game)
        {
            List<CardsMovement> moves = new List<CardsMovement>();
            // Deal everyone 4 cards
            foreach (Player player in game.Players)
            {
                CardsMovement move = new CardsMovement();
                move.cards = new List<Card>();
                move.to = new DeckPlace(player, DeckType.Hand);
                for (int i = 0; i < 4; i++)
                {
                    game.SyncCard(player, game.PeekCard(0));
                    Card c = game.DrawCard();
                    move.cards.Add(c);
                }
                moves.Add(move);
            }
            game.MoveCards(moves, null);
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

                StartGameDeal(game);
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
            RegisterTrigger(GameEvent.PhaseProceedEvents[TurnPhase.Draw], new PlayerDealStageTrigger());
            RegisterTrigger(GameEvent.PhaseProceedEvents[TurnPhase.Discard], new PlayerDiscardStageTrigger());
            RegisterTrigger(GameEvent.CommitActionToTargets, new CommitActionToTargetsTrigger());
        }
    }

}
