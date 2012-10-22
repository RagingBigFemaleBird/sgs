using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Diagnostics;
using System.Windows;

namespace Sanguosha.UI.Controls
{
    public class SkillToolTipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string convertType = parameter as string;
            if (convertType == null || value == null) return null;
            if (convertType == "Name" || convertType == "Description" || convertType == "Usage")
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
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
