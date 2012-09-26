using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;

namespace Sanguosha.Core.Cards
{
    public abstract class CardHandler
    {
        public virtual void Process(Player source, List<Player> dests)
        {
            foreach (var player in dests)
            {
                Process(source, player);
            }
        }

        protected abstract void Process(Player source, Player dest);

        public abstract VerifierResult Verify(Skill skill, List<Card> cards, List<Player> players);

        public string CardType
        {
            get { return this.GetType().ToString(); }
        }
    }

}
