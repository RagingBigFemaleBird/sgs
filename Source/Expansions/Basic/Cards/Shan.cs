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
    public class Shan : CardHandler
    {
        protected override void Process(Player source, Player dest, ICard card)
        {
            throw new NotImplementedException();
        }

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            return VerifierResult.Fail;
        }

        public override CardCategory Category
        {
            get { return CardCategory.Basic; }
        }
    }
}
