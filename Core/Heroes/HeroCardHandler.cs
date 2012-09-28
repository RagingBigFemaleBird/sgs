using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Cards;
using Sanguosha.Core.Heroes;
using Sanguosha.Core.Skills;
using Sanguosha.Core.UI;
using Sanguosha.Core.Players;

namespace Sanguosha.Core.Heroes
{
    class HeroCardHandler : CardHandler
    {
        protected override void Process(Players.Player source, Players.Player dest)
        {
            throw new NotImplementedException();
        }

        protected override VerifierResult Verify(ICard card, List<Player> targets)
        {
            if (targets.Count == 0)
            {
                return VerifierResult.Success;
            }
            else
            {
                return VerifierResult.Fail;
            }
        }

        private Hero hero;

        public Hero Hero
        {
            get { return hero; }
            set { hero = value; }
        }
    }
}
