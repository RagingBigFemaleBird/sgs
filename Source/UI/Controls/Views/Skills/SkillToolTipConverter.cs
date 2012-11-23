using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Documents;

namespace Sanguosha.UI.Controls
{
    public class SkillToolTipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string convertType = parameter as string;
            if (convertType == null || value == null) return null;
            if (convertType == "Name" || convertType == "Description")
            {
                try
                {
                    return Application.Current.Resources[string.Format("Skill.{0}.{1}", value.ToString(), convertType)];
                }
                catch (Exception)
                {
                    Trace.TraceWarning("Cannot find skill {0}'s {1}", value.ToString(), convertType.ToLower());
                }
            }
            else if (convertType == "Usage")
            {
                string usage = Application.Current.TryFindResource(string.Format("Skill.{0}.Usage", value.ToString())) as string;
                if (usage == null) return null;
                else
                {
                    TextBlock block = new TextBlock();
                    block.TextWrapping = TextWrapping.Wrap;
                    Run run1 = new Run(Application.Current.Resources["Translation.Usage"] as string);
                    run1.Foreground = new SolidColorBrush(Colors.Yellow);
                    Run run2 = new Run(usage);
                    block.Inlines.Add(run1);
                    block.Inlines.Add(new LineBreak());
                    block.Inlines.Add(run2);
                    return block;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
