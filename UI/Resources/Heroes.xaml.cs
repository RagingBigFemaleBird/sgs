using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows;

namespace Sanguosha.UI.Resources
{

    public abstract class HeroImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string heroName = value as string;
            if (heroName == null) return null;
            heroName = string.Format("Hero.{0}.Image", heroName);
            ResourceDictionary dict = new ResourceDictionary();
            dict.Source = new Uri("pack://application:,,,/Resources;component/Heroes.xaml");
            BitmapSource image = dict[heroName] as BitmapSource;
            if (image == null) return null;
            CroppedBitmap bitmap = new CroppedBitmap(image, GetCropRect(heroName));
            return bitmap;
        }

        public abstract Int32Rect GetCropRect(string heroName);       

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HeroLargeImageConverter : HeroImageConverter
    {        
        private static Int32Rect cropRect = new Int32Rect(28, 46, 220, 132);
             
        public override Int32Rect GetCropRect(string heroName)
        {
            return cropRect;
        }
    }

    public class HeroSquareImageConverter : HeroImageConverter
    {
        private static Int32Rect cropRect = new Int32Rect(71, 28, 145, 145);

        public override Int32Rect GetCropRect(string heroName)
        {
            return cropRect;
        }
    }
}
