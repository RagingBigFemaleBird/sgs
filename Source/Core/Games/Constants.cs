using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.Games
{
    public class Constants
    {
        #region Card Usage Prompts
        public static readonly string CardUsagePromptsPrefix = "CardUsage.Prompt.";
        public static readonly string PlayingPhasePrompt = CardUsagePromptsPrefix + "Play";
        public static readonly string DiscardPhasePrompt = CardUsagePromptsPrefix + "Discard";
        #endregion

        #region Multiple Choice Constants
        public static readonly string MultipleChoiceOptionPrefix = "MultiChoice.Choice";
        public static readonly string YesChoice = MultipleChoiceOptionPrefix + "Yes";
        public static readonly string NoChoice = MultipleChoiceOptionPrefix + "No";
        public static readonly List<string> YesNoChoices = new List<string>() { YesChoice, NoChoice};
        #endregion
    }
}
