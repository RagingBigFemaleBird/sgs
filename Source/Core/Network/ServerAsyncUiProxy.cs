using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Players;
using Sanguosha.Core.UI;

namespace Sanguosha.Core.Network
{
    public class ServerAsyncUiProxy : IAsyncUiProxy
    {
        public Player HostPlayer { get; set; }

        private NetworkGamer _gamer;

        protected NetworkGamer Gamer 
        {
            get
            {
                return _gamer;
            }
            set
            {
                if (_gamer == value) return;
                if (_gamer != null)
                {
                    _gamer.OnGameDataPacketReceived -= OnGameDataPacketReceived;
                }                
                if (value != null)
                {
                    value.OnGameDataPacketReceived += OnGameDataPacketReceived;
                }
                _gamer = value;
            }
        }

        private void OnGameDataPacketReceived(GameDataPacket packet)
        {
            if (QuestionState == QuestionState.AskForCardUsage)
            {
                CardUsageAnsweredEvent(
            }
        }

        private enum QuestionState
        {
            None,
            AskForCardUsage,
            AskForCardChoice,
            AskForMultipleChoice
        }

        QuestionState CurrentQuestionState { get; set; }
        int QuestionId { get; set; }

        public void AskForCardUsage(Prompt prompt, ICardUsageVerifier verifier, int timeOutSeconds)
        {
            QuestionId++;
            CurrentQuestionState = QuestionState.AskForCardUsage;
        }

        public void AskForCardChoice(Prompt prompt, List<Cards.DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier, int timeOutSeconds, AdditionalCardChoiceOptions options, CardChoiceRearrangeCallback callback)
        {
            QuestionId++;
            CurrentQuestionState = QuestionState.AskForCardChoice;
        }

        public void AskForMultipleChoice(Prompt prompt, List<OptionPrompt> questions, int timeOutSeconds)
        {
            QuestionId++;
            CurrentQuestionState = QuestionState.AskForMultipleChoice;
        }

        public event CardUsageAnsweredEventHandler CardUsageAnsweredEvent;

        public event CardChoiceAnsweredEventHandler CardChoiceAnsweredEvent;

        public event MultipleChoiceAnsweredEventHandler MultipleChoiceAnsweredEvent;

        public void Freeze()
        {
            CurrentQuestionState = QuestionState.None;
            throw new NotImplementedException();
        }
    }
}
