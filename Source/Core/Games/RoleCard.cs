using Sanguosha.Core.Cards;
using Sanguosha.Core.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.Games
{
    public class RoleCardHandler : CardHandler
    {
        public override CardCategory Category
        {
            get { return CardCategory.Unknown; }
        }

        protected override void Process(Players.Player source, Players.Player dest, ICard card, ReadOnlyCard cardr, GameEventArgs inResponseTo)
        {
            throw new NotImplementedException();
        }

        protected override UI.VerifierResult Verify(Players.Player source, ICard card, List<Players.Player> targets)
        {
            throw new NotImplementedException();
        }

        public Role Role { get; set; }

        public RoleCardHandler(Role r)
        {
            Role = r;
        }

        public override string CardType
        {
            get
            {
                return Role.ToString();
            }
        }
    }
    public class UnknownRoleCardHandler : RoleCardHandler
    {
        public UnknownRoleCardHandler() : base(Role.Unknown)
        {
        }

        public override string CardType
        {
            get
            {
                return _cardTypeString;
            }
        }

        private static string _cardTypeString = "UnknownRole";
    }
}
