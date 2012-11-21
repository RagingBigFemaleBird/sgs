using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Cards;
using Sanguosha.Core.Skills;

namespace Sanguosha.Core.UI
{

    public class CardUsagePrompt : Prompt
    {
        public CardUsagePrompt(string cardName, params object[] args) : base(CardUsagePromptsPrefix + cardName, args)
        {
        }
    }

    public class CardChoicePrompt : Prompt
    {
        public CardChoicePrompt(string cardName, params object[] args) : base(CardChoicePromptsPrefix + cardName, args)
        {
        }
    }

    public class MultipleChoicePrompt : Prompt
    {
        public MultipleChoicePrompt(string name, params object[] args)
            : base(MultipleChoicePromptPrefix + name, args)
        {
        }
    }

    public class Prompt
    {
        public Prompt()
        {
            _values = new List<object>();
        }

        List<object> _values;

        public Prompt(string resourceKey, params object[] args) : this()
        {
            ResourceKey = resourceKey;
            _values.AddRange(args);
        }

        public string ResourceKey
        {
            get;
            set;
        }

        public IList<object> Values
        {
            get
            {
                return _values;
            }
        }

        #region Resource Converter Prefixes
        public static string DirectOutputPrefix = "#";
        #endregion

        #region Card Usage Prompts
        public static readonly string CardUsagePromptsPrefix = "CardUsage.Prompt.";
        public static readonly string PlayingPhasePrompt = CardUsagePromptsPrefix + "Play";
        public static readonly string DiscardPhasePrompt = CardUsagePromptsPrefix + "Discard";
        #endregion

        #region Card Choice Prompts
        public static readonly string CardChoicePromptsPrefix = "CardChoice.Prompt.";
        #endregion

        #region Multiple Choice Constants
        public static readonly string MultipleChoicePromptPrefix = "MultiChoice.Prompt.";
        public static readonly string MultipleChoiceOptionPrefix = "MultiChoice.Choice.";
        public static readonly string SkillUseYewNoPrompt = "SkillYesNo";
        public static readonly string YesChoice = MultipleChoiceOptionPrefix + "Yes";
        public static readonly string NoChoice = MultipleChoiceOptionPrefix + "No";
        public static readonly string HeartChoice = MultipleChoiceOptionPrefix + "Heart";
        public static readonly string SpadeChoice = MultipleChoiceOptionPrefix + "Spade";
        public static readonly string ClubChoice = MultipleChoiceOptionPrefix + "Club";
        public static readonly string DiamondChoice = MultipleChoiceOptionPrefix + "Diamond";
        public static readonly List<string> YesNoChoices = new List<string>() { YesChoice, NoChoice };
        public static readonly List<string> SuitChoices = new List<string>() { ClubChoice, SpadeChoice, HeartChoice, DiamondChoice };
        #endregion
    }
}
