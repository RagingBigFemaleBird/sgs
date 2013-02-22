using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;

namespace Sanguosha.Expansions.Basic.Cards
{
    
    public class CardWrapper : CardTransformSkill
    {
        public override VerifierResult TryTransform(List<Card> cards, object arg, out CompositeCard card)
        {
            card = new CompositeCard();
            card.Type = handler;
            card.Subcards = new List<Card>(cards);
            card.Owner = Owner;
            return VerifierResult.Success;
        }

        CardHandler handler;
        bool withoutNotify;

        public CardWrapper(Player p, CardHandler h, bool Notify = true)
        {
            Owner = p;
            handler = h;
            withoutNotify = !Notify;
        }

        public override void NotifyAction(Player source, List<Player> targets, CompositeCard card)
        {
            if (withoutNotify) return;
            ActionLog log = new ActionLog();
            log.GameAction = GameAction.None;
            log.CardAction = card;
            log.Source = source;
            log.SpecialEffectHint = GenerateSpecialEffectHintIndex(source, targets, card);
            Game.CurrentGame.NotificationProxy.NotifySkillUse(log);
        }
    }
}
