using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Cards;

namespace Sanguosha.Core.UI
{
    public class AlwaysTrueChoiceVerifier : ICardChoiceVerifier
    {
        public VerifierResult Verify(List<List<Card>> answer)
        {
            return VerifierResult.Success;
        }
    }
}
