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

namespace Sanguosha.Expansions.SP.Skills
{
    public class AoCai : CardTransformSkill
    {
        public override VerifierResult TryTransform(List<Card> cards, List<Player> arg, out CompositeCard card, bool isPlay)
        {
            card = new CompositeCard();
            card.Subcards = new List<Card>();
            if (cards == null || cards.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (cards != null && cards.Count > 1)
            {
                return VerifierResult.Fail;
            }
            if (!(cards[0] == Game.CurrentGame.Decks[null, DeckType.Dealing][0] || cards[0] == Game.CurrentGame.Decks[null, DeckType.Dealing][1]))
            {
                return VerifierResult.Fail;
            }
            card.Type = cards[0].Type;
            return VerifierResult.Success;
        }

        public AoCai()
        {
            LinkedPassiveSkill = new AoCaiPassive();
            Helper.OtherGlobalCardDeckUsed.Add(new DeckPlace(null, DeckType.Dealing), 2);
        }

        public CardHandler AdditionalType { get; set; }

        public class AoCaiPassive : TriggerSkill
        {
            public AoCaiPassive()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) =>
                    {
                        return true;
                    },
                    (p, e, a) =>
                    {
                        //ensure we have two cards in the dealing deck
                        List<Card> cards = new List<Card>();
                        cards.Add(Game.CurrentGame.PeekCard(0));
                        cards.Add(Game.CurrentGame.PeekCard(1));
                        Game.CurrentGame.SyncImmutableCards(p, cards);
                    },
                    TriggerCondition.OwnerIsSource
                ) { AskForConfirmation = false, IsAutoNotify = false };
                Triggers.Add(GameEvent.PlayerIsAboutToPlayCard, trigger);
                Triggers.Add(GameEvent.PlayerIsAboutToUseCard, trigger);
            }
        }
    }
}
