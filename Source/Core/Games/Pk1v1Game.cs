using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Sanguosha.Lobby.Core;
using Sanguosha.Core.Network;
using System.Diagnostics;

namespace Sanguosha.Core.Games
{
    public class Pk1v1Game : RoleGame
    {
        static DeckType ReadyToGoHeroes = DeckType.Register("ReadyToGoHeroes");

        public class Pk1v1PlayerActionTrigger : Trigger
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
                        VerifierResult ret = s.TryTransform(cards, players, out result);
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

            private bool GetReadyToGo(Player p)
            {
                var lists = new List<Card>(Game.CurrentGame.Decks[p, ReadyToGoHeroes]);
                Game.CurrentGame.Decks[p, ReadyToGoHeroes].Clear();
                if (lists.Count > 0)
                {
                    for (int repeat = 0; repeat < lists.Count; repeat++)
                    {
                        var c = lists[repeat];
                        var h = (HeroCardHandler)c.Type;
                        Trace.TraceInformation("Assign {0} to player {1}", h.Hero.Name, p.Id);
                        var hero = h.Hero.Clone() as Hero;
                        foreach (var skill in new List<ISkill>(hero.Skills))
                        {
                            if (skill.IsRulerOnly)
                            {
                                hero.Skills.Remove(skill);
                            }
                        }
                        if (repeat == 0)
                        {
                            p.Hero = hero;
                        }
                        else
                        {
                            p.Hero2 = hero;
                        }
                    }
                    Game.CurrentGame.Emit(GameEvent.HeroDebut, new GameEventArgs() { Source = p });
                    return true;
                }
                return false;
            }

            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Player currentPlayer = Game.CurrentGame.CurrentPlayer;
                Trace.TraceInformation("Player {0} action.", currentPlayer.Id);
                while (!currentPlayer.IsDead)
                {
                    bool newturn = false;
                    foreach (var pl in Game.CurrentGame.Players)
                    {
                        if (GetReadyToGo(pl) && pl == currentPlayer)
                        {
                            newturn = true;
                        }
                    }
                    if (newturn) return;
                    Trace.Assert(Game.CurrentGame.UiProxies.ContainsKey(currentPlayer));
                    IPlayerProxy proxy = Game.CurrentGame.UiProxies[currentPlayer];
                    ISkill skill;
                    List<Card> cards;
                    List<Player> players;
                    PlayerActionStageVerifier v = new PlayerActionStageVerifier();
                    Game.CurrentGame.Emit(GameEvent.PlayerIsAboutToUseCard, new PlayerIsAboutToUseOrPlayCardEventArgs() { Source = currentPlayer, Verifier = v });
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
                        VerifierResult r = s.TryTransform(cards, players, out c);
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
        protected override void InitTriggers()
        {
            RegisterTrigger(GameEvent.DoPlayer, new DoPlayerTrigger());
            RegisterTrigger(GameEvent.Shuffle, new ShuffleTrigger());
            RegisterTrigger(GameEvent.GameStart, new Pk1v1GameRuleTrigger());
            RegisterTrigger(GameEvent.PhaseProceedEvents[TurnPhase.Judge], new PlayerJudgeStageTrigger());
            RegisterTrigger(GameEvent.PhaseProceedEvents[TurnPhase.Play], new Pk1v1PlayerActionTrigger());
            RegisterTrigger(GameEvent.PhaseProceedEvents[TurnPhase.Draw], new PlayerDealStageTrigger() { Priority = -1 });
            RegisterTrigger(GameEvent.PhaseProceedEvents[TurnPhase.Discard], new PlayerDiscardStageTrigger() { Priority = -1 });
            RegisterTrigger(GameEvent.CommitActionToTargets, new CommitActionToTargetsTrigger());
            RegisterTrigger(GameEvent.AfterHealthChanged, new PlayerHpChanged());
            RegisterTrigger(GameEvent.GameProcessPlayerIsDead, new PlayerIsDead() { Priority = int.MinValue });
            RegisterTrigger(GameEvent.CardUsageBeforeEffected, new DeadManStopper() { Priority = int.MaxValue });
            RegisterTrigger(GameEvent.CardUsageBeforeEffected, new DeadManStopper() { Priority = int.MinValue });
            RegisterTrigger(GameEvent.PlayerSkillSetChanged, cleanupSquad);
            var trigger = new InitialDealAdjustment() { Priority = int.MaxValue };
            RegisterTrigger(GameEvent.PhaseProceedEvents[TurnPhase.Draw], trigger);
            RegisterTrigger(GameEvent.PhasePostEnd, new InitialDealAdjustmentUnregister(trigger));
        }
        public static DeckType SelectedHero = DeckType.Register("SelectedHero");
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

                Game.CurrentGame.NotificationProxy.NotifyDeath(p, source);
                if (Game.CurrentGame.Decks[p, SelectedHero].Count <= (Game.CurrentGame.Settings.DualHeroMode ? 0 : 3))
                {
                    // 6 - 3 = 3. gg
                    Trace.TraceInformation("Out of heroes. Game over");

                    var winners = from pl in Game.CurrentGame.Players where pl != p select pl;
                    p.IsDead = true;
                    throw new GameOverException(false, winners);
                }

                Game.CurrentGame.Emit(GameEvent.PlayerIsDead, eventArgs);
                //弃置死亡玩家所有的牌和标记
                p.IsDead = true;
                Game.CurrentGame.SyncImmutableCardsAll(Game.CurrentGame.Decks[p, DeckType.Hand]);
                List<Card> toDiscarded = new List<Card>();
                toDiscarded.AddRange(p.HandCards());
                toDiscarded.AddRange(p.Equipments());
                toDiscarded.AddRange(p.DelayedTools());
                List<Card> privateCards = Game.CurrentGame.Decks.GetPlayerPrivateCards(p);
                var heroCards = from hc in privateCards where hc.Type.IsCardCategory(CardCategory.Hero) select hc;
                toDiscarded.AddRange(privateCards.Except(heroCards));
                if (heroCards.Count() > 0)
                {
                    if (Game.CurrentGame.IsClient)
                    {
                        foreach (var hc in heroCards)
                        {
                            hc.Id = Card.UnknownHeroId;
                            hc.Type = new UnknownHeroCardHandler();
                        }
                    }
                    CardsMovement move = new CardsMovement();
                    move.Cards.AddRange(heroCards);
                    move.To = new DeckPlace(null, DeckType.Heroes);
                    move.Helper.IsFakedMove = true;
                    Game.CurrentGame.MoveCards(move);
                }
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
                        s.Owner = null;
                    }
                }
                if (p.Hero2 != null)
                {
                    foreach (ISkill s in p.Hero2.Skills)
                    {
                        s.Owner = null;
                    }
                }
                p.IsDead = false;
                List<DeckPlace> sourceDecks = new List<DeckPlace>();
                sourceDecks.Add(new DeckPlace(p, SelectedHero));
                List<string> resultDeckNames = new List<string>();
                resultDeckNames.Add("HeroChoice");
                List<int> resultDeckMaximums = new List<int>();
                List<List<Card>> answer;
                int numberOfHeroes = Game.CurrentGame.Settings.DualHeroMode ? 2 : 1;
                resultDeckMaximums.Add(numberOfHeroes);
                var newVer = new RequireCardsChoiceVerifier(numberOfHeroes, false, true);

                if (!p.AskForCardChoice(new CardChoicePrompt("Pk1v1.NextHeroChoice", numberOfHeroes), sourceDecks, resultDeckNames, resultDeckMaximums, newVer, out answer))
                {
                    answer = new List<List<Card>>();
                    answer.Add(new List<Card>() { Game.CurrentGame.Decks[p, SelectedHero].First() });
                    if (numberOfHeroes == 2) answer.Add(new List<Card>() { Game.CurrentGame.Decks[p, SelectedHero].ElementAt(1) });
                }
                Game.CurrentGame.Decks[p, ReadyToGoHeroes].AddRange(answer[0]);
                for (int repeat = 0; repeat < numberOfHeroes; repeat++)
                {
                    var c = answer[0][repeat];
                    Game.CurrentGame.Decks[p, SelectedHero].Remove(c);
                    var h = (HeroCardHandler)c.Type;
                    var hero = h.Hero;
                    if (repeat == 0)
                    {
                        p.Allegiance = hero.Allegiance;
                    }
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
                StartGameDeal(Game.CurrentGame, p);
                Game.CurrentGame.Emit(GameEvent.HeroDebut, new GameEventArgs() { Source = p });
            }
        }

        private class Pk1v1GameRuleTrigger : Trigger
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

            static Pk1v1GameRuleTrigger()
            {
                allRoleCards = new List<Card>(from c in GameEngine.CardSet
                                              where c.Type is RoleCardHandler
                                              select c);
            }

            public Pk1v1GameRuleTrigger()
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

            public class Pk1v1HeroChoiceVerifier : ICardChoiceVerifier
            {
                bool noCardReveal;
                int count;
                bool showToall;
                int extraSec;
                public Pk1v1HeroChoiceVerifier(int count, int extraSeconds)
                {
                    noCardReveal = false;
                    this.count = count;
                    this.showToall = true;
                    extraSec = extraSeconds;
                }
                public VerifierResult Verify(List<List<Card>> answer)
                {
                    if ((answer.Count > 1) || (answer.Count > 0 && answer[0].Count > count))
                    {
                        return VerifierResult.Fail;
                    }
                    if (answer != null && answer[0] != null)
                    {
                        foreach (var h in answer[0])
                        {
                            if (Game.CurrentGame.Decks[Game.CurrentGame.Players[0], SelectedHero].Contains(h))
                            {
                                return VerifierResult.Fail;
                            }
                            if (Game.CurrentGame.Decks[Game.CurrentGame.Players[1], SelectedHero].Contains(h))
                            {
                                return VerifierResult.Fail;
                            }
                        }
                    }
                    if (answer == null || answer[0] == null || answer[0].Count < count)
                    {
                        return VerifierResult.Partial;
                    }
                    return VerifierResult.Success;
                }
                public UiHelper Helper
                {
                    get { return new UiHelper() { RevealCards = !noCardReveal, ShowToAll = showToall, ExtraTimeOutSeconds = extraSec }; }
                }
            }
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Pk1v1Game game = Game.CurrentGame as Pk1v1Game;

                foreach (Player pp in game.Players)
                {
                    game.HandCardVisibility.Add(pp, new List<Player>() { pp });
                }

                // Put the whole deck in the dealing deck

                foreach (Card card in game.CardSet)
                {
                    // We don't want hero cards
                    if (card.Type is HeroCardHandler)
                    {
                        game.Decks[DeckType.Heroes].Add(card);
                        card.Place = new DeckPlace(null, DeckType.Heroes);
                    }
                    else if (card.Type is RoleCardHandler)
                    {
                        card.Place = new DeckPlace(null, RoleDeckType);
                    }
                    else
                    {
                        game.Decks[DeckType.Dealing].Add(card);
                        card.Place = new DeckPlace(null, DeckType.Dealing);
                    }
                }
                if (game.Players.Count == 0)
                {
                    return;
                }
                // Await role decision
                int seed = DateTime.Now.Millisecond;
                game.Seed = seed;
                Trace.TraceError("Seed is {0}", seed);
                if (game.RandomGenerator == null)
                {
                    game.RandomGenerator = new Random(seed);
                    Random random = game.RandomGenerator;
                }
                int selectorId = game.RandomGenerator.Next(2);
                int rulerId = 0;
                bool selectorIs0 = selectorId == 0;
                game.SyncConfirmationStatus(ref selectorIs0);
                if (selectorIs0)
                {
                    selectorId = 0;
                }
                else
                {
                    selectorId = 1;
                }
                int wantToBeRuler = 0;
                game.Players[selectorId].AskForMultipleChoice(new MultipleChoicePrompt("BeRuler"), OptionPrompt.YesNoChoices, out wantToBeRuler);
                rulerId = 1 - (wantToBeRuler ^ selectorId);
                Trace.Assert(rulerId >= 0 && rulerId <= 1);
                Trace.Assert(game.Players.Count == 2);
                if (rulerId == 0)
                {
                    game.AvailableRoles.Add(Role.Ruler);
                    game.AvailableRoles.Add(Role.Defector);
                    game.Decks[null, RoleDeckType].Add(_FindARoleCard(Role.Ruler));
                    game.Decks[null, RoleDeckType].Add(_FindARoleCard(Role.Defector));
                }
                else
                {
                    game.AvailableRoles.Add(Role.Defector);
                    game.AvailableRoles.Add(Role.Ruler);
                    game.Decks[null, RoleDeckType].Add(_FindARoleCard(Role.Defector));
                    game.Decks[null, RoleDeckType].Add(_FindARoleCard(Role.Ruler));
                }

                List<CardsMovement> moves = new List<CardsMovement>();
                int i = 0;
                foreach (Player p in game.Players)
                {
                    CardsMovement move = new CardsMovement();
                    move.Cards = new List<Card>() { game.Decks[null, RoleDeckType][i] };
                    move.To = new DeckPlace(p, RoleDeckType);
                    moves.Add(move);
                    i++;
                }
                game.MoveCards(moves, null, GameDelays.GameStart);
                GameDelays.Delay(GameDelays.RoleDistribute);
                //hero allocation
                game.Shuffle(game.Decks[DeckType.Heroes]);

                List<Card> heroPool = new List<Card>();
                int toDraw = 12;
                for (int rc = 0; rc < toDraw; rc++)
                {
                    game.SyncImmutableCardAll(game.Decks[DeckType.Heroes][rc]);
                    heroPool.Add(game.Decks[DeckType.Heroes][rc]);
                }
                game.SyncImmutableCards(game.Players[rulerId], heroPool);
                DeckType tempHero = DeckType.Register("TempHero");
                game.Decks[null, tempHero].AddRange(heroPool);
                Trace.TraceInformation("Ruler is {0}", rulerId);
                game.Players[rulerId].Role = Role.Ruler;
                game.Players[1 - rulerId].Role = Role.Defector;

                List<int> heroSelectCount = new List<int>() { 1, 2, 2, 2, 2, 2, 1 };
                int seq = 0;
                int turn = rulerId;
                Dictionary<int, int> map = new Dictionary<int,int>();
                map.Add(0, 0);
                map.Add(1, 1);
                var deckPlace = new DeckPlace(null, tempHero);
                game.NotificationProxy.NotifyTwoSidesCardPickStart(new CardChoicePrompt("Pk1v1.InitHeroPick.Init"), deckPlace, map, 6, 6);
                while (heroSelectCount.Count > seq)
                {
                    List<DeckPlace> sourceDecks = new List<DeckPlace>();
                    sourceDecks.Add(new DeckPlace(null, tempHero));
                    List<string> resultDeckNames = new List<string>();
                    resultDeckNames.Add("HeroChoice");
                    List<int> resultDeckMaximums = new List<int>();
                    int numHeroes = heroSelectCount[seq];
                    resultDeckMaximums.Add(numHeroes);
                    List<List<Card>> answer;
                    var newVer = new Pk1v1HeroChoiceVerifier(1, seq + 1 == heroSelectCount.Count ? -(Game.CurrentGame.Settings.TimeOutSeconds - 2) : 0);
                    for (int j = 0; j < numHeroes; j++)
                    {
                        var option = new AdditionalCardChoiceOptions();
                        option.IsTwoSidesCardChoice = true;
                        if (!game.UiProxies[game.Players[turn]].AskForCardChoice(new CardChoicePrompt("Pk1v1.InitHeroPick", numHeroes), sourceDecks, resultDeckNames, resultDeckMaximums, newVer, out answer, option))
                        {
                            answer = new List<List<Card>>();
                            answer.Add(new List<Card>());
                            answer[0].Add(game.Decks[null, tempHero].First(h => !answer[0].Contains(h) && !game.Decks[game.Players[turn], SelectedHero].Contains(h) && !game.Decks[game.Players[1 - turn], SelectedHero].Contains(h)));
                        }
                        game.Decks[game.Players[turn], SelectedHero].AddRange(answer[0]);
                        game.NotificationProxy.NotifyTwoSidesCardPicked(turn == 0, game.Decks[deckPlace].IndexOf(answer[0][0]));
                    }
                    seq++;
                    turn = 1 - turn;
                }
                GameDelays.Delay(GameDelays.Pk1v1EndOfSelection);
                game.NotificationProxy.NotifyTwoSidesCardPickEnd();
                game.Shuffle(game.Decks[null, DeckType.Dealing]);

                Player current = game.CurrentPlayer = game.Players[1 - rulerId];

                Dictionary<Player, List<Card>> restDraw = new Dictionary<Player, List<Card>>();
                List<Player> players = new List<Player>(game.Players);
                foreach (Player p in players)
                {
                    restDraw.Add(p, new List<Card>(game.Decks[p, SelectedHero]));
                }

                var heroSelection = new Dictionary<Player, List<Card>>();
                int numberOfHeroes = Game.CurrentGame.Settings.DualHeroMode ? 2 : 1;
                game.GlobalProxy.AskForHeroChoice(restDraw, heroSelection, numberOfHeroes, new RequireCardsChoiceVerifier(numberOfHeroes, false, true));

                bool notUsed = true;
                game.SyncConfirmationStatus(ref notUsed);

                foreach (var pxy in game.UiProxies)
                {
                    pxy.Value.Freeze();
                }

                for (int repeat = 0; repeat < numberOfHeroes; repeat++)
                {
                    foreach (Player p in players)
                    {
                        Card c;
                        int idx;
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
                            if (game.GameServer != null)
                            {
                                foreach (Player player in game.Players)
                                {
                                    game.GameServer.SendPacket(player.Id, new StatusSync() { Status = idx });
                                }
                                game.GameServer.SendPacket(game.Players.Count, new StatusSync() { Status = idx });
                            }
                        }
                        // you are client
                        else
                        {
                            idx = (int)game.GameClient.Receive();
                            c = restDraw[p][idx];
                        }
                        game.Decks[p, SelectedHero].Remove(c);
                        var h = (HeroCardHandler)c.Type;
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

                foreach (var rm in heroPool)
                {
                    game.Decks[DeckType.Heroes].Remove(rm);
                }
                foreach (var st in game.Decks[game.Players[0], SelectedHero])
                {
                    st.Place = new DeckPlace(game.Players[0], SelectedHero);
                }
                foreach (var st in game.Decks[game.Players[1], SelectedHero])
                {
                    st.Place = new DeckPlace(game.Players[1], SelectedHero);
                } 
                game.Shuffle(game.Decks[DeckType.Heroes]);
                if (game.IsClient)
                {
                    foreach (var card in game.Decks[DeckType.Heroes])
                    {
                        card.Type = new UnknownHeroCardHandler();
                        card.Id = Card.UnknownHeroId;
                    }
                }

                GameDelays.Delay(GameDelays.GameBeforeStart);

                Game.CurrentGame.NotificationProxy.NotifyGameStart();
                GameDelays.Delay(GameDelays.GameStart);
                GameDelays.Delay(GameDelays.GameStart);

                foreach (var pl in game.Players)
                {
                    StartGameDeal(game, pl);
                }
                foreach (var pl in game.Players)
                {
                    try
                    {
                        game.Emit(GameEvent.HeroDebut, new GameEventArgs() { Source = pl });
                    }
                    catch (EndOfTurnException)
                    {
                        game.CurrentPlayer = game.Players[1 - game.CurrentPlayer.Id];
                    }
                }


                foreach (var act in game.AlivePlayers)
                {
                    game.Emit(GameEvent.PlayerGameStartAction, new GameEventArgs() { Source = act });
                }

                //redo this: current player might change
                current = game.CurrentPlayer;
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

        public class InitialDealAdjustment : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Game.CurrentGame.CurrentPlayer[Player.DealAdjustment]--;
            }
        }
        public class InitialDealAdjustmentUnregister : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhaseProceedEvents[TurnPhase.Draw], theTrigger);
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhasePostEnd, this);
            }
            InitialDealAdjustment theTrigger;
            public InitialDealAdjustmentUnregister(InitialDealAdjustment trigger)
            {
                theTrigger = trigger;
            }
        }

        protected static void StartGameDeal(Game game, Player player)
        {
            List<CardsMovement> moves = new List<CardsMovement>();
            CardsMovement move = new CardsMovement();
            move.Cards = new List<Card>();
            move.To = new DeckPlace(player, DeckType.Hand);
            game.Emit(GameEvent.StartGameDeal, new GameEventArgs() { Source = player });
            int dealCount = player.MaxHealth + player[Player.DealAdjustment];
            for (int i = 0; i < dealCount; i++)
            {
                game.SyncImmutableCard(player, game.PeekCard(0));
                Card c = game.DrawCard();
                move.Cards.Add(c);
            }
            moves.Add(move);
            game.MoveCards(moves, null, GameDelays.GameBeforeStart);
        }

    }
}
