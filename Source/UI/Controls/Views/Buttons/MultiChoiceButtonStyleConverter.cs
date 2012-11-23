using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using Sanguosha.Core.UI;

namespace Sanguosha.UI.Controls
{
    public class MultiChoiceButtonStyleConverter : IValueConverter
    {
        static MultiChoiceButtonStyleConverter()
        {
            dict = new ResourceDictionary();
            dict.Source = new Uri("pack://application:,,,/Controls;component/Views/Buttons/MultiChoiceButton.xaml");
        }

        static ResourceDictionary dict;
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string choiceKey = value.ToString();
            if (Prompt.SuitChoices.Contains(choiceKey))
            {
                return dict["MultiChoiceSuitButtonStyle"] as Style;
            }
            else
            {
                return dict["MultiChoiceButtonStyle"] as Style;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
