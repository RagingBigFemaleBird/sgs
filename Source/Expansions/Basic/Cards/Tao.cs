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
    
    public class Tao : LifeSaver
    {
        protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard)
        {
            Game.CurrentGame.RecoverHealth(source, dest, 1);
        }

        public override List<Player> ActualTargets(Player source, List<Player> targets)
        {
            if (Game.CurrentGame.IsDying.Count > 0)
            {
                return new List<Player>() { Game.CurrentGame.IsDying.First() };
            }
            else
            {
                return new List<Player>() { source };
            }
        }

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            if (Game.CurrentGame.IsDying.Count == 0 && targets != null && targets.Count >= 1)
            {
                return VerifierResult.Fail;
            }
            if (Game.CurrentGame.IsDying.Count > 0 && (targets == null || targets.Count != 1))
            {
                return VerifierResult.Fail;
            }
            Player p;
            if (Game.CurrentGame.IsDying.Count == 0)
            {
                p = source;
            }
            else
            {
                p = targets[0];
            }
            if (p.Health >= p.MaxHealth)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public override CardCategory Category
        {
            get { return CardCategory.Basic; }
        }
    }
}
