using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Sanguosha.Core.UI;
using System.Windows;

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
            foreach (string arg in prompt.Values)
            {
                string value = null;
                if (arg.StartsWith(Prompt.DirectOutputPrefix))
                {
                    value = arg.Substring(Prompt.DirectOutputPrefix.Length);
                }
                else
                {
                    value = Application.Current.TryFindResource(arg) as string;
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
