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
    public class TaoYuanJieYi : CardHandler
    {
        protected override void Process(Player source, Player dest)
        {
            throw new NotImplementedException();
        }

        public override void Process(Player source, List<Player> dests)
        {
            Trace.Assert(dests == null || dests.Count == 0);
            Player current = source;
            do
            {
                GameEventArgs args = new GameEventArgs() { Source = source, Targets = new List<Player>(), Cards = Game.CurrentGame.Decks[null, DeckType.Compute], IntArg = 1, IntArg2 = 0 };
                args.Targets.Add(current);

                Game.CurrentGame.Emit(GameEvent.BeforeHealthChanged, args);

                Trace.Assert(args.Targets.Count == 1);
                args.Targets[0].Health += args.IntArg;
                Trace.TraceInformation("Player {0} gain {1} hp, @ {2} hp", args.Targets[0].Id, args.IntArg, args.Targets[0].Health);

                Game.CurrentGame.Emit(GameEvent.AfterHealthChanged, args);

                current = Game.CurrentGame.NextPlayer(current);
            } while (current != source);
        }

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            if (targets != null && targets.Count >= 1)
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
