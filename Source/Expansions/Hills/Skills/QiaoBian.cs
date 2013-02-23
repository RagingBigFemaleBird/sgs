using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Hills.Skills
{
    /// <summary>
    /// 巧变-你可以弃置一张手牌跳过你的一个阶段(回合开始和回合结束阶段除外)，若以此法跳过摸牌阶段，你获得其他至多两名角色各一张手牌；若以此法跳过出牌阶段，你可以将场上的一张牌移动到另一名角色区域里的相应位置。
    /// </summary>
    public class QiaoBian : TriggerSkill
    {
        protected override int GenerateSpecialEffectHintIndex(Player source, List<Player> targets)
        {
            return (int)(Game.CurrentGame.CurrentPhase) - (int)(TurnPhase.Start);
        }
        class DrawVerifier : CardsAndTargetsVerifier
        {
            public DrawVerifier()
            {
                MaxCards = 0;
                MinPlayers = 1;
                MaxPlayers = 2;
                Discarding = true;
            }
            protected override bool VerifyPlayer(Player source, Player player)
            {
                return player != source && player.HandCards().Count > 0;
            }
        }

        class MoveVerifier : CardsAndTargetsVerifier
        {
            public MoveVerifier()
            {
                MaxCards = 0;
                MinPlayers = 2;
                MaxPlayers = 2;
                Discarding = true;
            }
            protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
            {
                if (players != null && players.Count == 2)
                {
                    if (players.All(p => p.DelayedTools().Count == 0 && p.Equipments().Count == 0))
                    {
                        return false;
                    }
                    bool hasSomethingToExchange;
                    bool delayToolsCheck = false;
                    bool equipmentCheck = false;
                    if (!players.Any(p => p.DelayedTools().Count == 0) && players[0].DelayedTools().Count == players[1].DelayedTools().Count)
                    {
                        foreach (var tool in players[0].DelayedTools())
                        {
                            if ((tool.Type as DelayedTool).DelayedToolConflicting(players[1]))
                            {
                                continue;
                            }
                            delayToolsCheck = true;
                            break;
                        }
                    }
                    else delayToolsCheck = !players.All(p => p.DelayedTools().Count == 0);
                    if (delayToolsCheck == false && !players.Any(p => p.Equipments().Count == 0) && players[0].Equipments().Count == players[1].Equipments().Count)
                    {
                        for (int i = 0; i < players[0].Equipments().Count; i++)
                        {
                            if (players[0].Equipments()[i].Type.IsCardCategory(players[1].Equipments()[i].Type.Category))
                            {
                                continue;
                            }
                            equipmentCheck = true;
                            break;
                        }
                    }
                    else equipmentCheck = true;
                    hasSomethingToExchange = delayToolsCheck || equipmentCheck;
                    return hasSomethingToExchange;
                }
                return true;
            }
        }


        class SkipOnlyVerifier : CardsAndTargetsVerifier
        {
            public SkipOnlyVerifier()
            {
                MinCards = 1;
                MaxCards = 1;
                MinPlayers = 0;
                MaxPlayers = 0;
                Discarding = true;
            }
            protected override bool VerifyCard(Player source, Card card)
            {
                return card.Place.DeckType == DeckType.Hand;
            }
        }

        protected void Draw(Player player, GameEvent gameEvent, GameEventArgs eventArgs, List<Card> cards, List<Player> players)
        {
            Game.CurrentGame.SortByOrderOfComputation(Game.CurrentGame.CurrentPlayer, players);
            StagingDeckType QiaoBianDeck = new StagingDeckType("QiaoBian");
            CardsMovement move = new CardsMovement();
            move.Helper.IsFakedMove = true;
            foreach (Player p in players)
            {
                if (p.HandCards().Count == 0) continue;
                List<List<Card>> answer;
                var places = new List<DeckPlace>() { new DeckPlace(p, DeckType.Hand) };
                if (!Game.CurrentGame.UiProxies[Owner].AskForCardChoice(new CardChoicePrompt("QiaoBian", Owner), places,
                    new List<string>() { "QiaoBian" }, new List<int>() { 1 }, new RequireOneCardChoiceVerifier(true), out answer))
                {
                    answer = new List<List<Card>>();
                    answer.Add(Game.CurrentGame.PickDefaultCardsFrom(places));
                }
                move.Cards = answer[0];
                move.To = new DeckPlace(p, QiaoBianDeck);
                Game.CurrentGame.MoveCards(move, false, Core.Utils.GameDelayTypes.None);
                Game.CurrentGame.PlayerLostCard(p, answer[0]);
            }
            move.Cards.Clear();
            move.Helper.IsFakedMove = false;
            move.To = new DeckPlace(Owner, DeckType.Hand);
            foreach (Player p in players)
            {
                move.Cards.AddRange(Game.CurrentGame.Decks[p, QiaoBianDeck]);
            }
            cards = new List<Card>(move.Cards);
            Game.CurrentGame.MoveCards(move);
            Game.CurrentGame.PlayerAcquiredCard(Owner, cards);
            Game.CurrentGame.NotificationProxy.NotifyActionComplete();
        }

        class QiaoBianMoveVerifier : ICardChoiceVerifier
        {
            public VerifierResult Verify(List<List<Card>> answer)
            {
                if ((answer != null && answer.Count > 1) || (answer != null && answer[0] != null && answer[0].Count > 1))
                {
                    return VerifierResult.Fail;
                }
                if (answer == null || answer.Count == 0 || answer[0] == null || answer[0].Count == 0)
                {
                    return VerifierResult.Partial;
                }
                Card theCard = answer[0][0];
                if (theCard.Place.DeckType == DeckType.DelayedTools)
                {
                    Player toCheck;
                    if (theCard.Place.Player == source)
                    {
                        toCheck = dest;
                    }
                    else
                    {
                        toCheck = source;
                    }
                    if (!Game.CurrentGame.PlayerCanBeTargeted(null, new List<Player>() { toCheck }, theCard))
                    {
                        return VerifierResult.Fail;
                    }
                    if ((theCard.Type as DelayedTool).DelayedToolConflicting(toCheck))
                    {
                        return VerifierResult.Fail;
                    }
                }
                if (theCard.Place.DeckType == DeckType.Equipment)
                {
                    Player toCheck;
                    if (theCard.Place.Player == source)
                    {
                        toCheck = dest;
                    }
                    else
                    {
                        toCheck = source;
                    }
                    foreach (var c in Game.CurrentGame.Decks[toCheck, DeckType.Equipment])
                    {
                        if (CardCategoryManager.IsCardCategory(c.Type.Category, theCard.Type.Category))
                        {
                            return VerifierResult.Fail;
                        }
                    }
                }
                return VerifierResult.Success;
            }

            Player source, dest;
            public QiaoBianMoveVerifier(Player src, Player dst)
            {
                source = src;
                dest = dst;
            }
            public UiHelper Helper
            {
                get { return null; }
            }
        }


        protected void Move(Player player, GameEvent gameEvent, GameEventArgs eventArgs, List<Card> cards, List<Player> players)
        {
            Game.CurrentGame.HandleCardDiscard(player, cards);
            Player source = players[0];
            Player dest = players[1];
            List<DeckPlace> places = new List<DeckPlace>();
            places.Add(new DeckPlace(source, DeckType.Equipment));
            places.Add(new DeckPlace(source, DeckType.DelayedTools));
            places.Add(new DeckPlace(dest, DeckType.Equipment));
            places.Add(new DeckPlace(dest, DeckType.DelayedTools));
            List<string> resultDeckPlace = new List<string>();
            resultDeckPlace.Add("QiaoBian");
            List<int> resultDeckMax = new List<int>();
            resultDeckMax.Add(1);
            List<List<Card>> answer;
            if (Game.CurrentGame.UiProxies[player].AskForCardChoice(new CardChoicePrompt("QiaoBian", Owner), places, resultDeckPlace, resultDeckMax, new QiaoBianMoveVerifier(source, dest), out answer))
            {
                Card theCard = answer[0][0];
                DeckType targetDeck = theCard.Place.DeckType;
                if (theCard.Place.Player == source)
                {
                    Game.CurrentGame.HandleCardTransfer(source, dest, targetDeck, new List<Card>() { theCard });
                }
                else
                {
                    Game.CurrentGame.HandleCardTransfer(dest, source, targetDeck, new List<Card>() { theCard });
                }

            }
            Game.CurrentGame.NotificationProxy.NotifyActionComplete();
        }

        Prompt GetPrompt()
        {
            return new CardUsagePrompt(string.Format("{0}.{1}", this.GetType().Name, GenerateSpecialEffectHintIndex(Owner, null) + 1), this);
        }

        public QiaoBian()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    ISkill skill;
                    List<Card> cards;
                    List<Player> players;
                    if (p.AskForCardUsage(GetPrompt(), new SkipOnlyVerifier(), out skill, out cards, out players))
                    {
                        NotifySkillUse(players);
                        Game.CurrentGame.HandleCardDiscard(p, cards);
                        if (Game.CurrentGame.CurrentPhase == TurnPhase.Judge)
                        {
                            if (p.AskForCardUsage(new CardUsagePrompt("QiaoBianDraw"), new DrawVerifier(), out skill, out cards, out players))
                            {
                                Draw(p, e, a, cards, players);
                            }
                        }
                        else if (Game.CurrentGame.CurrentPhase == TurnPhase.Draw)
                        {
                            if (p.AskForCardUsage(new CardUsagePrompt("QiaoBianMove"), new MoveVerifier(), out skill, out cards, out players))
                            {
                                Move(p, e, a, cards, players);
                            }
                        }
                        Game.CurrentGame.CurrentPhase++;
                        Game.CurrentGame.CurrentPhaseEventIndex = 2;
                        throw new TriggerResultException(TriggerResult.End);
                    }
                },
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PhaseOutEvents[TurnPhase.Judge], trigger);
            Triggers.Add(GameEvent.PhaseOutEvents[TurnPhase.Draw], trigger);
            Triggers.Add(GameEvent.PhaseOutEvents[TurnPhase.Start], trigger);
            Triggers.Add(GameEvent.PhaseOutEvents[TurnPhase.Play], trigger);
            IsAutoInvoked = null;
        }
    }
}
