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
        protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard)
        {
            if (dest.Health >= dest.MaxHealth)
            {
                return;
            }
            Game.CurrentGame.RecoverHealth(source, dest, 1);
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

        public override List<Player> ActualTargets(Player source, List<Player> dests)
        {
            var z = new List<Player>(Game.CurrentGame.AlivePlayers);
            foreach (var p in new List<Player>(z))
            {
                if (p.Health >= p.MaxHealth)
                {
                    z.Remove(p);
                }
            }
            return z;
        }
    }
}
