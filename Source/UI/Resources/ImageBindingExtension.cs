using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows.Data;
using System.Windows;
using System.Globalization;
using System.ComponentModel;

namespace Sanguosha.UI.Resources
{
    public class ImageBindingExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var imageBinding = new Binding()
            {
                BindsDirectlyToSource = BindsDirectlyToSource,
                Mode = BindingMode.OneWay,
                Path = Path,
                XPath = XPath,
            };

            //Binding throws an InvalidOperationException if we try setting all three
            // of the following properties simultaneously: thus make sure we only set one
            if (ElementName != null)
            {
                imageBinding.ElementName = ElementName;
            }
            else if (RelativeSource != null)
            {
                imageBinding.RelativeSource = RelativeSource;
            }
            else if (Source != null)
            {
                imageBinding.Source = Source;
            }

            var targetElementBinding = new Binding();
            targetElementBinding.RelativeSource = new RelativeSource()
            {
                Mode = RelativeSourceMode.Self
            };

            var multiBinding = new MultiBinding();
            multiBinding.Bindings.Add(targetElementBinding);
            multiBinding.Bindings.Add(imageBinding);            

            // If we set the Converter on resourceKeyBinding then, for some reason,
            // MultiBinding wants it to produce a value matching the Target Type of the MultiBinding
            // When it doesn't, it throws a wobbly and passes DependencyProperty.UnsetValue through
            // to our MultiBinding ValueConverter. To circumvent this, we do the value conversion ourselves.
            // See http://social.msdn.microsoft.com/forums/en-US/wpf/thread/af4a19b4-6617-4a25-9a61-ee47f4b67e3b
            multiBinding.Converter = new FileNameToImageSourceConverter()
            {
                ResourceKeyConverter = Converter,
                ConverterParameter = ConverterParameter,
                StringFormat = StringFormat,
                ResourceKeyFormat = ResourceKeyFormat,
                CropRect = CropRect
            };

            return multiBinding.ProvideValue(serviceProvider);
        }

        [DefaultValue("")]
        public object AsyncState { get; set; }

        [DefaultValue(false)]
        public bool BindsDirectlyToSource { get; set; }

        [DefaultValue("")]
        public IValueConverter Converter { get; set; }

        [TypeConverter(typeof(CultureInfoIetfLanguageTagConverter))]
        [DefaultValue("")]
        public CultureInfo ConverterCulture { get; set; }

        [DefaultValue("")]
        public object ConverterParameter { get; set; }

        [DefaultValue("")]
        public string ElementName { get; set; }

        [DefaultValue("")]
        public PropertyPath Path { get; set; }

        [DefaultValue("")]
        public RelativeSource RelativeSource { get; set; }

        [DefaultValue("")]
        public object Source { get; set; }

        [DefaultValue("")]
        public string XPath { get; set; }

        [DefaultValue("")]
        public string StringFormat { get; set; }

        [DefaultValue("")]
        public string ResourceKeyFormat { get; set; }

        [DefaultValue("")]
        public Int32Rect CropRect { get; set; }
    }
}
