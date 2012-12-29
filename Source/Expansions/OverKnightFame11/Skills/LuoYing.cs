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
            int answer = 0;
            var cardsToProcess = from c in eventArgs.Cards
                                 where c.Suit == SuitType.Club
                                 select c;
            foreach (var c in cardsToProcess)
            {
                CardsMovement temp = new CardsMovement();
                temp.Cards = new List<Card>(cardsToProcess);
                temp.To = new DeckPlace(null, DeckType.Discard);
                Game.CurrentGame.NotificationProxy.NotifyCardMovement(new List<CardsMovement>() { temp });
                foreach (Card cc in cardsToProcess)
                {
                    cc.PlaceOverride = new DeckPlace(null, DeckType.Discard);
                }
                break;
            }
            foreach (var c in cardsToProcess)
            {
                var prompt = new MultipleChoicePrompt("LuoYing", c);
                if (Owner.AskForMultipleChoice(prompt, Prompt.YesNoChoices, out answer) && answer == 1)
                {
                    NotifySkillUse();
                    Game.CurrentGame.HandleCardTransferToHand(null, Owner, new List<Card>() { c });
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
            IsAutoInvoked = true;
        }
    }
}
