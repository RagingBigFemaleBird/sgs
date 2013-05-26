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
        bool RecursiveShanDianDriver(Player start, Player current, Card c)
        {
            List<Player> toProcess = new List<Player>(Game.CurrentGame.AlivePlayers);
            toProcess.Remove(current);
            Game.CurrentGame.SortByOrderOfComputation(current, toProcess);
            foreach (var next in toProcess)
            {
                List<Player> targets = new List<Player>();
                targets.Add(next);
                if (next == start) return false;
                if (Game.CurrentGame.PlayerCanBeTargeted(null, targets, c))
                {
                    if (DelayedToolConflicting(next))
                    {
                        Card nextCard = null;
                        foreach (var card in Game.CurrentGame.Decks[next, DeckType.DelayedTools])
                        {
                            if (card.Type is ShanDian)
                            {
                                nextCard = card;
                                break;
                            }
                        }
                        Trace.Assert(nextCard != null);
                        if (!RecursiveShanDianDriver(start, next, nextCard))
                        {
                            return false;
                        }
                    }
                    CardsMovement move = new CardsMovement();
                    move.Cards = new List<Card>();
                    move.Cards.Add(c);
                    move.To = new DeckPlace(next, DeckType.DelayedTools);
                    Game.CurrentGame.MoveCards(move);
                    return true;
                }
            }
            return false;
        }

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
                ReadOnlyCard result = Game.CurrentGame.Judge(p, null, c, (judgeResultCard) => { return judgeResultCard.Suit == SuitType.Spade && judgeResultCard.Rank >= 2 && judgeResultCard.Rank <= 9; });
                if (result.Suit == SuitType.Spade && result.Rank >= 2 && result.Rank <= 9)
                {
                    var roc = new ReadOnlyCard(c);
                    CardsMovement move = new CardsMovement();
                    move.Cards = new List<Card>();
                    move.Cards.Add(c);
                    move.To = new DeckPlace(null, DeckType.Discard);
                    Game.CurrentGame.MoveCards(move);
                    Game.CurrentGame.DoDamage(null, p, 3, DamageElement.Lightning, c, roc);
                    return;
                }
                break;
            }
            RecursiveShanDianDriver(p, p, c);
        }

        protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs inResponseTo)
        {
            throw new NotImplementedException();
        }

        public override void Process(GameEventArgs handlerArgs)
        {
            var source = handlerArgs.Source;
            var dests = handlerArgs.Targets;
            var readonlyCard = handlerArgs.ReadonlyCard;
            var inResponseTo = handlerArgs.InResponseTo;
            var card = handlerArgs.Card;
            Trace.Assert(dests == null || dests.Count == 0);
            AttachTo(source, source, card);
        }

        public override VerifierResult Verify(Player source, ICard card, List<Player> targets, bool isLooseVerify)
        {
            if (DelayedToolConflicting(source))
            {
                return VerifierResult.Fail;
            }
            if (!Game.CurrentGame.PlayerCanBeTargeted(source, new List<Player>() { source }, card))
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
