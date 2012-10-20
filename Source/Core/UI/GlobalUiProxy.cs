using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.UI
{
    public class GlobalUiProxy : IUiProxy
    {
        public bool AskForCardUsage(string prompt, CardUsageVerifier verifier, out Skills.ISkill skill, out List<Cards.Card> cards, out List<Players.Player> players)
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

        public void NotifyCardMovement(List<Games.CardsMovement> m, List<IGameLog> notes)
        {
            throw new NotImplementedException();
        }


        public bool AskForMultipleChoice(string prompt, List<string> questions, out int answer)
        {
            throw new NotImplementedException();
        }


        public int TimeOutSeconds { get; set; }
    }
}
