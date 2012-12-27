using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Sanguosha.Core.UI;
using System.Windows;
using Sanguosha.Core.Players;
using Sanguosha.Core.Cards;
using Sanguosha.Core.Skills;
using System.Windows.Documents;

namespace Sanguosha.UI.Controls
{
    public class PromptFormatter
    {
        public static string Format(Prompt prompt)
        {         
            List<string> values = new List<string>();
            string format = Application.Current.TryFindResource(prompt.ResourceKey) as string;
            if (format == null)
            {
                Trace.TraceInformation("Key not found: {0}", format);
                return prompt.ResourceKey;
            }
            else if (format.StartsWith(Prompt.DirectOutputPrefix))
            {
                format = format.Substring(Prompt.DirectOutputPrefix.Length);
            }

            foreach (object arg in prompt.Values)
            {
                string resKey = null;

                if (arg is Player)
                {
                    Player player = arg as Player;
                    if (player == null || player.Hero == null)
                    {
                        resKey = string.Empty;
                    }
                    else
                    {
                        resKey = string.Format("Hero.{0}.Name", (arg as Player).Hero.Name);
                    }
                }
                else if (arg is ICard)
                {
                    resKey = string.Format("Card.{0}.Name", (arg as ICard).Type.CardType);
                }
                else if (arg is SuitType)
                {
                    resKey = string.Format("Suit.{0}.Text", ((SuitType)arg).ToString());
                }
                else if (arg is ISkill)
                {
                    resKey = string.Format("Skill.{0}.Name", arg.GetType().Name);
                }

                string value;
                if (resKey != null)
                {
                    value = Application.Current.TryFindResource(resKey) as string;
                }
                else
                {
                    value = arg.ToString();
                }               
                
                if (value == null)
                {
                    value = string.Empty;
                    Trace.TraceInformation("Key not found: {0}", arg);                    
                }

                values.Add(value);
            }
            return string.Format(format, values.ToArray());
        }
    }
}
