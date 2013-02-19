using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sanguosha.UI.Resources
{
    public class FileNameToImageSourceConverter : IMultiValueConverter
    {
        // expects the target object as the first parameter, and the resource key as the second
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length < 2)
            {
                return null;
            }

            var element = values[0] as FrameworkElement;
            var resourceKey = values[1];

            if (element == null || resourceKey == null)
            {
                return null;
            }

            if (ResourceKeyFormat != null)
            {
                resourceKey = string.Format(ResourceKeyFormat, resourceKey);
            }

            if (ResourceKeyConverter != null)
            {
                resourceKey = ResourceKeyConverter.Convert(resourceKey, targetType, ConverterParameter, culture);
            }

            if (resourceKey == null) return null;

            var resource = element.TryFindResource(resourceKey);

            if (resource != null)
            {             
                if (CropRect == null) return resource;
                else if (resource is BitmapSource)
                {
                    BitmapSource bitmap = resource as BitmapSource;
                    var result = new CroppedBitmap(bitmap, CropRect);
                    result.Freeze();
                    return result;
                }
            }

            string fileName = null;
            resourceKey = values[1];

            if (StringFormat != null)
            {
                fileName = string.Format(StringFormat, resourceKey);
            }           

            if (ResourceKeyConverter != null)
            {
                fileName = ResourceKeyConverter.Convert(fileName, targetType, ConverterParameter, culture).ToString();
            }

            if (fileName != null)
            {

                fileName = string.Format("pack://siteoforigin:,,,/{0}", fileName);

                try
                {
                    object imageObj = (new ImageSourceConverter()).ConvertFromString(fileName);
                    var image = imageObj as ImageSource;
                    if (image != null)
                    {
                        if (CropRect == null) return image;
                        else if (image is BitmapSource)
                        {
                            BitmapSource bitmap = image as BitmapSource;
                            var result = new CroppedBitmap(bitmap, CropRect);
                            result.Freeze();
                            return result;
                        }
                    }
                }
                catch (NullReferenceException)
                {
                }
                catch (NotSupportedException)
                {
                }
                catch (ArgumentException)
                {
                    Trace.TraceWarning("Image not in expected size: {0}", fileName);
                }
            }

            Trace.TraceWarning("Resource not found: {0} or {1}\n", resourceKey, fileName);
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public IValueConverter ResourceKeyConverter { get; set; }

        public object ConverterParameter { get; set; }

        public string StringFormat { get; set; }

        public string ResourceKeyFormat { get; set; }

        public Int32Rect CropRect { get; set; }
    }
}
