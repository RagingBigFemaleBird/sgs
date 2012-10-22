using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Diagnostics;

namespace Sanguosha.UI.Controls
{
    public class HeroToolTipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string convertType = parameter as string;
            if (convertType == null || value == null) return null;
            if (convertType == "Name")
            {
                try
                {
                    return Application.Current.Resources[string.Format("Hero.{0}.Name", value.ToString())];
                }
                catch (Exception)
                {
                    Trace.TraceWarning("Cannot find hero {0}'s Name", value.ToString());
                }
            }
            else if (convertType == "Allegiance")
            {
                try
                {
                    return Application.Current.Resources[string.Format("Allegiance.{0}.Name", value.ToString())];
                }
                catch (Exception)
                {
                    Trace.TraceWarning("Cannot find allegiance {0}'s Name", value.ToString());
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
