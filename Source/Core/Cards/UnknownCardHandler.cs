using Sanguosha.Core.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.Cards
{
    public class UnknownCardHandler : CardHandler
    {
        public override CardCategory Category
        {
            get { return CardCategory.Unknown; }
        }

        protected override void Process(Players.Player source, Players.Player dest, ICard card, ReadOnlyCard cardr, GameEventArgs inResponseTo)
        {
            throw new NotImplementedException();
        }

        public override UI.VerifierResult Verify(Players.Player source, ICard card, List<Players.Player> targets, bool isLooseVerify)
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get
            {
                return _cardTypeString;
            }
        }

        private static string _cardTypeString = "Unknown";
    }
}
