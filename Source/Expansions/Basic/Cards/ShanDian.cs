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
            if (PlayerIsCardTargetCheck(null, ref p, c))
            {
                Card result = Game.CurrentGame.Judge(p);
                if (result.Suit == SuitType.Spade && result.Rank >= 2 && result.Rank <= 9)
                {
                    GameEventArgs args = new GameEventArgs();
                    args.Source = null;
                    args.Targets = new List<Player>();
                    args.Targets.Add(p);
                    args.Skill = null;
                    args.Cards = new List<Card>();
                    args.Cards.Add(c);
                    c.Type = new ShanDianCardHandler();
                    Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
                    return;
                }
            }
            Player next = p;
            while ((next = Game.CurrentGame.NextPlayer(next)) != p)
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

        protected override void Process(Player source, Player dest, ICard card)
        {
            throw new NotImplementedException();
        }

        public override void Process(Player source, List<Player> dests, ICard card)
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

        private class ShanDianCardHandler : CardHandler
        {
            public override CardCategory Category
            {
                get { return CardCategory.Unknown; }
            }

            protected override void Process(Player source, Player dest, ICard card)
            {
                Game.CurrentGame.DoDamage(null, dest, 3, DamageElement.Lightning, card);
            }

            protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
            {
                throw new NotImplementedException();
            }
        }
    }
}
