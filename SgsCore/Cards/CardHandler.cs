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
        public delegate VerifierResult VerifierPointer(Skill skill, List<Card> cards, List<Player> players);

        public class CardHandlerVerifier : ICardUsageVerifier
        {
            public VerifierResult Verify(Skill skill, List<Card> cards, List<Player> players)
            {
                return verifier(skill, cards, players);
            }

            VerifierPointer verifier;

            public VerifierPointer Verifier
            {
                get { return verifier; }
                set { verifier = value; }
            }
        }

        public ICardUsageVerifier Verifier
        {
            get
            {
                return new CardHandlerVerifier() { Verifier = Verify };
            }
        }

        public abstract VerifierResult Verify(Skill skill, List<Card> cards, List<Player> players);

        public string CardType
        {
            get { return this.GetType().ToString(); }
        }
    }

}
