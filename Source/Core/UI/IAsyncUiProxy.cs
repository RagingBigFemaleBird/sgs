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
        void AskForCardUsage(Prompt prompt, ICardUsageVerifier verifier, int timeOutSeconds);
        void AskForCardChoice(Prompt prompt, List<DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier, int timeOutSeconds, AdditionalCardChoiceOptions options, CardChoiceRearrangeCallback callback);
        void AskForMultipleChoice(Prompt prompt, List<string> questions, int timeOutSeconds);
        event CardUsageAnsweredEventHandler CardUsageAnsweredEvent;
        event CardChoiceAnsweredEventHandler CardChoiceAnsweredEvent;
        event MultipleChoiceAnsweredEventHandler MultipleChoiceAnsweredEvent;
        void Freeze();
    }
}
