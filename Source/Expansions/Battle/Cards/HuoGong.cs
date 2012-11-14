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
    [Serializable]
    public class HuoGong : CardHandler
    {

        public class HuoGongCardChoiceVerifier : CardUsageVerifier
        {
            public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
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


            public override IList<CardHandler> AcceptableCardType
            {
                get { return null; }
            }
        }

        public class HuoGongCardMatchVerifier : CardUsageVerifier
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

            public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                if (skill != null || (players != null && players.Count != 0))
                {
                    return VerifierResult.Fail;
                }
                if (cards != null && cards.Count > 1)
                {
                    return VerifierResult.Fail;
                }
                if (cards == null || cards.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                if (!Game.CurrentGame.PlayerCanDiscardCard(Owner, cards[0]))
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
            public Player Owner { get; set; }

            public override IList<CardHandler> AcceptableCardType
            {
                get { return null; }
            }
        }

        protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard)
        {
            IUiProxy ui = Game.CurrentGame.UiProxies[dest];
            HuoGongCardChoiceVerifier v1 = new HuoGongCardChoiceVerifier();
            ISkill s;
            List<Player> p;
            List<Card> cards;
            if (dest.IsDead) return;
            if (!ui.AskForCardUsage(new CardUsagePrompt("HuoGong", source), v1, out s, out cards, out p))
            {
                Trace.TraceInformation("Player {0} Invalid answer", dest);
                cards = new List<Card>();
                if (Game.CurrentGame.Decks[dest, DeckType.Hand].Count == 0)
                {
                    Trace.TraceError("HuoGong Cannot Show Card! This should NOT have happened!");
                    return;
                }
                cards.Add(Game.CurrentGame.Decks[dest, DeckType.Hand][0]);
            }
            Trace.TraceInformation("Player {0} HuoGong showed {1}, ", dest.Id, cards[0].Suit);
            if (source.IsDead) return;
            ui = Game.CurrentGame.UiProxies[source];
            HuoGongCardMatchVerifier v2 = new HuoGongCardMatchVerifier(cards[0].Suit);
            v2.Owner = source;
            if (ui.AskForCardUsage(new CardUsagePrompt("HuoGong2", dest, cards[0].Suit), v2, out s, out cards, out p))
            {
                Game.CurrentGame.HandleCardDiscard(source, cards);
                Game.CurrentGame.DoDamage(source, dest, 1, DamageElement.Fire, card, readonlyCard);
            }
            else
            {
                Trace.TraceInformation("HuoGong aborted, failed to provide card");
            }
        }

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            Trace.Assert(targets != null);
            if (targets != null && targets.Count > 1)
            {
                return VerifierResult.Fail;
            }
            if (targets == null || targets.Count == 0)
            {
                return VerifierResult.Partial;
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
