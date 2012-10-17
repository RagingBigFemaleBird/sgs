using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;


namespace Sanguosha.Core.UI
{
    public class NetworkUiProxy : IAsyncUiProxy
    {

        public Player HostPlayer
        {
            get;
            set;
        }

        public void AskForCardUsage(string prompt, ICardUsageVerifier verifier)
        {
            throw new NotImplementedException();
        }

        public void AskForCardChoice(string prompt, List<DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier)
        {
            throw new NotImplementedException();
        }

        public void AskForMultipleChoice(string prompt, List<string> questions)
        {
            throw new NotImplementedException();
        }

        public void NotifyCardMovement(List<CardsMovement> m, List<IGameLog> notes)
        {
            CardUsageAnsweredEvent(null, null, null);
            CardChoiceAnsweredEvent(null);
            MultipleChoiceAnsweredEvent(0);
            throw new NotImplementedException();
        }

        public event CardUsageAnsweredEventHandler CardUsageAnsweredEvent;

        public event CardChoiceAnsweredEventHandler CardChoiceAnsweredEvent;

        public event MultipleChoiceAnsweredEventHandler MultipleChoiceAnsweredEvent;
    }
}
