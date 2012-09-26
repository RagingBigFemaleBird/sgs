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
    }
}
