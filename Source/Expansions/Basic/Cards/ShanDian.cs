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

namespace Sanguosha.Expansions.Basic.Cards
{
    
    public class ShanDian : DelayedTool
    {
        public override void Activate(Player p, Card c)
        {
            while (true)
            {
                GameEventArgs args = new GameEventArgs();
                args.Source = null;
                args.Targets = new List<Player>() { p };
                args.Card = c;
                args.ReadonlyCard = new ReadOnlyCard(c);
                try
                {
                    Game.CurrentGame.Emit(GameEvent.CardUsageBeforeEffected, args);
                }
                catch (TriggerResultException e)
                {
                    Trace.Assert(e.Status == TriggerResult.End);
                    break;
                }
                ReadOnlyCard result = Game.CurrentGame.Judge(p, null, c);
                if (result.Suit == SuitType.Spade && result.Rank >= 2 && result.Rank <= 9)
                {
                    var roc = new ReadOnlyCard(c);
                    CardsMovement move = new CardsMovement();
                    move.cards = new List<Card>();
                    move.cards.Add(c);
                    move.to = new DeckPlace(null, DeckType.Discard);
                    Game.CurrentGame.MoveCards(move, null);
                    Game.CurrentGame.DoDamage(null, p, 3, DamageElement.Lightning, c, roc);
                    return;
                }
                break;
            }
            //todo: drive chain ShanDian cards
            List<Player> toProcess = new List<Player>(Game.CurrentGame.AlivePlayers);
            toProcess.Remove(p);
            Game.CurrentGame.SortByOrderOfComputation(p, toProcess);
            foreach (var next in toProcess)
            {
                List<Player> targets = new List<Player>();
                targets.Add(next);
                if (Game.CurrentGame.PlayerCanBeTargeted(null, targets, c))
                {
                    CardsMovement move = new CardsMovement();
                    move.cards = new List<Card>();
                    move.cards.Add(c);
                    move.to = new DeckPlace(next, DeckType.DelayedTools);
                    Game.CurrentGame.MoveCards(move, null);
                    break;
                }
            }
        }

        protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard)
        {
            throw new NotImplementedException();
        }

        public override void Process(Player source, List<Player> dests, ICard card, ReadOnlyCard readonlyCard)
        {
            Trace.Assert(dests == null || dests.Count == 0);
            AttachTo(source, source, card);
        }

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            if (DelayedToolConflicting(source))
            {
                return VerifierResult.Fail;
            }
            if (targets == null || targets.Count == 0)
            {
                return VerifierResult.Success;
            }
            return VerifierResult.Fail;
        }

    }
}
