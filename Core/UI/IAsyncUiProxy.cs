using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;

namespace Sanguosha.Core.UI
{
    public delegate void CardUsageAnsweredEventHandler(ISkill skill, List<Card> cards, List<Player> players);
    public delegate void CardChoiceAnsweredEventHandler(List<List<Card>> cards);
    public delegate void MultipleChoiceAnsweredEventHandler(int answer);
    public interface IAsyncUiProxy
    {
        Player HostPlayer { get; set; }
        void AskForCardUsage(string prompt, CardUsageVerifier verifier);
        void AskForCardChoice(string prompt, List<DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier);
        void AskForMultipleChoice(string prompt, List<string> questions);
        void NotifyCardMovement(List<CardsMovement> m, List<IGameLog> notes);
        event CardUsageAnsweredEventHandler CardUsageAnsweredEvent;
        event CardChoiceAnsweredEventHandler CardChoiceAnsweredEvent;
        event MultipleChoiceAnsweredEventHandler MultipleChoiceAnsweredEvent;
    }
}
