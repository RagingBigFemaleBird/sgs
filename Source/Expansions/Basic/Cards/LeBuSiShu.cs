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
    
    public class LeBuSiShu : DelayedTool
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
                ReadOnlyCard result = Game.CurrentGame.Judge(p, null, c, (judgeResultCard) => { return judgeResultCard.Suit != SuitType.Heart; });
                if (result.Suit != SuitType.Heart)
                {
                    var theTrigger = new LeBuSiShuTrigger() { Priority = int.MaxValue };
                    theTrigger.Owner = p;
                    Game.CurrentGame.RegisterTrigger(GameEvent.PhaseOutEvents[TurnPhase.Draw], theTrigger);
                }
                break;
            }
            CardsMovement move = new CardsMovement();
            move.cards = new List<Card>();
            move.cards.Add(c);
            move.to = new DeckPlace(null, DeckType.Discard);
            Game.CurrentGame.MoveCards(move, null);
        }

        protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard)
        {
            throw new NotImplementedException();
        }

        public override void Process(Player source, List<Player> dests, ICard card, ReadOnlyCard readonlyCard)
        {
            Trace.Assert(dests.Count == 1);
            AttachTo(source, dests[0], card);
        }

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            if (targets != null && targets.Count != 0 &&
                (targets.Count > 1 || DelayedToolConflicting(targets[0]) || targets[0] == source))
            {
                return VerifierResult.Fail;
            }
            if (targets == null || targets.Count == 0)
            {
                return VerifierResult.Partial;
            }
            return VerifierResult.Success;
        }

        private class LeBuSiShuTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (Owner == eventArgs.Source)
                {
                    Game.CurrentGame.CurrentPhase++;
                    Game.CurrentGame.CurrentPhaseEventIndex = 2;
                    Game.CurrentGame.UnregisterTrigger(GameEvent.PhaseOutEvents[TurnPhase.Draw], this);
                    throw new TriggerResultException(TriggerResult.End);
                }
            }
        }
    }
}
