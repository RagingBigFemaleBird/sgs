using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;

namespace Sanguosha.Expansions.Battle.Cards
{
    public class HuoGong : CardHandler
    {

        public class HuoGongCardChoiceVerifier : ICardUsageVerifier
        {
            public VerifierResult Verify(ISkill skill, List<Card> cards, List<Player> players)
            {
                if (skill != null || cards == null || cards.Count != 1 || (players != null && players.Count != 0))
                {
                    return VerifierResult.Fail;
                }
                if (cards[0].Place.DeckType != DeckType.Hand)
                {
                    return VerifierResult.Fail;
                }
                return VerifierResult.Success;
            }


            public IList<CardHandler> AcceptableCardType
            {
                get { return null; }
            }
        }

        public class HuoGongCardMatchVerifier : ICardUsageVerifier
        {
            private SuitType suit;

            public SuitType Suit
            {
                get { return suit; }
                set { suit = value; }
            }
            public HuoGongCardMatchVerifier(SuitType s)
            {
                suit = s;
            }

            public VerifierResult Verify(ISkill skill, List<Card> cards, List<Player> players)
            {
                if (skill != null || cards == null || cards.Count != 1 || (players != null && players.Count != 0))
                {
                    return VerifierResult.Fail;
                }
                if (cards[0].Suit != suit)
                {
                    return VerifierResult.Fail;
                }
                if (cards[0].Place.DeckType != DeckType.Hand)
                {
                    return VerifierResult.Fail;
                }
                return VerifierResult.Success;
            }


            public IList<CardHandler> AcceptableCardType
            {
                get { return null; }
            }
        }

        protected override void Process(Player source, Player dest, ICard card)
        {
            IUiProxy ui = Game.CurrentGame.UiProxies[dest];
            HuoGongCardChoiceVerifier v1 = new HuoGongCardChoiceVerifier();
            ISkill s;
            List<Player> p;
            List<Card> cards;
            if (!ui.AskForCardUsage("HuoGong", v1, out s, out cards, out p))
            {
                Trace.TraceInformation("Player {0} Invalid answer", dest);
                cards = new List<Card>();
                cards.Add(Game.CurrentGame.Decks[dest, DeckType.Hand][0]);
            }
            Trace.TraceInformation("Player {0} HuoGong showed {1}, ", dest.Id, cards[0].Suit);
            ui = Game.CurrentGame.UiProxies[source];
            HuoGongCardMatchVerifier v2 = new HuoGongCardMatchVerifier(cards[0].Suit);
            if (ui.AskForCardUsage("Choose your card for HuoGong", v2, out s, out cards, out p))
            {
                CardsMovement m;
                m.cards = cards;
                m.to = new DeckPlace(null, DeckType.Discard);
                Game.CurrentGame.MoveCards(m, new CardUseLog() { Source = source, Targets = null, Cards = null, Type = this });
                Game.CurrentGame.DoDamage(source, dest, 1, DamageElement.Fire, card);
            }
            else
            {
                Trace.TraceInformation("HuoGong aborted, failed to provide card");
            }
        }

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            if (targets.Count > 1)
            {
                return VerifierResult.Fail;
            }
            Player player = targets[0];

            if (Game.CurrentGame.Decks[player, DeckType.Hand].Count == 0)
            {                
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public override CardCategory Category
        {
            get { return CardCategory.ImmediateTool; }
        }
    }
}
