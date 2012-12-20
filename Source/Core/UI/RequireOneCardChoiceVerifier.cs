using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Cards;

namespace Sanguosha.Core.UI
{
    public class RequireOneCardChoiceVerifier : ICardChoiceVerifier
    {
        public VerifierResult Verify(List<List<Card>> answer)
        {
            if ((answer != null && answer.Count > 1) || (answer != null && answer[0] != null && answer[0].Count > 1))
            {
                return VerifierResult.Fail;
            }
            if (answer == null || answer.Count == 0 || answer[0] == null || answer[0].Count == 0)
            {
                return VerifierResult.Partial;
            }
            return VerifierResult.Success;
        }
        public UiHelper Helper
        {
            get { return null; }
        }
    }
}
