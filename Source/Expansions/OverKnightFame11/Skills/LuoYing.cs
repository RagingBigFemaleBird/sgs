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
            if (eventArgs.Source == null || eventArgs.Source == Owner || ((DiscardReason)eventArgs.IntArg != DiscardReason.Discard && (DiscardReason)eventArgs.IntArg != DiscardReason.Judge))
            {
                return;
            }
            int answer = 0;
            List<Card> cardsToProcess = new List<Card>(eventArgs.Cards);
            foreach (Card c in cardsToProcess)
            {
                if (c.Suit == SuitType.Club)
                {
                    CardsMovement temp = new CardsMovement();
                    temp.cards = new List<Card>(cardsToProcess);
                    temp.to = new DeckPlace(null, DeckType.Discard);
                    Game.CurrentGame.NotificationProxy.NotifyCardMovement(new List<CardsMovement>() { temp }, new List<IGameLog>());
                    foreach (Card cc in cardsToProcess)
                    {
                        cc.PlaceOverride = new DeckPlace(null, DeckType.Discard);
                    }
                    break;
                }
            }
            foreach (Card c in cardsToProcess)
            {
                var prompt = new MultipleChoicePrompt("LuoYing", c);
                if (c.Suit == SuitType.Club &&
                    Game.CurrentGame.UiProxies[Owner].AskForMultipleChoice(prompt, Prompt.YesNoChoices, out answer) && answer == 0)
                {
                    ActionLog log = new ActionLog();
                    log.GameAction = GameAction.None;
                    log.SkillAction = this;
                    log.Source = Owner;
                    log.CardAction = c;
                    Game.CurrentGame.NotificationProxy.NotifySkillUse(log); 
                    List<Card> cc = new List<Card>();
                    cc.Add(c);
                    Game.CurrentGame.HandleCardTransferToHand(null, Owner, cc);
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
