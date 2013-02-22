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

namespace Sanguosha.Expansions.Wind.Skills
{
    /// <summary>
    /// 涉猎-摸牌阶段，你可以放弃摸牌，改为从牌堆顶亮出五张牌，你获得不同花色的牌各一张，将其余的牌置入弃牌堆。
    /// </summary>
    public class SheLie : TriggerSkill
    {
        class SheLieVerifier : ICardChoiceVerifier
        {
            public VerifierResult Verify(List<List<Card>> answer)
            {
                HashSet<SuitType> choice = new HashSet<SuitType>();
                if (answer == null || answer.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                if (answer[0] == null || answer[0].Count == 0)
                {
                    return VerifierResult.Partial;
                }
                foreach (var c in answer[0])
                {
                    if (choice.Contains(c.Suit))
                    {
                        return VerifierResult.Fail;
                    }
                    choice.Add(c.Suit);
                }
                foreach (var c in cards)
                {
                    if (!choice.Contains(c.Suit))
                    {
                        return VerifierResult.Partial;
                    }
                }
                return VerifierResult.Success;
            }

            List<Card> cards;
            public SheLieVerifier(List<Card> c)
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
            DeckType shelieDeck = new DeckType("SheLie");

            CardsMovement move = new CardsMovement();
            move.Cards = new List<Card>();
            for (int i = 0; i < 5; i++)
            {
                Game.CurrentGame.SyncImmutableCardAll(Game.CurrentGame.PeekCard(0));
                Card c = Game.CurrentGame.DrawCard();
                move.Cards.Add(c);
            }
            move.To = new DeckPlace(null, shelieDeck);
            Game.CurrentGame.MoveCards(move);
            List<List<Card>> answer;
            if (!Game.CurrentGame.UiProxies[Owner].AskForCardChoice(new CardChoicePrompt("SheLie", Owner),
                    new List<DeckPlace>() { new DeckPlace(null, shelieDeck) },
                    new List<string>() { "SheLie" },
                    new List<int>() { 4 },
                    new SheLieVerifier(Game.CurrentGame.Decks[null, shelieDeck]),
                    out answer,
                    null,
                    CardChoiceCallback.GenericCardChoiceCallback))
            {
                Trace.TraceInformation("Invalid answer for SheLie, choosing for you");
                answer = new List<List<Card>>();
                answer.Add(new List<Card>());
                HashSet<SuitType> choice = new HashSet<SuitType>();
                foreach (Card c in Game.CurrentGame.Decks[null, shelieDeck])
                {
                    if (!choice.Contains(c.Suit))
                    {
                        answer[0].Add(c);
                        choice.Add(c.Suit);
                    }
                }
            }

            Game.CurrentGame.HandleCardTransferToHand(null, Owner, answer[0]);
            foreach (var c in Game.CurrentGame.Decks[null, shelieDeck])
            {
                c.Log.SkillAction = this;
                c.Log.GameAction = GameAction.PlaceIntoDiscard;
            }
            Game.CurrentGame.PlaceIntoDiscard(null, new List<Card>(Game.CurrentGame.Decks[null, shelieDeck]));
            Game.CurrentGame.CurrentPhaseEventIndex++;
            throw new TriggerResultException(TriggerResult.End);
        }


        public SheLie()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Draw], trigger);
        }

    }
}
