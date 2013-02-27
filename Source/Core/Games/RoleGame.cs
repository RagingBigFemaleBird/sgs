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
using Sanguosha.Core.Utils;

namespace Sanguosha.Core.Games
{
    public class RoleGame : Game
    {
        public int NumberOfRebels { get; set; }
        public int NumberOfDefectors { get; set; }

        public virtual int GetMaxHealth(Player p)
        {
            int maxHealth = 0;
            if (p.Role == Role.Ruler && Game.CurrentGame.Players.Count > 4) maxHealth += 1;
            if (Game.CurrentGame.Settings.DualHeroMode && p.Hero2 != null)
            {
                maxHealth += (p.Hero.MaxHealth + p.Hero2.MaxHealth) / 2;
            }
            else
            {
                maxHealth += p.Hero.MaxHealth;
            }
            return maxHealth;
        }

        public class PlayerActionTrigger : Trigger
        {
            private class PlayerActionStageVerifier : CardUsageVerifier
            {
                public PlayerActionStageVerifier()
                {
                    Helper.IsActionStage = true;
                }

                public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
                {
                    if (players != null && players.Any(p => p.IsDead))
                    {
                        return VerifierResult.Fail;
                    }
                    if ((cards == null || cards.Count == 0) && skill == null)
                    {
                        return VerifierResult.Fail;
                    }
                    if (skill is CheatSkill)
                    {
                        if (!Game.CurrentGame.Settings.CheatEnabled) return VerifierResult.Fail;
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
                while (!currentPlayer.IsDead)
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
                            if (!Game.CurrentGame.Settings.CheatEnabled) break;
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
                                                Game.CurrentGame.PlayerAcquireAdditionalSkill(currentPlayer, sk.Clone() as ISkill, currentPlayer.Hero);
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
                            Game.CurrentGame.NotificationProxy.NotifyActionComplete();
                            Game.CurrentGame.LastAction = skill;
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
                    Game.CurrentGame.LastAction = skill;
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
                        int i = Game.CurrentGame.Decks[p, DeckType.Hand].Count - Math.Max(0, p.Health) - args.AdjustmentAmount;
                        if (i < 0) i = 0;
                        return i;
                    },
                    false,
                    false);
            }

        }

        public class PlayerJudgeStageTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Player currentPlayer = Game.CurrentGame.CurrentPlayer;
                Trace.TraceInformation("Player {0} judge.", currentPlayer.Id);
                var save = new List<Card>(currentPlayer.DelayedTools());
                while (save.Count > 0)
                {
                    Card card = save.Last();
                    save.Remove(card);
                    if (!currentPlayer.DelayedTools().Contains(card))
                    {
                        continue;
                    }
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
                    if (eventArgs.Card is CompositeCard)
                    {
                        //仅在 Sha.UseDummyShaTo 里，eventArgs.Card才会被赋值且为CompositeCard
                        //表示 该DummySha的Verifier里被skill所转化
                        //参看 Sha.UseDummyShaTo
                        card = eventArgs.Card as CompositeCard;
                        s.NotifyAction(eventArgs.Source, eventArgs.Targets, card);
                    }
                    else if (!s.Transform(eventArgs.Cards, null, out card, eventArgs.Targets))
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

                c.Type.TagAndNotify(eventArgs.Source, eventArgs.Targets, c);

                // if it's delayed tool or equipment, we can't move it to compute area. call handlers directly
                if (CardCategoryManager.IsCardCategory(c.Type.Category, CardCategory.DelayedTool)
                    || CardCategoryManager.IsCardCategory(c.Type.Category, CardCategory.Equipment))
                {
                    c.Type.Process(new GameEventArgs() { Source = eventArgs.Source, Targets = eventArgs.Targets, Card = c });
                    return;
                }
                var rdonlyCard = new ReadOnlyCard(c);
                computeBackup = new List<Card>(Game.CurrentGame.Decks[DeckType.Compute]);
                Game.CurrentGame.Decks[DeckType.Compute].Clear();
                CardsMovement m = new CardsMovement();
                Player isDoingAFavor = eventArgs.Source;
                if (c is CompositeCard)
                {
                    m.Cards = new List<Card>(((CompositeCard)c).Subcards);
                    if (c.Owner != null && c.Owner != eventArgs.Source)
                    {
                        Trace.TraceInformation("Acting on behalf of others");
                        isDoingAFavor = c.Owner;
                    }
                }
                else
                {
                    m.Cards = new List<Card>(eventArgs.Cards);
                }
                m.To = new DeckPlace(null, DeckType.Compute);
                m.Helper = new MovementHelper();
                foreach (var checkFavor in m.Cards)
                {
                    if (checkFavor.Owner != null && checkFavor.Owner != eventArgs.Source)
                    {
                        Trace.TraceInformation("Acting on behalf of others");
                        isDoingAFavor = checkFavor.Owner;
                    }
                }
                bool runTrigger = !c.Type.IsReforging(eventArgs.Source, eventArgs.Skill, m.Cards, eventArgs.Targets);
                Game.CurrentGame.MoveCards(m, false, GameDelayTypes.PlayerAction);
                if (isDoingAFavor != eventArgs.Source)
                {
                    Game.CurrentGame.PlayerPlayedCard(isDoingAFavor, new List<Player>() { eventArgs.Source }, c);
                    Game.CurrentGame.PlayerLostCard(isDoingAFavor, eventArgs.Cards != null ? eventArgs.Cards : ((CompositeCard)c).Subcards);
                }
                else
                {
                    Game.CurrentGame.PlayerLostCard(eventArgs.Source, eventArgs.Cards);
                }
                Player savedSource = eventArgs.Source;

                GameEventArgs arg = new GameEventArgs();
                arg.Source = eventArgs.Source;
                arg.UiTargets = eventArgs.Targets;
                arg.Targets = c.Type.ActualTargets(arg.Source, eventArgs.Targets, c);
                arg.Card = c;
                arg.ReadonlyCard = rdonlyCard;
                arg.InResponseTo = eventArgs.InResponseTo;
                //we inherit card attributes if some readonly card is passed down from the caller (解烦)
                if (eventArgs.ReadonlyCard != null)
                {
                    arg.ReadonlyCard.Attributes.Clear();
                    foreach (var pair in eventArgs.ReadonlyCard.Attributes)
                    {
                        arg.ReadonlyCard.Attributes.Add(pair.Key, pair.Value);
                    }
                }
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

                c.Type.Process(arg);

                if (runTrigger)
                {
                    try
                    {
                        Game.CurrentGame.Emit(GameEvent.CardUsageDone, arg);
                    }
                    catch (TriggerResultException)
                    {
                        throw new NotImplementedException();
                    }
                }

                if (Game.CurrentGame.Decks[DeckType.Compute].Count > 0)
                {
                    m.Cards = Game.CurrentGame.Decks[DeckType.Compute];
                    m.To = new DeckPlace(null, DeckType.Discard);
                    m.Helper = new MovementHelper();
                    Game.CurrentGame.PlayerAboutToDiscardCard(savedSource, m.Cards, DiscardReason.Use);
                    Game.CurrentGame.MoveCards(m, false, GameDelayTypes.None);
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
            foreach (Player player in game.AlivePlayers)
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
            game.MoveCards(moves, null, GameDelayTypes.GameBeforeStart);
        }

        public static DeckType RoleDeckType = new DeckType("Role");


        public class RoleGameRuleTrigger : Trigger
        {
            List<Card> usedRoleCards;
            static List<Card> allRoleCards;

            private Card _FindARoleCard(Role role)
            {
                foreach (var card in allRoleCards)
                {
                    if ((card.Type as RoleCardHandler).Role == role && !usedRoleCards.Contains(card))
                    {
                        var c = new Card();
                        c.CopyFrom(card);
                        c.Place = new DeckPlace(null, RoleGame.RoleDeckType);
                        usedRoleCards.Add(card);
                        return c;
                    }
                }
                return null;
            }

            static RoleGameRuleTrigger()
            {
                allRoleCards = new List<Card>(from c in GameEngine.CardSet
                                              where c.Type is RoleCardHandler
                                              select c);
            }

            public RoleGameRuleTrigger()
            {
                usedRoleCards = new List<Card>();
            }

            private void _DebugDealingDeck(Game game)
            {
                if (game.Decks[null, DeckType.Dealing].Any(tc => tc.Type is HeroCardHandler || tc.Type is RoleCardHandler || tc.Id == Card.UnknownHeroId || tc.Id == Card.UnknownRoleId))
                {
                    var card = game.Decks[null, DeckType.Dealing].FirstOrDefault(tc => tc.Type is HeroCardHandler || tc.Type is RoleCardHandler || tc.Id == Card.UnknownHeroId || tc.Id == Card.UnknownRoleId);
                    Trace.TraceError("Dealing deck poisoning by card {0} @ {1}", card.Id, game.Decks[null, DeckType.Dealing].IndexOf(card));
                    Trace.Assert(false);
                }
            }

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
                var dealdeck = new List<Card>(game.Decks[DeckType.Dealing]);
                foreach (Card card in dealdeck)
                {
                    // We don't want hero cards
                    if (card.Type is HeroCardHandler)
                    {
                        game.Decks[DeckType.Dealing].Remove(card);
                        bool isSPHero = false;
                        if (!game.IsClient) isSPHero = (card.Type as HeroCardHandler).Hero.IsSpecialHero;
                        else isSPHero = card.Id == Card.UnknownSPHeroId;
                        if (isSPHero)
                        {
                            game.Decks[DeckType.SpecialHeroes].Add(card);
                            card.Place = new DeckPlace(null, DeckType.SpecialHeroes);
                        }
                        else
                        {
                            game.Decks[DeckType.Heroes].Add(card);
                            card.Place = new DeckPlace(null, DeckType.Heroes);
                        }

                    }
                    else if (card.Type is RoleCardHandler)
                    {
                        game.Decks[DeckType.Dealing].Remove(card);
                        card.Place = new DeckPlace(null, RoleDeckType);
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

                game.Decks[null, RoleDeckType].Add(_FindARoleCard(Role.Ruler));
                Trace.Assert(game.Players.Count > 1);
                Trace.Assert(numberOfDefectors + 1 <= game.Players.Count);
                int t = numberOfDefectors;
                while (t-- > 0)
                {
                    game.Decks[null, RoleDeckType].Add(_FindARoleCard(Role.Defector));
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
                    game.Decks[null, RoleDeckType].Add(_FindARoleCard(Role.Rebel));
                }
                while (loyalist-- > 0)
                {
                    game.Decks[null, RoleDeckType].Add(_FindARoleCard(Role.Loyalist));
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
                        c.Place = new DeckPlace(null, RoleDeckType);
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
                game.MoveCards(moves, null, GameDelayTypes.GameStart);

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

                GameDelays.Delay(GameDelayTypes.RoleDistribute);

                game.NotificationProxy.NotifyLogEvent(new LogEvent("HerosInitialization"), new List<Player>());
                if (!game.IsClient) GameDelays.Delay(GameDelayTypes.ServerSideCompensation);

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
                int numHeroes = Game.CurrentGame.Settings.DualHeroMode ? 2 : 1;
                List<Card> rulerDraw = new List<Card>();
                int toDraw = 12 + (Game.CurrentGame.Settings.DualHeroMode ? 3 : 0);
                for (int rc = 0; rc < toDraw; rc++)
                {
                    game.SyncImmutableCardAll(game.Decks[DeckType.Heroes][rc]);
                    rulerDraw.Add(game.Decks[DeckType.Heroes][rc]);
                }
                game.SyncImmutableCards(game.Players[rulerId], rulerDraw);
                DeckType tempHero = new DeckType("TempHero");
                game.Decks[null, tempHero].AddRange(rulerDraw);
                Trace.TraceInformation("Ruler is {0}", rulerId);
                game.Players[rulerId].Role = Role.Ruler;
                List<DeckPlace> sourceDecks = new List<DeckPlace>();
                sourceDecks.Add(new DeckPlace(null, tempHero));
                List<string> resultDeckNames = new List<string>();
                resultDeckNames.Add("HeroChoice");
                List<int> resultDeckMaximums = new List<int>();
                resultDeckMaximums.Add(numHeroes);
                List<List<Card>> answer;
                if (!game.UiProxies[game.Players[rulerId]].AskForCardChoice(new CardChoicePrompt("RulerHeroChoice"), sourceDecks, resultDeckNames, resultDeckMaximums, new RequireCardsChoiceVerifier(numHeroes, false, true), out answer))
                {
                    answer = new List<List<Card>>();
                    answer.Add(new List<Card>());
                    answer[0].Add(game.Decks[DeckType.Heroes][0]);
                    if (Game.CurrentGame.Settings.DualHeroMode)
                    {
                        answer[0].Add(game.Decks[DeckType.Heroes][1]);
                    }
                }
                game.SyncImmutableCardAll(answer[0][0]);
                if (Game.CurrentGame.Settings.DualHeroMode)
                {
                    game.SyncImmutableCardAll(answer[0][1]);
                }
                game.Decks[DeckType.Heroes].Remove(answer[0][0]);
                if (Game.CurrentGame.Settings.DualHeroMode)
                {
                    game.Decks[DeckType.Heroes].Remove(answer[0][1]);
                }

                HeroCardHandler h = (HeroCardHandler)answer[0][0].Type;
                Trace.TraceInformation("Assign {0} to player {1}", h.Hero.Name, rulerId);
                Game.CurrentGame.Players[rulerId].Hero = h.Hero;
                Game.CurrentGame.Players[rulerId].Allegiance = h.Hero.Allegiance;
                Game.CurrentGame.Players[rulerId].IsMale = h.Hero.IsMale ? true : false;
                Game.CurrentGame.Players[rulerId].IsFemale = h.Hero.IsMale ? false : true;
                if (Game.CurrentGame.Settings.DualHeroMode)
                {
                    h = (HeroCardHandler)answer[0][1].Type;
                    var hero2 = h.Hero.Clone() as Hero;
                    Trace.TraceInformation("Assign {0} to player {1}", hero2.Name, rulerId);
                    Game.CurrentGame.Players[rulerId].Hero2 = hero2;
                }
                Game.CurrentGame.Players[rulerId].MaxHealth = Game.CurrentGame.Players[rulerId].Health = (game as RoleGame).GetMaxHealth(Game.CurrentGame.Players[rulerId]);

                int optionalHeros = game.Settings.NumHeroPicks;
                toDraw = optionalHeros + (Game.CurrentGame.Settings.DualHeroMode ? Math.Max(6 - optionalHeros, 0) : 0);
                Shuffle(game.Decks[DeckType.Heroes]);
                Dictionary<Player, List<Card>> restDraw = new Dictionary<Player, List<Card>>();
                List<Player> players = new List<Player>(game.Players);
                players.Remove(game.Players[rulerId]);
                int idx = 0;
                foreach (Player p in players)
                {
                    restDraw.Add(p, new List<Card>());
                    for (int n = 0; n < toDraw; n++)
                    {
                        game.SyncImmutableCard(p, game.Decks[DeckType.Heroes][idx]);
                        restDraw[p].Add(game.Decks[DeckType.Heroes][idx]);
                        idx++;
                    }
                }

                var heroSelection = new Dictionary<Player, List<Card>>();
                game.GlobalProxy.AskForHeroChoice(restDraw, heroSelection, numHeroes, new RequireCardsChoiceVerifier(numHeroes));

                bool notUsed = true;
                game.SyncConfirmationStatus(ref notUsed);

                List<Card> toRemove = new List<Card>();
                for (int repeat = 0; repeat < 2; repeat++)
                {
                    if (repeat == 1 && !Game.CurrentGame.Settings.DualHeroMode) break;
                    foreach (Player p in players)
                    {
                        Card c;
                        //only server has the result
                        if (!game.IsClient)
                        {
                            idx = repeat;
                            if (heroSelection.ContainsKey(p))
                            {
                                c = heroSelection[p][repeat];
                                idx = restDraw[p].IndexOf(c);
                            }
                            else
                            {
                                c = restDraw[p][idx];
                            }
                            foreach (Player player in game.Players)
                            {
                                game.GameServer.SendObject(player.Id, idx);
                            }
                            game.GameServer.SendObject(game.Players.Count, idx);
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
                        if (repeat == 1) p.Hero2 = hero;
                        else p.Hero = hero;
                        if (repeat == 0) p.Allegiance = hero.Allegiance;
                        if (repeat == 0)
                        {
                            p.MaxHealth = p.Health = hero.MaxHealth;
                            p.IsMale = hero.IsMale ? true : false;
                            p.IsFemale = hero.IsMale ? false : true;
                        }
                        if (repeat == 1)
                        {
                            int aveHp = (p.Hero2.MaxHealth + p.Hero.MaxHealth) / 2;
                            p.MaxHealth = p.Health = aveHp;
                        }

                    }
                }
                foreach (var card in toRemove)
                {
                    game.Decks[DeckType.Heroes].Remove(card);
                }

                if (game.IsClient)
                {
                    foreach (var card in game.Decks[DeckType.Heroes])
                    {
                        card.Type = new UnknownHeroCardHandler();
                        card.Id = Card.UnknownHeroId;
                    }
                }
                Shuffle(game.Decks[DeckType.Heroes]);

                foreach (var pxy in game.UiProxies)
                {
                    pxy.Value.Freeze();
                }

                //Heroes Convert and handle god hero
                var toCheck = new List<Player>(game.Players);
                game.SortByOrderOfComputation(game.Players[rulerId], toCheck);
                Dictionary<string, List<Card>> convertibleHeroes = new Dictionary<string, List<Card>>();
                game.SyncImmutableCardsAll(game.Decks[DeckType.SpecialHeroes]);
                foreach (Card card in game.Decks[DeckType.SpecialHeroes])
                {
                    var hero = (card.Type as HeroCardHandler).Hero.HeroConvertFrom;
                    if (!convertibleHeroes.Keys.Contains(hero)) convertibleHeroes[hero] = new List<Card>();
                    convertibleHeroes[hero].Add(card);
                }
                foreach (var p in toCheck)
                {
                    bool changeHero = false;
                    for (int heroIndex = 0; heroIndex < 2; heroIndex++)
                    {
                        if (heroIndex == 1 && !Game.CurrentGame.Settings.DualHeroMode) break;
                        Hero playerHero = heroIndex == 0 ? p.Hero : p.Hero2;
                        if (convertibleHeroes.Keys.Contains(playerHero.Name))
                        {
                            DeckType tempSpHeroes = new DeckType("tempSpHeroes");
                            DeckPlace heroesConvert = new DeckPlace(p, tempSpHeroes);
                            game.Decks[heroesConvert].AddRange(convertibleHeroes[playerHero.Name]);
                            List<List<Card>> choice;
                            AdditionalCardChoiceOptions options = new AdditionalCardChoiceOptions();
                            options.IsCancellable = true;
                            if (p.AskForCardChoice(new CardChoicePrompt("HeroesConvert", p),
                                new List<DeckPlace>() { heroesConvert },
                                new List<string>() { "convert" },
                                new List<int>() { 1 },
                                new RequireOneCardChoiceVerifier(false, true),
                                out choice,
                                options))
                            {
                                foreach (var sk in playerHero.Skills)
                                {
                                    sk.Owner = null;
                                }
                                Hero hero = ((choice[0][0].Type as HeroCardHandler).Hero.Clone()) as Hero;
                                foreach (var skill in new List<ISkill>(hero.Skills))
                                {
                                    if (skill.IsRulerOnly && (p.Role != Role.Ruler || heroIndex == 1))
                                    {
                                        hero.Skills.Remove(skill);
                                    }
                                }
                                if (heroIndex == 0)
                                {
                                    p.Hero = hero;
                                    p.Allegiance = hero.Allegiance;
                                    p.IsMale = hero.IsMale ? true : false;
                                    p.IsFemale = hero.IsMale ? false : true;
                                }
                                else p.Hero2 = hero;
                                changeHero = true;
                                game.Emit(GameEvent.PlayerChangedHero, new GameEventArgs() { Source = p });
                            }
                            game.Decks[heroesConvert].Clear();
                        }
                    }
                    if (changeHero) p.MaxHealth = p.Health = (game as RoleGame).GetMaxHealth(p);
                    Game.CurrentGame.HandleGodHero(p);
                }

                Shuffle(game.Decks[null, DeckType.Dealing]);

                Player current = game.CurrentPlayer = game.Players[rulerId];

                GameDelays.Delay(GameDelayTypes.GameBeforeStart);
                StartGameDeal(game);

                Game.CurrentGame.NotificationProxy.NotifyGameStart();
                GameDelays.Delay(GameDelayTypes.GameStart);
                GameDelays.Delay(GameDelayTypes.GameStart);

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
                    if (game.CurrentPhaseEventIndex >= Game.PhaseEvents.Length || currentPlayer.IsDead)
                    {
                        game.CurrentPhaseEventIndex = 0;
                        game.CurrentPhase++;
                        if ((int)game.CurrentPhase >= Enum.GetValues(typeof(TurnPhase)).Length || (int)game.CurrentPhase < 0 || currentPlayer.IsDead)
                        {
                            break;
                        }
                        GameDelays.Delay(GameDelayTypes.ChangePhase);
                    }
                }
                game.CurrentPhase = TurnPhase.Inactive;
                Game.CurrentGame.Emit(GameEvent.PhasePostEnd, new GameEventArgs() { Source = currentPlayer });
            }
        }


        public RoleGame()
        {
        }

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
            DeckType role = new DeckType("Role");
            private void RevealAllPlayersRoles()
            {
                foreach (var player in Game.CurrentGame.Players)
                {
                    Game.CurrentGame.SyncImmutableCardAll(Game.CurrentGame.Decks[player, role][0]);
                    var r = (Game.CurrentGame.Decks[player, role][0].Type as RoleCardHandler).Role;
                    if (player.Role != r)
                    {
                        player.Role = r;
                    }
                }
            }

            private void TallyGameResult(List<Player> winners)
            {
                if (Game.CurrentGame.GameServer == null) return;
                foreach (Player p in Game.CurrentGame.Players)
                {
                    int idx = Game.CurrentGame.Players.IndexOf(p);
                    Game.CurrentGame.Settings.Accounts[idx].TotalGames++;
                    if (Game.CurrentGame.GameServer.IsDisconnected(idx))
                    {
                        var account = Game.CurrentGame.Settings.Accounts[idx];
                        var sidx = Game.CurrentGame.Configuration.Accounts.IndexOf(account);
                        if (!Game.CurrentGame.Configuration.isDead[sidx])
                        {
                            Game.CurrentGame.Settings.Accounts[idx].Quits++;
                            continue;
                        }
                    }
                    if (winners.Contains(p))
                    {
                        Game.CurrentGame.Settings.Accounts[idx].Wins++;
                        Game.CurrentGame.Settings.Accounts[idx].Experience += 5;
                        if (p.Role == Role.Defector) Game.CurrentGame.Settings.Accounts[idx].Experience += 50;
                    }
                    else
                    {
                        Game.CurrentGame.Settings.Accounts[idx].Losses++;
                        Game.CurrentGame.Settings.Accounts[idx].Experience -= 1;
                    }
                }
            }

            private void ReleaseIntoLobby(Player p)
            {
                if (Game.CurrentGame.GameServer == null) return;
                if (Game.CurrentGame.Settings == null) return;
                if (Game.CurrentGame.Configuration == null) return;
                var idx = Game.CurrentGame.Players.IndexOf(p);
                var account = Game.CurrentGame.Settings.Accounts[idx];
                idx = Game.CurrentGame.Configuration.Accounts.IndexOf(account);
                Game.CurrentGame.Configuration.isDead[idx] = true;
            }

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
                Game.CurrentGame.Emit(GameEvent.BeforeRevealRole, eventArgs);

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
                        RevealAllPlayersRoles();
                        var winners = from pl in Game.CurrentGame.Players where pl.Role == Role.Defector select pl;
                        Game.CurrentGame.NotificationProxy.NotifyGameOver(false, winners.ToList());
                        TallyGameResult(new List<Player>(winners));
                    }
                    else
                    {
                        RevealAllPlayersRoles();
                        var winners = from pl in Game.CurrentGame.Players where pl.Role == Role.Rebel select pl;
                        Game.CurrentGame.NotificationProxy.NotifyGameOver(false, winners.ToList());
                        TallyGameResult(new List<Player>(winners));
                    }
                    p.IsDead = true;
                    throw new GameOverException();
                }

                ReleaseIntoLobby(p);

                if (p.Role == Role.Rebel || p.Role == Role.Defector)
                {
                    int deadRebel = 0;
                    int deadDefector = 0;
                    foreach (Player z in Game.CurrentGame.Players)
                    {
                        if (z.Role == Role.Rebel && (z.IsDead || z == p))
                        {
                            deadRebel++;
                        }
                        if (z.Role == Role.Defector && (z.IsDead || z == p))
                        {
                            deadDefector++;
                        }
                    }
                    Trace.TraceInformation("Deathtoll: Rebel {0}/{1}, Defector {2}/{3}", deadRebel, (Game.CurrentGame as RoleGame).NumberOfRebels, deadDefector, (Game.CurrentGame as RoleGame).NumberOfDefectors);
                    if (deadRebel == (Game.CurrentGame as RoleGame).NumberOfRebels && deadDefector == (Game.CurrentGame as RoleGame).NumberOfDefectors)
                    {
                        Trace.TraceInformation("Ruler wins.");
                        RevealAllPlayersRoles();
                        var winners = from pl in Game.CurrentGame.Players where pl.Role == Role.Ruler || pl.Role == Role.Loyalist select pl;
                        Game.CurrentGame.NotificationProxy.NotifyGameOver(false, winners.ToList());
                        TallyGameResult(new List<Player>(winners));
                        p.IsDead = true;
                        throw new GameOverException();
                    }
                }

                Game.CurrentGame.Emit(GameEvent.PlayerIsDead, eventArgs);
                p.IsDead = true;
                //弃置死亡玩家所有的牌和标记
                Game.CurrentGame.SyncImmutableCardsAll(Game.CurrentGame.Decks[p, DeckType.Hand]);
                List<Card> toDiscarded = new List<Card>();
                toDiscarded.AddRange(p.HandCards());
                toDiscarded.AddRange(p.Equipments());
                toDiscarded.AddRange(p.DelayedTools());
                toDiscarded.AddRange(Game.CurrentGame.Decks.GetPlayerPrivateCards(p));
                Game.CurrentGame.HandleCardDiscard(p, toDiscarded);
                var makeACopy = new List<PlayerAttribute>(p.Attributes.Keys);
                foreach (var kvp in makeACopy)
                {
                    if (kvp.IsMark)
                        p[kvp] = 0;
                }

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
                    if (source != null && !source.IsDead && p.Role == Role.Rebel)
                    {
                        Trace.TraceInformation("Killed rebel. GIVING YOU THREE CARDS OMG WIN GAME RIGHT THERE!!!");
                        Game.CurrentGame.DrawCards(source, 3);
                    }
                }
                if (p.Role == Role.Loyalist && source != null && source.Role == Role.Ruler)
                {
                    Trace.TraceInformation("Loyalist killl by ruler. GG");
                    Game.CurrentGame.SyncImmutableCardsAll(Game.CurrentGame.Decks[source, DeckType.Hand]);
                    CardsMovement move = new CardsMovement();
                    move.Cards = new List<Card>();
                    bool showHandCards = false;
                    foreach (Card c in Game.CurrentGame.Decks[source, DeckType.Hand])
                    {
                        if (Game.CurrentGame.PlayerCanDiscardCard(source, c))
                        {
                            move.Cards.Add(c);
                        }
                        else showHandCards = true;
                    }
                    if (showHandCards)
                    {
                        Game.CurrentGame.ShowHandCards(p, p.HandCards());
                        Game.CurrentGame.SyncImmutableCardsAll(move.Cards);
                    }
                    List<Card> cards = new List<Card>();
                    cards.AddRange(move.Cards);
                    cards.AddRange(Game.CurrentGame.Decks[source, DeckType.Equipment]);
                    move.Cards = new List<Card>(cards);
                    move.To = new DeckPlace(null, DeckType.Discard);
                    move.Helper = new MovementHelper();
                    Game.CurrentGame.MoveCards(move);
                    Game.CurrentGame.PlayerLostCard(p, cards);
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
                foreach (var c in Game.CurrentGame.Decks[null, DeckType.Discard])
                {
                    if (Game.CurrentGame.IsClient)
                    {
                        c.Id = -1;
                    }
                    c.Place = new DeckPlace(null, DeckType.Dealing);
                    Game.CurrentGame.Decks[null, DeckType.Dealing].Add(c);
                }
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
            RegisterTrigger(GameEvent.PlayerSkillSetChanged, cleanupSquad);
        }
    }

}
