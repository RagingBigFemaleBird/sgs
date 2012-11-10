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
    public class BingLiangCunDuan : DelayedTool
    {
        public override void Activate(Player p, Card c)
        {
            Player nullPlayer = null;
            if (PlayerIsCardTargetCheck(ref nullPlayer, ref p, c))
            {
                ReadOnlyCard result = Game.CurrentGame.Judge(p, null, c);
                if (result.Suit != SuitType.Club)
                {
                    var theTrigger = new BingLiangCunDuanTrigger() { Priority = int.MaxValue };
                    theTrigger.Owner = p;
                    Game.CurrentGame.RegisterTrigger(GameEvent.PhaseOutEvents[TurnPhase.Judge], theTrigger);
                }
            }
            CardsMovement move = new CardsMovement();
            move.cards = new List<Card>();
            move.cards.Add(c);
            move.to = new DeckPlace(null, DeckType.Discard);
            Game.CurrentGame.MoveCards(move, null);
        }

        protected override void Process(Player source, Player dest, ICard card)
        {
            throw new NotImplementedException();
        }

        public override void Process(Player source, List<Player> dests, ICard card)
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
            Player dest = targets[0];
            GameEventArgs args = new GameEventArgs();
            args.Source = source;
            args.Targets = new List<Player>() { dest };
            args.Card = card;
            args.IntArg = 0;
            Game.CurrentGame.Emit(GameEvent.CardRangeModifier, args);
            if (Game.CurrentGame.DistanceTo(source, dest) > 1)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        private class BingLiangCunDuanTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (Owner == eventArgs.Source)
                {
                    Game.CurrentGame.CurrentPhase++;
                    Game.CurrentGame.CurrentPhaseEventIndex = 2;
                    Game.CurrentGame.UnregisterTrigger(GameEvent.PhaseOutEvents[TurnPhase.Judge], this);
                    throw new TriggerResultException(TriggerResult.End);
                }
            }
        }
    }
}
