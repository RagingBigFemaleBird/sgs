using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.UI
{
    public class GlobalProxy : IUiProxy
    {
        public void AskForCardUsage(string prompt, ICardUsageVerifier verifier, out Skills.ISkill skill, out List<Cards.Card> cards, out List<Players.Player> players)
        {
            throw new NotImplementedException();
        }

        public void AskForCardChoice(List<Cards.DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier, out List<List<Cards.Card>> answer)
        {
            throw new NotImplementedException();
        }

        public Players.Player HostPlayer
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
