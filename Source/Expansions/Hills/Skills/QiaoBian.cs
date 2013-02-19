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
                MinCards = 1;
                MaxCards = 1;
                MinPlayers = 0;
                MaxPlayers = 2;
                Discarding = true;
            }
            protected override bool VerifyCard(Player source, Card card)
            {
                return card.Place.DeckType == DeckType.Hand;
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
                MinCards = 1;
                MaxCards = 1;
                MinPlayers = 2;
                MaxPlayers = 2;
                Discarding = true;
            }
            protected override bool VerifyCard(Player source, Card card)
            {
                return card.Place.DeckType == DeckType.Hand;
            }
            protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
            {
                if (players != null && players.Count == 2)
                {
                    bool hasSomethingToExchange = false;
                    foreach (var p in players)
                    {
                        if (Game.CurrentGame.Decks[p, DeckType.DelayedTools].Count != 0 || Game.CurrentGame.Decks[p, DeckType.Equipment].Count != 0)
                        {
                            hasSomethingToExchange = true;
                        }
                    }
                    if (!hasSomethingToExchange) return false;
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
            Game.CurrentGame.HandleCardDiscard(player, cards);
            Game.CurrentGame.EnterAtomicContext();
            foreach (Player p in players)
            {
                List<List<Card>> answer;
                var places = new List<DeckPlace>() { new DeckPlace(p, DeckType.Hand) };
                if (!Game.CurrentGame.UiProxies[Owner].AskForCardChoice(new CardChoicePrompt("QiaoBian", Owner), places,
                    new List<string>() { "QiaoBian" }, new List<int>() { 1 }, new RequireOneCardChoiceVerifier(true), out answer))
                {
                    answer = new List<List<Card>>();
                    answer.Add(Game.CurrentGame.PickDefaultCardsFrom(places));
                }
                Game.CurrentGame.HandleCardTransferToHand(p, Owner, answer[0]);
            }
            Game.CurrentGame.ExitAtomicContext();
            Game.CurrentGame.CurrentPhase++;
            Game.CurrentGame.CurrentPhaseEventIndex = 2;
            Game.CurrentGame.NotificationProxy.NotifyActionComplete();
            throw new TriggerResultException(TriggerResult.End);
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

            Game.CurrentGame.CurrentPhase++;
            Game.CurrentGame.CurrentPhaseEventIndex = 2;
            throw new TriggerResultException(TriggerResult.End);
        }


        public QiaoBian()
        {
            var trigger = new AutoNotifyUsagePassiveSkillTrigger(
                this,
                Draw,
                TriggerCondition.OwnerIsSource,
                new DrawVerifier()
            );
            var trigger2 = new AutoNotifyUsagePassiveSkillTrigger(
                this,
                (p, e, a, cards, players) =>
                {
                    Game.CurrentGame.HandleCardDiscard(p, cards);
                    Game.CurrentGame.CurrentPhase++;
                    Game.CurrentGame.CurrentPhaseEventIndex = 2;
                    throw new TriggerResultException(TriggerResult.End);
                },
                TriggerCondition.OwnerIsSource,
                new SkipOnlyVerifier()
            );
            var trigger3 = new AutoNotifyUsagePassiveSkillTrigger(
                this,
                Move,
                TriggerCondition.OwnerIsSource,
                new MoveVerifier()
            );
            Triggers.Add(GameEvent.PhaseOutEvents[TurnPhase.Judge], trigger);
            Triggers.Add(GameEvent.PhaseOutEvents[TurnPhase.Draw], trigger3);
            Triggers.Add(GameEvent.PhaseOutEvents[TurnPhase.Start], trigger2);
            Triggers.Add(GameEvent.PhaseOutEvents[TurnPhase.Play], trigger2);
            IsAutoInvoked = null;
        }
    }
}
