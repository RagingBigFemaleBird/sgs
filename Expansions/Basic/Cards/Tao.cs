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
    public class Tao : CardHandler
    {
        protected override void Process(Player source, Player dest, ICard card)
        {
            throw new NotImplementedException();
        }

        public override void Process(Player source, List<Player> dests, ICard card)
        {
            Game.CurrentGame.PlayerUsedCard(source, card);
            Trace.Assert(dests == null || dests.Count == 0);
            if (PlayerIsCardTargetCheck(source, ref source))
            {
                return;
            }
            Game.CurrentGame.RecoverHealth(source, source, 1);
        }

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            if (targets != null && targets.Count >= 1)
            {
                return VerifierResult.Fail;
            }
            if (source.Health >= source.MaxHealth)
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
