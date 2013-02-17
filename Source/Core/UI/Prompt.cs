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
        public CardUsagePrompt(string key, params object[] args)
            : base(CardUsagePromptsPrefix + key, args)
        {
        }
    }

    public class CardChoicePrompt : Prompt
    {
        public CardChoicePrompt(string key, params object[] args)
            : base(CardChoicePromptsPrefix + key, args)
        {
        }
    }

    public class MultipleChoicePrompt : Prompt
    {
        public MultipleChoicePrompt(string key, params object[] args)
            : base(MultipleChoicePromptPrefix + key, args)
        {
        }
    }

    public class OptionPrompt : Prompt
    {
        public OptionPrompt(string key, params object[] args)
            : base(MultipleChoiceOptionPrefix + key, args)
        {

        }
    }

    public class LogEvent : Prompt
    {
        public LogEvent(string key, params object[] args)
            : base(LogEventPrefix + key, args)
        {
        }
    }

    public class LogEventArg : Prompt
    {
        public LogEventArg(string key, params object[] args)
            : base(LogEventArgPrefix + key, args)
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

        public Prompt(string resourceKey, params object[] args)
            : this()
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

        #region Log Event
        public static readonly string LogEventPrefix = "LogEvent.";
        public static readonly string LogEventArgPrefix = "LogEvent.Arg.";
        public static readonly LogEventArg Success = new LogEventArg("Success");
        public static readonly LogEventArg Fail = new LogEventArg("Fail");
        #endregion

        #region Multiple Choice Constants
        public static readonly string MultipleChoicePromptPrefix = "MultiChoice.Prompt.";
        public static readonly string MultipleChoiceOptionPrefix = "MultiChoice.Choice.";
        public static readonly string NonPlaybleAppendix = ".Others";
        public static readonly string SkillUseYewNoPrompt = "SkillYesNo";
        public static readonly OptionPrompt YesChoice = new OptionPrompt("Yes");
        public static readonly OptionPrompt NoChoice = new OptionPrompt("No");
        public static readonly OptionPrompt HeartChoice = new OptionPrompt("Heart");
        public static readonly OptionPrompt SpadeChoice = new OptionPrompt("Spade");
        public static readonly OptionPrompt ClubChoice = new OptionPrompt("Club");
        public static readonly OptionPrompt DiamondChoice = new OptionPrompt("Diamond");
        public static readonly List<OptionPrompt> YesNoChoices = new List<OptionPrompt>() { NoChoice, YesChoice };
        public static readonly List<OptionPrompt> SuitChoices = new List<OptionPrompt>() { ClubChoice, SpadeChoice, HeartChoice, DiamondChoice };
        public static readonly List<OptionPrompt> AllegianceChoices = new List<OptionPrompt>() { new OptionPrompt("Qun"), new OptionPrompt("Shu"), new OptionPrompt("Wei"), new OptionPrompt("Wu") };
        #endregion
    }
}
