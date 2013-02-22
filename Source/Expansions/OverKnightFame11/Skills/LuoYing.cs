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

namespace Sanguosha.Expansions.OverKnightFame11.Skills
{
    /// <summary>
    /// 落英-当其他角色的梅花牌，因弃牌或判定而进入弃牌堆时，你可以获得之。
    /// </summary>
    public class LuoYing : TriggerSkill
    {
        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            var args = eventArgs as DiscardCardEventArgs;
            if (args.Source == null || args.Source == Owner || (args.Reason != DiscardReason.Discard && args.Reason != DiscardReason.Judge))
            {
                return;
            }
            var cardsToProcess = new List<Card>(
                                 from c in eventArgs.Cards
                                 where c.Suit == SuitType.Club
                                 select c);
            if (cardsToProcess.Count() > 0)
            {
                CardsMovement temp = new CardsMovement();
                temp.Cards = new List<Card>(cardsToProcess);
                temp.To = new DeckPlace(null, DeckType.Discard);
                foreach (Card cc in cardsToProcess)
                {
                    cc.PlaceOverride = new DeckPlace(null, DeckType.Discard);
                }
                Game.CurrentGame.NotificationProxy.NotifyCardMovement(new List<CardsMovement>() { temp });
            }
            else return;
            List<OptionPrompt> prompts = new List<OptionPrompt>();
            if (cardsToProcess.Count > 1)
            {
                prompts.Add(OptionPrompt.NoChoice);
                prompts.Add(new OptionPrompt("LuoYingQuanBu"));
                prompts.Add(new OptionPrompt("LuoYingBuFen"));
            }
            else prompts.AddRange(OptionPrompt.YesNoChoices);
            int choiceIndex = 0;
            Owner.AskForMultipleChoice(new MultipleChoicePrompt(Prompt.SkillUseYewNoPrompt, this), prompts, out choiceIndex);
            if (choiceIndex == 0) return;
            if (choiceIndex == 1) NotifySkillUse();
            foreach (var c in cardsToProcess)
            {
                var prompt = new MultipleChoicePrompt("LuoYing", c);
                int answer = 0;
                if (choiceIndex == 1 || Owner.AskForMultipleChoice(prompt, Prompt.YesNoChoices, out answer) && answer == 1)
                {
                    if (choiceIndex == 2) NotifySkillUse();
                    c.Log = new ActionLog();
                    c.Log.SkillAction = this;
                    CardsMovement temp = new CardsMovement();
                    temp.Cards.Add(c);
                    temp.To = new DeckPlace(null, new DeckType("LuoYing"));
                    c.PlaceOverride = new DeckPlace(null, DeckType.Discard);
                    Game.CurrentGame.NotificationProxy.NotifyCardMovement(new List<CardsMovement>() { temp });
                    c.Log = new ActionLog();
                    Game.CurrentGame.HandleCardTransferToHand(c.Owner, Owner, new List<Card>() { c }, new MovementHelper() { IsFakedMove = true, AlwaysShowLog = true });
                    eventArgs.Cards.Remove(c);
                }
            }
        }

        public LuoYing()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.Global
            ) { IsAutoNotify = false, AskForConfirmation = false };
            Triggers.Add(GameEvent.CardsEnteringDiscardDeck, trigger);
            IsAutoInvoked = null;
        }
    }
}
