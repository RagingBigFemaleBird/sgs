using Sanguosha.Core.Cards;
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

        protected override void Process(Players.Player source, Players.Player dest, ICard card)
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
    }
    public class UnknownRoleCardHandler : RoleCardHandler
    {
        public UnknownRoleCardHandler() : base(Role.Unknown)
        {
        }   
    }
}
