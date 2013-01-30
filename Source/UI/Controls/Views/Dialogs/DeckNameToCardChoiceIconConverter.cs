using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Sanguosha.UI.Controls
{
    public class DeckNameToCardChoiceIconConverter : IValueConverter
    {
        static ResourceDictionary dict = new ResourceDictionary()
        {
            Source = new Uri("pack://application:,,,/Resources;component/Images/SystemImages.xaml")
        };

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string deckName = value.ToString();
            string resKey = string.Format("Game.CardChoice.Area.{0}", deckName);
            if (!dict.Contains(resKey))
            {
                Trace.TraceInformation("Image for deck key {0} not found.", resKey);
                return null;
            }
            else
            {
                var image = dict[resKey] as ImageSource;
                if (image == null)
                {
                    Trace.TraceWarning("Cannot load image {0}", resKey);
                }
                return image;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
