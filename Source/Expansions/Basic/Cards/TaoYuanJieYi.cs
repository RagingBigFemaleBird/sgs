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
    [Serializable]
    public class TaoYuanJieYi : CardHandler
    {
        protected override void Process(Player source, Player dest, ICard card)
        {
            throw new NotImplementedException();
        }

        public override void Process(Player source, List<Player> dests, ICard card)
        {
            Trace.Assert(dests == null || dests.Count == 0);
            List<Player> toProcess = new List<Player>(Game.CurrentGame.AlivePlayers);
            Game.CurrentGame.SortByOrderOfComputation(source, toProcess);
            foreach (Player player in toProcess)
            {
                Player current = player;
                GameEventArgs args = new GameEventArgs() { Source = source, Targets = new List<Player>(), Cards = Game.CurrentGame.Decks[null, DeckType.Compute], IntArg = 1, IntArg2 = 0 };
                args.Targets.Add(current);
                if (args.Targets[0].Health >= args.Targets[0].MaxHealth)
                {
                    continue;
                }
                if (!PlayerIsCardTargetCheck(ref source, ref current, card))
                {
                    continue;
                }
                Game.CurrentGame.RecoverHealth(source, current, 1);

            }
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

        protected override List<Player> LogTargetsModifier(Player source, List<Player> dests)
        {
            var z = new List<Player>(Game.CurrentGame.AlivePlayers);
            return z;
        }
    }
}
