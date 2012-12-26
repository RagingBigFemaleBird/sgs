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
using Sanguosha.Core.Heroes;
using System.Threading;
using Sanguosha.Core.Games;

namespace Sanguosha.Core.Games
{
    public class RoleGame : Game
    {
        public int NumberOfRebels { get; set; }
        public int NumberOfDefectors { get; set; }

        public class PlayerActionTrigger : Trigger
        {
            private class PlayerActionStageVerifier : CardUsageVerifier
            {
                public override UiHelper Helper { get { return new UiHelper() { IsActionStage = true }; } }
                public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
                {
                    if (!Game.CurrentGame.AllAlive(players))
                    {
                        return VerifierResult.Fail;
                    }
                    if ((cards == null || cards.Count == 0) && skill == null)
                    {
                        return VerifierResult.Fail;
                    }
                    if (skill is CheatSkill)
                    {
                        if (!Game.CurrentGame.Options.CheatingEnabled) return VerifierResult.Fail;
                        return VerifierResult.Success;
                    }
                    else if (skill is ActiveSkill)
                    {
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
                    if (cards[0].Place.DeckType != DeckType.Hand)
                    {
                        return VerifierResult.Fail;
                    }
                    return cards[0].Type.Verify(Game.CurrentGame.CurrentPlayer, skill, cards, players);
                }


                public override IList<CardHandler> AcceptableCardTypes
                {
                    get { return null; }
                }
            }

            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Player currentPlayer = Game.CurrentGame.CurrentPlayer;
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
                        if (skill is CheatSkill)
                        {
                            if (!Game.CurrentGame.Options.CheatingEnabled) break;
                            CheatSkill cs = skill as CheatSkill;
                            if (cs.CheatType == CheatType.Card)
                            {
                                if (Game.CurrentGame.IsClient)
                                {
                                    Game.CurrentGame.SyncUnknownLocationCardAll(null);
                                }
                                else
                                {
                                    foreach (var searchCard in Game.CurrentGame.CardSet)
                                    {
                                        if (searchCard.Id == cs.CardId)
                                        {
                                            Game.CurrentGame.SyncUnknownLocationCardAll(searchCard);
                                            break;
                                        }
                                    }
                                }
                                foreach (var searchCard in Game.CurrentGame.CardSet)
                                {
                                    if (searchCard.Id == cs.CardId)
                                    {
                                        CardsMovement move = new CardsMovement();
                                        move.Cards = new List<Card>() { searchCard };
                                        move.To = new DeckPlace(Game.CurrentGame.CurrentPlayer, DeckType.Hand);
                                        move.Helper = new MovementHelper();
                                        Game.CurrentGame.MoveCards(move);
                                        break;
                                    }
                                }
                            }
                            else if (cs.CheatType == CheatType.Skill)
                            {
                                foreach (var hero in Game.CurrentGame.OriginalCardSet)
                                {
                                    bool found = false;
                                    if (hero.Type is HeroCardHandler)
                                    {
                                        foreach (var sk in (hero.Type as HeroCardHandler).Hero.Skills)
                                        {
                                            if (sk.GetType().Name == cs.SkillName)
                                            {
                                                Game.CurrentGame.PlayerAcquireSkill(currentPlayer, sk.Clone() as ISkill);
                                                found = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (found) break;
                                }
                            }
                            continue;
                        }
                        else if (skill is ActiveSkill)
                        {
                            GameEventArgs arg = new GameEventArgs();
                            arg.Source = Game.CurrentGame.CurrentPlayer;
                            arg.Targets = players;
                            arg.Cards = cards;
                            ((ActiveSkill)skill).NotifyAndCommit(arg);
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
                    Game.CurrentGame.NotificationProxy.NotifyActionComplete();
                }
            }
        }

        public class PlayerDealStageTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Player currentPlayer = Game.CurrentGame.CurrentPlayer;
                Trace.TraceInformation("Player {0} deal.", currentPlayer.Id);
                Game.CurrentGame.DrawCards(currentPlayer, 2 + currentPlayer[Player.DealAdjustment]);
            }
        }

        public class PlayerDiscardStageTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Player currentPlayer = Game.CurrentGame.CurrentPlayer;
                Trace.TraceInformation("Player {0} discard stage.", currentPlayer.Id);
                var args = new AdjustmentEventArgs();
                args.Source = eventArgs.Source;
                args.AdjustmentAmount = 0;
                Game.CurrentGame.Emit(GameEvent.PlayerHandCardCapacityAdjustment, args);
                Game.CurrentGame.ForcePlayerDiscard(currentPlayer, 
                    (p, d) => 
                    { 
                        int i = Game.CurrentGame.Decks[p, DeckType.Hand].Count - p.Health - args.AdjustmentAmount;
                        if (i < 0) i = 0;
                        return i;
                    }, 
                    false);
            }

        }

        public class PlayerJudgeStageTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Player currentPlayer = Game.CurrentGame.CurrentPlayer;
                Trace.TraceInformation("Player {0} judge.", currentPlayer.Id);
                while (Game.CurrentGame.Decks[currentPlayer, DeckType.DelayedTools].Count > 0)
                {
                    Card card = Game.CurrentGame.Decks[currentPlayer, DeckType.DelayedTools].Last();
                    if (CardCategoryManager.IsCardCategory(card.Type.Category, CardCategory.DelayedTool))
                    {
                        DelayedTool tool = card.Type as DelayedTool;
                        tool.Activate(currentPlayer, card);
                    }
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// CardHandler and Damage eventArg format:
        /// Source : source of action/damage
        /// Targets: targets of action/damage
        /// Card:    the card (Card or CompositeCard) that produced the action / caused the damage
        /// Cards (damage only): the actual card(s), i.e. Card / subCards of CompositeCard
        /// ReadonlyCard: A readonly version (Cannot be moved, cannot set anything else other than its attributes) of the Card
        /// Note to developers:
        ///     In case you want to add properties to the card, you must add attributes to ReadonlyCard,
        ///     because the actual card (Card / Cards) can be moved to any place (therefore has its face type changed) during
        ///     multi-target computation.
        /// </remarks>
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
                        throw new TriggerResultException(TriggerResult.Retry);
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
                    c.Type.Process(eventArgs.Source, eventArgs.Targets, c, null);
                    return;
                }

                computeBackup = new List<Card>(Game.CurrentGame.Decks[DeckType.Compute]);
                Game.CurrentGame.Decks[DeckType.Compute].Clear();
                CardsMovement m = new CardsMovement();
                if (c is CompositeCard)
                {
                    m.Cards = new List<Card>(((CompositeCard)c).Subcards);
                }
                else
                {
                    m.Cards = new List<Card>(eventArgs.Cards);
                }
                m.To = new DeckPlace(null, DeckType.Compute);
                m.Helper = new MovementHelper();
                Player isDoingAFavor = eventArgs.Source;
                foreach (var checkFavor in m.Cards)
                {
                    if (checkFavor.Owner != eventArgs.Source)
                    {
                        Trace.TraceInformation("Acting on behalf of others");
                        isDoingAFavor = checkFavor.Owner;
                    }
                }
                bool runTrigger = !c.Type.IsReforging(eventArgs.Source, eventArgs.Skill, m.Cards, eventArgs.Targets);
                c.Type.TagAndNotify(eventArgs.Source, eventArgs.Targets, c);
                Game.CurrentGame.MoveCards(m);
                if (isDoingAFavor != eventArgs.Source)
                {
                    Game.CurrentGame.PlayerLostCard(isDoingAFavor, eventArgs.Cards);
                    Game.CurrentGame.PlayerPlayedCard(isDoingAFavor, eventArgs.Card);
                }
                else
                {
                    Game.CurrentGame.PlayerLostCard(eventArgs.Source, eventArgs.Cards);
                }
                Player savedSource = eventArgs.Source;

                GameEventArgs arg = new GameEventArgs();
                arg.Source = eventArgs.Source;
                arg.Targets = c.Type.ActualTargets(arg.Source, eventArgs.Targets, c);
                arg.Card = c;
                arg.ReadonlyCard = new ReadOnlyCard(c);
                if (runTrigger)
                {
                    try
                    {
                        Game.CurrentGame.Emit(GameEvent.PlayerUsedCard, arg);
                        Game.CurrentGame.Emit(GameEvent.CardUsageTargetConfirming, arg);
                        Game.CurrentGame.Emit(GameEvent.CardUsageTargetConfirmed, arg);
                    }
                    catch (TriggerResultException)
                    {
                        throw new NotImplementedException();
                    }
                }


                c.Type.Process(arg.Source, arg.Targets, c, arg.ReadonlyCard);

                if (Game.CurrentGame.Decks[DeckType.Compute].Count > 0)
                {
                    m.Cards = Game.CurrentGame.Decks[DeckType.Compute];
                    m.To = new DeckPlace(null, DeckType.Discard);
                    m.Helper = new MovementHelper();
                    Game.CurrentGame.PlayerAboutToDiscardCard(savedSource, m.Cards, DiscardReason.Use);
                    Game.CurrentGame.MoveCards(m);
                    Game.CurrentGame.PlayerDiscardedCard(savedSource, m.Cards, DiscardReason.Use);
                }
                Trace.Assert(Game.CurrentGame.Decks[DeckType.Compute].Count == 0);
                Game.CurrentGame.Decks[DeckType.Compute].AddRange(computeBackup);
            }
        }

        private static void StartGameDeal(Game game)
        {
            List<CardsMovement> moves = new List<CardsMovement>();
            // Deal everyone 4 cards
            foreach (Player player in game.Players)
            {
                CardsMovement move = new CardsMovement();
                move.Cards = new List<Card>();
                move.To = new DeckPlace(player, DeckType.Hand);
                for (int i = 0; i < 4; i++)
                {
                    game.SyncImmutableCard(player, game.PeekCard(0));
                    Card c = game.DrawCard();
                    move.Cards.Add(c);
                }
                moves.Add(move);
            }
            game.MoveCards(moves, null);
            int p = 0;
            foreach (Player player in game.Players)
            {
                game.PlayerAcquiredCard(player, moves[p].Cards);
                p++;
            }
        }

        public static DeckType RoleDeckType = new DeckType("Role");

        public class RoleGameRuleTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Game game = Game.CurrentGame;

                foreach (Player pp in game.Players)
                {
                    game.HandCardVisibility.Add(pp, new List<Player>() { pp });
                }

                int numberOfDefectors = 1;

                // Put the whole deck in the dealing deck
                game.Decks[DeckType.Dealing] = game.CardSet.GetRange(0, game.CardSet.Count);
                foreach (Card card in new List<Card>(game.Decks[DeckType.Dealing]))
                {
                    // We don't want hero cards
                    if (card.Type is HeroCardHandler)
                    {
                        game.Decks[DeckType.Dealing].Remove(card);
                        game.Decks[DeckType.Heroes].Add(card);
                        card.Place = new DeckPlace(null, DeckType.Heroes);
                    }
                    else
                    {
                        card.Place = new DeckPlace(null, DeckType.Dealing);
                    }
                }

                if (game.Players.Count == 0)
                {
                    return;
                }
                // Await role decision
                int seed = DateTime.Now.Millisecond;
                Trace.TraceError("Seed is {0}", seed);
                Random random = new Random(seed);
                int rulerId = 0;

                game.Decks[null, RoleDeckType].Add(new Card(SuitType.None, 0, new RoleCardHandler(Role.Ruler)));
                Trace.Assert(game.Players.Count > 1);
                Trace.Assert(numberOfDefectors + 1 <= game.Players.Count);
                int t = numberOfDefectors;
                while (t-- > 0)
                {
                    game.Decks[null, RoleDeckType].Add(new Card(SuitType.None, 0, new RoleCardHandler(Role.Defector)));
                }
                int remaining = game.Players.Count - numberOfDefectors;
                int rebel;
                int loyalist;
                if (remaining <= 1)
                {
                    rebel = 0;
                    loyalist = 0;
                }
                else
                {
                    rebel = (int)Math.Ceiling(((double)remaining) / 2);
                    loyalist = remaining - rebel - 1;
                }

                Trace.Assert(rebel + loyalist + numberOfDefectors + 1 == game.Players.Count);
                (game as RoleGame).NumberOfDefectors = numberOfDefectors;
                (game as RoleGame).NumberOfRebels = rebel;

                while (rebel-- > 0)
                {
                    game.Decks[null, RoleDeckType].Add(new Card(SuitType.None, 0, new RoleCardHandler(Role.Rebel)));
                }
                while (loyalist-- > 0)
                {
                    game.Decks[null, RoleDeckType].Add(new Card(SuitType.None, 0, new RoleCardHandler(Role.Loyalist)));
                }


                foreach (Card c in game.Decks[null, RoleDeckType])
                {
                    c.Place = new DeckPlace(null, RoleDeckType);
                    c.Id = GameEngine.CardSet.Count;
                    GameEngine.CardSet.Add(c);
                }

                Shuffle(game.Decks[null, RoleDeckType]);

                if (game.IsClient)
                {
                    int count = game.Decks[null, RoleDeckType].Count;
                    game.Decks[null, RoleDeckType].Clear();
                    while (count-- > 0)
                    {
                        Card c = new Card(SuitType.None, 0, new UnknownRoleCardHandler());
                        c.Id = Card.UnknownRoleId;
                        c.Place = new DeckPlace(null, RoleDeckType);
                        game.Decks[null, RoleDeckType].Add(c);
                    }
                    game.SyncImmutableCardAll(game.Decks[null, RoleDeckType][0]);
                }
                else
                {
                    foreach (Card c in game.Decks[null, RoleDeckType])
                    {
                        if ((c.Type as RoleCardHandler).Role == Role.Ruler)
                        {
                            game.SyncImmutableCardAll(c);
                        }
                    }
                }

                int i = 0;
                for (i = 0; i < game.Players.Count; i++)
                {
                    game.SyncImmutableCard(game.Players[i], game.Decks[null, RoleDeckType][i]);
                }

                List<CardsMovement> moves = new List<CardsMovement>();
                i = 0;
                foreach (Player p in game.Players)
                {
                    CardsMovement move = new CardsMovement();
                    move.Cards = new List<Card>() { game.Decks[null, RoleDeckType][i] };
                    move.To = new DeckPlace(p, RoleDeckType);
                    moves.Add(move);
                    i++;
                }
                game.MoveCards(moves, null);

                Thread.Sleep(1200);

                i = 0;
                foreach (Player player in game.Players)
                {
                    Card card = game.Decks[player, RoleDeckType][0];
                    var role = card.Type as RoleCardHandler;
                    if (role != null)
                    {
                        if (role.Role == Role.Ruler)
                        {
                            rulerId = i;
                        }
                        player.Role = role.Role;
                    }
                    i++;
                }
                
                //hero allocation
                Shuffle(game.Decks[DeckType.Heroes]);
                if (!game.IsClient)
                {
                    foreach (var hero in new List<Card>(game.Decks[DeckType.Heroes]))
                    {
                        foreach (var s in (hero.Type as HeroCardHandler).Hero.Skills)
                        {
                            if (s.IsRulerOnly)
                            {
                                game.Decks[DeckType.Heroes].Remove(hero);
                                game.Decks[DeckType.Heroes].Insert(0, hero);
                            }
                        }
                    }
                }
                List<Card> rulerDraw = new List<Card>();
                for (int rc = 0; rc < 12; rc++)
                {
                    rulerDraw.Add(game.Decks[DeckType.Heroes][rc]);
                }
                game.SyncCards(game.Players[rulerId], rulerDraw);
                DeckType tempHero = new DeckType("TempHero");
                game.Decks[null, tempHero].AddRange(rulerDraw);
                Trace.TraceInformation("Ruler is {0}", rulerId);
                game.Players[rulerId].Role = Role.Ruler;
                List<DeckPlace> sourceDecks = new List<DeckPlace>();
                sourceDecks.Add(new DeckPlace(null, tempHero));
                List<string> resultDeckNames = new List<string>();
                resultDeckNames.Add("HeroChoice");
                List<int> resultDeckMaximums = new List<int>();
                resultDeckMaximums.Add(1);
                List<List<Card>> answer;
                if (!game.UiProxies[game.Players[rulerId]].AskForCardChoice(new CardChoicePrompt("RulerHeroChoice"), sourceDecks, resultDeckNames, resultDeckMaximums, new AlwaysTrueChoiceVerifier(), out answer))
                {
                    answer = new List<List<Card>>();
                    answer.Add(new List<Card>());
                    answer[0].Add(game.Decks[DeckType.Heroes][0]);
                }
                game.SyncImmutableCardAll(answer[0][0]);
                game.Decks[DeckType.Heroes].Remove(answer[0][0]);

                HeroCardHandler h = (HeroCardHandler)answer[0][0].Type;
                Trace.TraceInformation("Assign {0} to player {1}", h.Hero.Name, rulerId);
                Game.CurrentGame.Players[rulerId].Hero = h.Hero;
                Game.CurrentGame.Players[rulerId].Allegiance = h.Hero.Allegiance;
                Game.CurrentGame.Players[rulerId].MaxHealth = Game.CurrentGame.Players[rulerId].Health = ((game.Players.Count > 4) ? h.Hero.MaxHealth + 1 : h.Hero.MaxHealth);
                Game.CurrentGame.Players[rulerId].IsMale = h.Hero.IsMale ? true : false;
                Game.CurrentGame.Players[rulerId].IsFemale = h.Hero.IsMale ? false : true;


                Shuffle(game.Decks[DeckType.Heroes]);
                Dictionary<Player, List<Card>> restDraw = new Dictionary<Player, List<Card>>();
                List<Player> players = new List<Player>(game.Players);
                players.Remove(game.Players[rulerId]);
                int idx = 0;
                foreach (Player p in players)
                {
                    restDraw.Add(p, new List<Card>());
                    for (int n = 0; n < 3; n++)
                    {
                        game.SyncImmutableCard(p, game.Decks[DeckType.Heroes][idx]);
                        restDraw[p].Add(game.Decks[DeckType.Heroes][idx]);
                        idx++;
                    }
                }

                var heroSelection = new Dictionary<Player, Card>();
                game.GlobalProxy.AskForHeroChoice(restDraw, heroSelection);

                bool notUsed = true;
                game.SyncConfirmationStatus(ref notUsed);

                List<Card> toRemove = new List<Card>();
                foreach (Player p in players)
                {
                    Card c;
                    //only server has the result
                    if (!game.IsClient)
                    {
                        idx = 0;
                        if (heroSelection.ContainsKey(p))
                        {
                            c = heroSelection[p];
                            idx = restDraw[p].IndexOf(c);
                        }
                        else
                        {
                            c = restDraw[p][0];
                        }
                        foreach (Player player in game.Players)
                        {
                            game.GameServer.SendObject(player.Id, idx);
                        }
                    }
                    // you are client
                    else
                    {
                        idx = (int)game.GameClient.Receive();
                        c = restDraw[p][idx];
                    }
                    game.SyncImmutableCardAll(c);
                    toRemove.Add(c);
                    h = (HeroCardHandler)c.Type;
                    Trace.TraceInformation("Assign {0} to player {1}", h.Hero.Name, p.Id);
                    var hero = h.Hero.Clone() as Hero;
                    foreach (var skill in new List<ISkill>(hero.Skills))
                    {
                        if (skill.IsRulerOnly)
                        {
                            hero.Skills.Remove(skill);
                        }
                    }
                    p.Hero = hero;
                    p.Allegiance = hero.Allegiance;
                    p.MaxHealth = p.Health = hero.MaxHealth;
                    p.IsMale = hero.IsMale ? true : false;
                    p.IsFemale = hero.IsMale ? false : true;
                    
                }

                foreach (var card in toRemove)
                {
                    game.Decks[DeckType.Heroes].Remove(card);
                }

                foreach (var pxy in game.UiProxies)
                {
                    pxy.Value.Freeze();
                }
                var toCheck = game.AlivePlayers;
                game.SortByOrderOfComputation(game.Players[rulerId], toCheck);
                foreach (var p in toCheck)
                {
                    Game.CurrentGame.HandleGodHero(p);
                }

                Shuffle(game.Decks[null, DeckType.Dealing]);

                Player current = game.CurrentPlayer = game.Players[rulerId];

                StartGameDeal(game);
                
                foreach (var act in game.AlivePlayers)
                {
                    game.Emit(GameEvent.PlayerGameStartAction, new GameEventArgs() { Source = act });
                }

                while (true)
                {
                    GameEventArgs args = new GameEventArgs();
                    args.Source = current;
                    game.CurrentPhaseEventIndex = 0;
                    game.CurrentPhase = TurnPhase.BeforeStart;
                    game.CurrentPlayer = current;
                    game.Emit(GameEvent.DoPlayer, args);
                    current = game.NextAlivePlayer(current);
                }
            }
        }

        public class DoPlayerTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Game game = Game.CurrentGame;
                Player currentPlayer = eventArgs.Source;
                if (currentPlayer.IsImprisoned)
                {
                    currentPlayer.IsImprisoned = false;
                    return;
                }
                game.CurrentPlayer = currentPlayer;
                Game.CurrentGame.Emit(GameEvent.PhaseBeforeStart, new GameEventArgs() { Source = currentPlayer });
                while (true)
                {
                    GameEventArgs args = new GameEventArgs() { Source = currentPlayer };
                    Trace.TraceInformation("Main game loop running {0}:{1}", currentPlayer.Id, game.CurrentPhase);
                    try
                    {
                        var phaseEvent = Game.PhaseEvents[game.CurrentPhaseEventIndex];
                        if (phaseEvent.ContainsKey(game.CurrentPhase))
                        {
                            game.Emit(Game.PhaseEvents[game.CurrentPhaseEventIndex][game.CurrentPhase], args);
                        }
                    }
                    catch (TriggerResultException e)
                    {
                        if (e.Status == TriggerResult.End)
                        {
                        }
                    }

                    game.CurrentPhaseEventIndex++;
                    if (game.CurrentPhaseEventIndex >= Game.PhaseEvents.Length)
                    {
                        game.CurrentPhaseEventIndex = 0;
                        game.CurrentPhase++;
                        if ((int)game.CurrentPhase >= Enum.GetValues(typeof(TurnPhase)).Length || (int)game.CurrentPhase < 0)
                        {
                            break;
                        }
                    }
                }
                Game.CurrentGame.Emit(GameEvent.PhasePostEnd, new GameEventArgs() { Source = currentPlayer });
            }
        }


        public RoleGame(int numberOfDefectors)
        {
            Trace.Assert(numberOfDefectors <= 2 && numberOfDefectors >= 0);
            defectorsCount = numberOfDefectors;
        }


        int defectorsCount;

        public static void Shuffle(IList<Card> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Card value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private class PlayerIsDead : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Player p = eventArgs.Targets[0];
                Player source = eventArgs.Source;
                if (source == null)
                {
                    Trace.TraceInformation("Player {0} killed", p.Id);
                }
                else
                {
                    Trace.TraceInformation("Player {0} killed by Player {1}", p.Id, source.Id);
                }
                DeckType role = new DeckType("Role");
                Trace.Assert(Game.CurrentGame.Decks[p, role].Count == 1);
                Game.CurrentGame.SyncImmutableCardAll(Game.CurrentGame.Decks[p, role][0]);
                Trace.TraceInformation("Player {0} is {1}", p.Id, (Game.CurrentGame.Decks[p, role][0].Type as RoleCardHandler).Role);
                p.Role = (Game.CurrentGame.Decks[p, role][0].Type as RoleCardHandler).Role;
                Game.CurrentGame.NotificationProxy.NotifyDeath(p, source);

                if (p.Role == Role.Ruler)
                {
                    Trace.TraceInformation("Ruler dead. Game over");
                    if (Game.CurrentGame.AlivePlayers.Count == 2)
                    {
                        Game.CurrentGame.NotificationProxy.NotifyGameOver(GameResult.Defector);
                    }
                    else
                    {
                        Game.CurrentGame.NotificationProxy.NotifyGameOver(GameResult.Rebel);
                    }
                    throw new GameOverException();
                }

                Game.CurrentGame.Emit(GameEvent.PlayerIsDead, eventArgs);
                p.IsDead = true;
                if (p.Hero != null)
                {
                    foreach (ISkill s in p.Hero.Skills)
                    {
                        if (s is TriggerSkill)
                        {
                            (s as TriggerSkill).Owner = null;
                        }
                    }
                }
                if (p.Hero2 != null)
                {
                    foreach (ISkill s in p.Hero2.Skills)
                    {
                        if (s is TriggerSkill)
                        {
                            (s as TriggerSkill).Owner = null;
                        }
                    }
                }
                
                if (p.Role == Role.Rebel || p.Role == Role.Defector)
                {
                    int deadRebel = 0;
                    int deadDefector = 0;
                    foreach (Player z in Game.CurrentGame.Players)
                    {
                        if (z.Role == Role.Rebel && z.IsDead)
                        {
                            deadRebel++;
                        }
                        if (z.Role == Role.Defector && z.IsDead)
                        {
                            deadDefector++;
                        }
                    }
                    Trace.TraceInformation("Deathtoll: Rebel {0}/{1}, Defector {2}/{3}", deadRebel, (Game.CurrentGame as RoleGame).NumberOfRebels, deadDefector, (Game.CurrentGame as RoleGame).NumberOfDefectors);
                    if (deadRebel == (Game.CurrentGame as RoleGame).NumberOfRebels && deadDefector == (Game.CurrentGame as RoleGame).NumberOfDefectors)
                    {
                        Trace.TraceInformation("Ruler wins.");
                        Game.CurrentGame.NotificationProxy.NotifyGameOver(GameResult.Ruler);
                        throw new GameOverException();
                    }
                    if (source != null && !source.IsDead && p.Role == Role.Rebel)
                    {
                        Trace.TraceInformation("Killed rebel. GIVING YOU THREE CARDS OMG WIN GAME RIGHT THERE!!!");
                        Game.CurrentGame.DrawCards(source, 3);
                    }
                }
                if (p.Role == Role.Loyalist && source.Role == Role.Ruler)
                {
                    Trace.TraceInformation("Loyalist killl by ruler. GG");
                    Game.CurrentGame.SyncCardsAll(Game.CurrentGame.Decks[source, DeckType.Hand]);
                    CardsMovement move = new CardsMovement();
                    move.Cards = new List<Card>();
                    foreach (Card c in Game.CurrentGame.Decks[source, DeckType.Hand])
                    {
                        if (Game.CurrentGame.PlayerCanDiscardCard(source, c))
                        {
                            move.Cards.Add(c);
                        }
                    }
                    move.Cards.AddRange(Game.CurrentGame.Decks[source, DeckType.Equipment]);
                    move.Cards.AddRange(Game.CurrentGame.Decks[source, DeckType.DelayedTools]);
                    move.To = new DeckPlace(null, DeckType.Discard);
                    move.Helper = new MovementHelper();
                    Game.CurrentGame.MoveCards(move);
                }
            }
        }

        private class DeadManStopper : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Trace.Assert(eventArgs.Targets.Count == 1);
                if (eventArgs.Targets[0].IsDead)
                {
                    Trace.TraceInformation("RIP {0}", eventArgs.Targets[0].Id);
                    throw new TriggerResultException(TriggerResult.Fail);
                }
            }
        }

        private class ShuffleTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                RoleGame.Shuffle(Game.CurrentGame.Decks[null, DeckType.Discard]);
                Game.CurrentGame.Decks[null, DeckType.Dealing].AddRange(Game.CurrentGame.Decks[null, DeckType.Discard]);
            }
        }

        protected override void InitTriggers()
        {
            RegisterTrigger(GameEvent.DoPlayer, new DoPlayerTrigger());
            RegisterTrigger(GameEvent.Shuffle, new ShuffleTrigger());
            RegisterTrigger(GameEvent.GameStart, new RoleGameRuleTrigger());
            RegisterTrigger(GameEvent.PhaseProceedEvents[TurnPhase.Judge], new PlayerJudgeStageTrigger());
            RegisterTrigger(GameEvent.PhaseProceedEvents[TurnPhase.Play], new PlayerActionTrigger());
            RegisterTrigger(GameEvent.PhaseProceedEvents[TurnPhase.Draw], new PlayerDealStageTrigger() { Priority = -1 });
            RegisterTrigger(GameEvent.PhaseProceedEvents[TurnPhase.Discard], new PlayerDiscardStageTrigger() { Priority = -1 });
            RegisterTrigger(GameEvent.CommitActionToTargets, new CommitActionToTargetsTrigger());
            RegisterTrigger(GameEvent.AfterHealthChanged, new PlayerHpChanged());
            RegisterTrigger(GameEvent.GameProcessPlayerIsDead, new PlayerIsDead() { Priority = int.MinValue });
            RegisterTrigger(GameEvent.CardUsageBeforeEffected, new DeadManStopper() { Priority = int.MaxValue });
            RegisterTrigger(GameEvent.CardUsageBeforeEffected, new DeadManStopper() { Priority = int.MinValue });
        }
    }

}
