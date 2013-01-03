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
    /// 固政-其他角色的弃牌阶段结束时，你可将此阶段中弃置的一张牌从弃牌堆返回该角色手牌，然后你可以获得弃牌堆里其余于此阶段中弃置的牌。
    /// </summary>
    public class GuZheng : TriggerSkill
    {
        List<Card> GuZhengCards;

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            var GuZhengDeck = new DeckType("GuZheng");
            foreach (Card c in new List<Card>(GuZhengCards))
            {
                if (c.Place.DeckType != DeckType.Discard)
                {
                    GuZhengCards.Remove(c);
                }
            }
            CardsMovement move = new CardsMovement();
            move.Cards = GuZhengCards;
            move.To = new DeckPlace(null, GuZhengDeck);
            Game.CurrentGame.MoveCards(move);
            List<List<Card>> answer;
            var options = new AdditionalCardChoiceOptions() { Options = new List<OptionPrompt>() { new OptionPrompt("GuZhengHuoDe"), new OptionPrompt("GuZhengBuHuoDe") } };
            if (!Game.CurrentGame.UiProxies[Owner].AskForCardChoice(
                new CardChoicePrompt("GuZheng", Owner, Game.CurrentGame.CurrentPlayer),
                new List<DeckPlace>() { new DeckPlace(null, GuZhengDeck) },
                new List<string>() { "GuZhengFanHui" },
                new List<int>() { 1 },
                new RequireOneCardChoiceVerifier(),
                out answer,
                options))
            {
                Trace.TraceInformation("Invalid answer, choosing for you");
                answer = new List<List<Card>>();
                answer.Add(new List<Card>());
                answer[0].Add(Game.CurrentGame.Decks[null, GuZhengDeck][0]);
            }
            move = new CardsMovement();
            move.Cards = new List<Card>(answer[0]);
            move.To = new DeckPlace(Game.CurrentGame.CurrentPlayer, DeckType.Hand);
            Game.CurrentGame.MoveCards(move);
            Game.CurrentGame.PlayerAcquiredCard(Game.CurrentGame.CurrentPlayer, answer[0]);

            if (options.OptionResult == 1)
            {
                Game.CurrentGame.PlaceIntoDiscard(null, new List<Card>(Game.CurrentGame.Decks[null, GuZhengDeck]));
            }
            else
            {
                var cardsToAcquire = new List<Card>(Game.CurrentGame.Decks[null, GuZhengDeck]);
                move = new CardsMovement();
                move.Cards = new List<Card>(cardsToAcquire);
                move.To = new DeckPlace(Owner, DeckType.Hand);
                Game.CurrentGame.MoveCards(move);
                Game.CurrentGame.PlayerAcquiredCard(Owner, cardsToAcquire);
            }
            GuZhengCards = new List<Card>();
        }

        public GuZheng()
        {
            GuZhengCards = new List<Card>();
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return Game.CurrentGame.CurrentPlayer != p && Game.CurrentGame.CurrentPhase == TurnPhase.Discard; },
                (p, e, a) => { GuZhengCards.AddRange(a.Cards); },
                TriggerCondition.Global
            ) { IsAutoNotify = false, AskForConfirmation = false };
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return GuZhengCards.Count > 0 && Game.CurrentGame.CurrentPlayer != p; },
                Run,
                TriggerCondition.Global
            ) { Priority = 1 };
            var trigger3 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { GuZhengCards = new List<Card>(); },
                TriggerCondition.Global
            ) { Priority = 0, AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.CardsEnteredDiscardDeck, trigger);
            Triggers.Add(GameEvent.PhaseEndEvents[TurnPhase.Discard], trigger2);
            Triggers.Add(GameEvent.PhasePostEnd, trigger3);
        }
    }
}
