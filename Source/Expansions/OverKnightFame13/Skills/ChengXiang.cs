using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using System.Diagnostics;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.OverKnightFame13.Skills
{
    /// <summary>
    /// 称象-每当你受到一次伤害后，你可以展示所有手牌，若点数之和小于13，你摸一张牌。你可以重复此流程，直至你的所有手牌点数之和等于或大于13为止。
    /// </summary>
    public class ChengXiang : TriggerSkill
    {
        class DaXiangVerifier : ICardChoiceVerifier
        {
            public VerifierResult Verify(List<List<Card>> answer)
            {
                if (answer == null || answer.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                if (answer[0] == null || answer[0].Count == 0)
                {
                    return VerifierResult.Partial;
                }
                if (answer[0].Sum(c => c.Rank) >= 13) return VerifierResult.Fail;
                return VerifierResult.Success;
            }

            List<Card> cards;
            public DaXiangVerifier(List<Card> c)
            {
                cards = new List<Card>(c);
            }
            public UiHelper Helper
            {
                get { return new UiHelper() { ShowToAll = true }; }
            }
        }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            DeckType daXiangDeck = DeckType.Register("DaXiang");

            CardsMovement move = new CardsMovement();
            move.Cards = new List<Card>();
            for (int i = 0; i < 4; i++)
            {
                Game.CurrentGame.SyncImmutableCardAll(Game.CurrentGame.PeekCard(0));
                Card c = Game.CurrentGame.DrawCard();
                move.Cards.Add(c);
            }
            move.To = new DeckPlace(null, daXiangDeck);
            Game.CurrentGame.MoveCards(move);
            List<List<Card>> answer;
            if (!Game.CurrentGame.UiProxies[Owner].AskForCardChoice(new CardChoicePrompt("DaXiang", Owner),
                    new List<DeckPlace>() { new DeckPlace(null, daXiangDeck) },
                    new List<string>() { "DaXiang" },
                    new List<int>() { 4 },
                    new DaXiangVerifier(Game.CurrentGame.Decks[null, daXiangDeck]),
                    out answer,
                    null,
                    CardChoiceCallback.GenericCardChoiceCallback))
            {
                Trace.TraceInformation("Invalid answer for DaXiang, choosing for you");
                answer = new List<List<Card>>();
                answer.Add(new List<Card>());
                HashSet<SuitType> choice = new HashSet<SuitType>();
                foreach (Card c in Game.CurrentGame.Decks[null, daXiangDeck])
                {
                    if (!choice.Contains(c.Suit))
                    {
                        answer[0].Add(c);
                        choice.Add(c.Suit);
                    }
                }
            }

            Game.CurrentGame.HandleCardTransferToHand(null, Owner, answer[0]);
            foreach (var c in Game.CurrentGame.Decks[null, daXiangDeck])
            {
                c.Log.SkillAction = this;
                c.Log.GameAction = GameAction.PlaceIntoDiscard;
            }
            Game.CurrentGame.PlaceIntoDiscard(null, new List<Card>(Game.CurrentGame.Decks[null, daXiangDeck]));
            Game.CurrentGame.CurrentPhaseEventIndex++;
            throw new TriggerResultException(TriggerResult.End);
        }
        
        public ChengXiang()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.OwnerIsTarget
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.AfterDamageInflicted, trigger);
        }
    }
}