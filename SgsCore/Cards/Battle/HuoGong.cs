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

namespace Sanguosha.Core.Cards.Battle
{

    public class HuoGong : CardHandler
    {
        public override VerifierResult Verify(Skill skill, List<Card> cards, List<Player> players)
        {
            Trace.Assert(skill == null);
            if (cards == null || cards.Count != 1)
            {
                return VerifierResult.Fail;
            }
            Card card = cards[0];
            if (card.Type != CardType)
            {
                return VerifierResult.Fail;
            }
            if (players == null || players.Count == 0)
            {
                return VerifierResult.Partial;
            }
            else if (players.Count > 1)
            {
                return VerifierResult.Fail;
            }
            Player player = players[0];
            DeckPlace dp = card.Place;
            int index = Game.CurrentGame.Decks[dp].IndexOf(card);
            Game.CurrentGame.Decks[dp].Remove(card);
            if (Game.CurrentGame.Decks[player, DeckType.Hand].Count == 0)
            {
                Game.CurrentGame.Decks[dp].Insert(index, card);
                return VerifierResult.Fail;
            }
            try
            {
                Game.CurrentGame.Emit(GameEvent.PlayerCanBeTargeted, new Triggers.GameEventArgs() {Source = Game.CurrentGame.CurrentPlayer, Target = player, Cards = cards});
            }
            catch(TriggerResultException e)
            {
                Trace.Assert(e.Status == TriggerResult.Fail);
                Game.CurrentGame.Decks[dp].Insert(index, card);
                return VerifierResult.Fail;
            }

            Game.CurrentGame.Decks[dp].Insert(index, card);
            return VerifierResult.Success;
        }

        public class HuoGongCardChoiceVerifier : ICardUsageVerifier
        {
            public VerifierResult Verify(Skill skill, List<Card> cards, List<Player> players)
            {
                if (skill != null || cards == null || cards.Count != 1 || (players != null && players.Count != 0))
                {
                    return VerifierResult.Fail;
                }
                return VerifierResult.Success;
            }
        }

        public class HuoGongCardMatchVerifier : ICardUsageVerifier
        {
            private Cards.SuitType suit;

            public Cards.SuitType Suit
            {
                get { return suit; }
                set { suit = value; }
            }
            public HuoGongCardMatchVerifier(Cards.SuitType s)
            {
                suit = s;
            }

            public VerifierResult Verify(Skill skill, List<Card> cards, List<Player> players)
            {
                if (skill != null || cards == null || cards.Count != 1 || (players != null && players.Count != 0))
                {
                    return VerifierResult.Fail;
                }
                if (cards[0].Suit != suit)
                {
                    return VerifierResult.Fail;
                }
                return VerifierResult.Success;
            }
        }

        protected override void Process(Player source, Player dest)
        {
            IUiProxy ui = Game.CurrentGame.UiProxies[dest];
            HuoGongCardChoiceVerifier v1 = new HuoGongCardChoiceVerifier();
            Skill s;
            List<Player> p;
            List<Card> cards;
            ui.AskForCardUsage("HuoGong", v1, out s, out cards, out p);
            Trace.TraceInformation("Player {0} HuoGong showed {1}, ", dest.Id, cards[0].Suit);
            ui = Game.CurrentGame.UiProxies[source];
            HuoGongCardMatchVerifier v2 = new HuoGongCardMatchVerifier(cards[0].Suit);
            ui.AskForCardUsage("Choose your card for HuoGong", v2, out s, out cards, out p);
            if (cards != null)
            {
                CardsMovement m;
                m.cards = cards;
                m.to = new DeckPlace(null, DeckType.Discard);
                Game.CurrentGame.MoveCards(m);
                Game.CurrentGame.DoDamage(source, dest, 1, Games.DamageElement.Fire, Game.CurrentGame.Decks[DeckType.Compute]);
            }
            else
            {
                Trace.TraceInformation("HuoGong aborted, failed to provide card");
            }
        }
    }
}
