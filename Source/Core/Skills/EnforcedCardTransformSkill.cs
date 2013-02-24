using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;

namespace Sanguosha.Core.Skills
{
    public abstract class EnforcedCardTransformSkill : TriggerSkill
    {
        List<DeckType> decks;

        public List<DeckType> Decks
        {
            get { return decks; }
            set { decks = value; }
        }

        protected abstract bool CardVerifier(ICard card);

        protected abstract void TransfromAction(Player Owner, ICard card);

        public EnforcedCardTransformSkill()
        {
            Decks = new List<DeckType>();
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Card != null && Decks.Contains(a.Card.Place.DeckType) && CardVerifier(a.Card); },
                (p, e, a) => { TransfromAction(p, a.Card); },
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.EnforcedCardTransform, trigger);

            var notify = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    Card card = a.Card as Card;
                    return card != null && Decks.Contains(card.HistoryPlace1.DeckType) && CardVerifier(Game.CurrentGame.OriginalCardSet[card.Id]);
                },
                (p, e, a) =>
                {
                    Game.CurrentGame.NotificationProxy.NotifyLogEvent(
                        new LogEvent("EnforcedCardTransform", Owner, Game.CurrentGame.OriginalCardSet[(a.Card as Card).Id], a.Card),
                        new List<Player> { Owner },
                        true,
                        false
                    );
                },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PlayerUsedCard, notify);
            IsEnforced = true;
        }
    }
}
