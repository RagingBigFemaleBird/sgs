using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.UI
{
    public class GlobalProxy : IUiProxy
    {
        public bool AskForCardUsage(string prompt, ICardUsageVerifier verifier, out Skills.ISkill skill, out List<Cards.Card> cards, out List<Players.Player> players)
        {
            throw new NotImplementedException();
        }

        public bool AskForCardChoice(string prompt, List<Cards.DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier, out List<List<Cards.Card>> answer)
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


        public void NotifyCardMovement(Games.CardsMovement m, List<string> notes)
        {
            throw new NotImplementedException();
        }

        public void NotifyLog(string log)
        {
            throw new NotImplementedException();
        }

        public void NotifySkillUse(int SkillID)
        {
            throw new NotImplementedException();
        }
    }
}
