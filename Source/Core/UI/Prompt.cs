using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Players;
using Sanguosha.Core.Games;

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

    public class Prompt
    {
        public Prompt()
        {
            _values = new List<string>();
        }

        List<string> _values;

        public Prompt(string resourceKey, params object[] args)
        {
            ResourceKey = resourceKey;
            _values = new List<string>();
            foreach (object arg in args)
            {
                if (arg is Player)
                {
                    _values.Add(string.Format("Hero.{0}.Name", (arg as Player).Hero.Name));
                }
                else
                {
                    _values.Add(string.Format(DirectOutputPrefix + arg.ToString()));
                }
            }
        }

        public string ResourceKey
        {
            get;
            set;
        }

        public IList<string> Values
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
        public static readonly string MultipleChoiceOptionPrefix = "MultiChoice.Choice";
        public static readonly string YesChoice = MultipleChoiceOptionPrefix + "Yes";
        public static readonly string NoChoice = MultipleChoiceOptionPrefix + "No";
        public static readonly List<string> YesNoChoices = new List<string>() { YesChoice, NoChoice };
        #endregion
    }
}
