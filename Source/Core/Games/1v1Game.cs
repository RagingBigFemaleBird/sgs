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
        protected override void InitTriggers()
        {
            RegisterTrigger(GameEvent.DoPlayer, new DoPlayerTrigger());
            RegisterTrigger(GameEvent.Shuffle, new ShuffleTrigger());
            RegisterTrigger(GameEvent.GameStart, new Pk1v1GameRuleTrigger());
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
                if (Game.CurrentGame.Decks[p, SelectedHero].Count <= 3)
                {
                    // 6 - 3 = 3. gg
                    Trace.TraceInformation("Out of heroes. Game over");

                    var winners = from pl in Game.CurrentGame.Players where pl != p select pl;
                    p.IsDead = true;
                    throw new GameOverException(false, winners);
                }

                Game.CurrentGame.Emit(GameEvent.PlayerIsDead, eventArgs);
                //弃置死亡玩家所有的牌和标记
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
                        if (s is TriggerSkill)
                        {
                            (s as TriggerSkill).Owner = null;
                        }
                    }
                }
                List<DeckPlace> sourceDecks = new List<DeckPlace>();
                sourceDecks.Add(new DeckPlace(p, SelectedHero));
                List<string> resultDeckNames = new List<string>();
                resultDeckNames.Add("HeroChoice");
                List<int> resultDeckMaximums = new List<int>();
                resultDeckMaximums.Add(1);
                List<List<Card>> answer;
                var newVer = new RequireCardsChoiceVerifier(1, false, true);

                if (!p.AskForCardChoice(new CardChoicePrompt("RulerHeroChoice"), sourceDecks, resultDeckNames, resultDeckMaximums, newVer, out answer))
                {
                    answer = new List<List<Card>>();
                    answer.Add(new List<Card>() { Game.CurrentGame.Decks[p, SelectedHero].First() });
                }
                var c = answer[0][0];
                Game.CurrentGame.Decks[p, SelectedHero].Remove(c);
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
                p.Hero = hero;
                p.Allegiance = hero.Allegiance;
                p.MaxHealth = p.Health = hero.MaxHealth;
                p.IsMale = hero.IsMale ? true : false;
                p.IsFemale = hero.IsMale ? false : true;
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

            public class _1v1HeroChoiceVerifier : ICardChoiceVerifier
            {
                bool noCardReveal;
                int count;
                bool showToall;
                public _1v1HeroChoiceVerifier(int count)
                {
                    noCardReveal = false;
                    this.count = count;
                    this.showToall = true;
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
                    get { return new UiHelper() { RevealCards = !noCardReveal, ShowToAll = showToall }; }
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

                game.NotificationProxy.NotifyLogEvent(new LogEvent("HerosInitialization"), new List<Player>());
                if (!game.IsClient) GameDelays.Delay(GameDelays.ServerSideCompensation);

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
                    var newVer = new _1v1HeroChoiceVerifier(numHeroes);
                    if (numHeroes > 1) newVer.Helper.ExtraTimeOutSeconds = 10;
                    if (!game.UiProxies[game.Players[turn]].AskForCardChoice(new CardChoicePrompt("RulerHeroChoice"), sourceDecks, resultDeckNames, resultDeckMaximums, newVer, out answer))
                    {
                        answer = new List<List<Card>>();
                        answer.Add(new List<Card>());
                        for (int jj = 0; jj < numHeroes; jj++)
                        {
                            answer[0].Add(game.Decks[null, tempHero].First(h => !answer[0].Contains(h) && !game.Decks[game.Players[turn], SelectedHero].Contains(h) && !game.Decks[game.Players[1 - turn], SelectedHero].Contains(h)));
                        }
                    }
                    game.Decks[game.Players[turn], SelectedHero].AddRange(answer[0]);
                    seq++;
                    turn = 1 - turn;
                }

                game.Shuffle(game.Decks[null, DeckType.Dealing]);

                Player current = game.CurrentPlayer = game.Players[1 - rulerId];

                Dictionary<Player, List<Card>> restDraw = new Dictionary<Player, List<Card>>();
                List<Player> players = new List<Player>(game.Players);
                foreach (Player p in players)
                {
                    restDraw.Add(p, new List<Card>(game.Decks[p, SelectedHero]));
                }

                var heroSelection = new Dictionary<Player, List<Card>>();
                game.GlobalProxy.AskForHeroChoice(restDraw, heroSelection, 1, new RequireCardsChoiceVerifier(1, false, true));

                bool notUsed = true;
                game.SyncConfirmationStatus(ref notUsed);

                foreach (var pxy in game.UiProxies)
                {
                    pxy.Value.Freeze();
                }

                foreach (Player p in players)
                {
                    Card c;
                    int idx = 0;
                    //only server has the result
                    if (!game.IsClient)
                    {
                        idx = 0;
                        if (heroSelection.ContainsKey(p))
                        {
                            c = heroSelection[p][0];
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
                    p.Hero = hero;
                    p.Allegiance = hero.Allegiance;
                    p.MaxHealth = p.Health = hero.MaxHealth;
                    p.IsMale = hero.IsMale ? true : false;
                    p.IsFemale = hero.IsMale ? false : true;

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

                foreach (var pl in game.Players)
                {
                    StartGameDeal(game, pl);
                }
                foreach (var pl in game.Players)
                {
                    game.Emit(GameEvent.HeroDebut, new GameEventArgs() { Source = pl });
                }
                //redo this: current player might change
                current = game.CurrentPlayer = game.Players[1 - rulerId];

                GameDelays.Delay(GameDelays.GameBeforeStart);

                Game.CurrentGame.NotificationProxy.NotifyGameStart();
                GameDelays.Delay(GameDelays.GameStart);
                GameDelays.Delay(GameDelays.GameStart);

                foreach (var act in game.AlivePlayers)
                {
                    game.Emit(GameEvent.PlayerGameStartAction, new GameEventArgs() { Source = act });
                }
                game.CurrentPlayer = game.Players[1 - current.Id];
                current[Player.DealAdjustment] = -1;
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
