using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Cards;

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
        public MultipleChoicePrompt(string cardName, params object[] args)
            : base(MultipleChoicePromptPrefix + cardName, args)
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
                else if (arg is ICard)
                {
                    _values.Add(string.Format("Card.{0}.Name", (arg as ICard).Type.CardType));
                }
                else if (arg is SuitType)
                {
                    _values.Add(string.Format("Suit.{0}.Name", ((SuitType)arg).ToString()));
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
        public static readonly string MultipleChoicePromptPrefix = "MultiChoice.Prompt.";
        public static readonly string MultipleChoiceOptionPrefix = "MultiChoice.Choice.";
        public static readonly string YesChoice = MultipleChoiceOptionPrefix + "Yes";
        public static readonly string NoChoice = MultipleChoiceOptionPrefix + "No";
        public static readonly string HeartChoice = MultipleChoiceOptionPrefix + "Heart";
        public static readonly string SpadeChoice = MultipleChoiceOptionPrefix + "Spade";
        public static readonly string ClubChoice = MultipleChoiceOptionPrefix + "Club";
        public static readonly string DiamondChoice = MultipleChoiceOptionPrefix + "Diamond";
        public static readonly List<string> YesNoChoices = new List<string>() { YesChoice, NoChoice };
        public static readonly List<string> SuitChoices = new List<string>() { HeartChoice, SpadeChoice, ClubChoice, DiamondChoice };
        #endregion
    }
}
