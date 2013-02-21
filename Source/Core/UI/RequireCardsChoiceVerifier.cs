using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Cards;

namespace Sanguosha.Core.UI
{
    public class RequireCardsChoiceVerifier : ICardChoiceVerifier
    {
        bool noCardReveal;
        int count;
        bool showToall;
        public RequireCardsChoiceVerifier(int count, bool noreveal = false, bool showToAll = false)
        {
            noCardReveal = noreveal;
            this.count = count;
            this.showToall = showToAll;
        }
        public VerifierResult Verify(List<List<Card>> answer)
        {
            if ((answer.Count > 1) || (answer.Count > 0 && answer[0].Count > count))
            {
                return VerifierResult.Fail;
            }
            if (answer == null || answer[0] == null || answer[0].Count < count)
            {
                return VerifierResult.Partial;
            }
            return VerifierResult.Success;
        }
        public UiHelper Helper
        {
            get { return new UiHelper() { RevealCards = !noCardReveal, ShowToAll = showToall }; }
        }
    }
}
