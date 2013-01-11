using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows;

namespace Sanguosha.UI.Controls
{
    public class ImageToggleButton : ToggleButton
    {
        static ImageToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageToggleButton),
                                                     new FrameworkPropertyMetadata(typeof(ImageToggleButton)));
        }

        #region properties

        public string HoverImage
        {
            get { return (string)GetValue(HoverImageProperty); }
            set { SetValue(HoverImageProperty, value); }
        }

        public string NormalImage
        {
            get { return (string)GetValue(NormalImageProperty); }
            set { SetValue(NormalImageProperty, value); }
        }

        public string PressedImage
        {
            get { return (string)GetValue(PressedImageProperty); }
            set { SetValue(PressedImageProperty, value); }
        }

        public string HoverPressedImage
        {
            get { return (string)GetValue(HoverPressedImageProperty); }
            set { SetValue(HoverPressedImageProperty, value); }
        }

        #endregion

        #region dependency properties
                
        public static readonly DependencyProperty HoverImageProperty =
            DependencyProperty.Register(
                "HoverImage", typeof(string), typeof(ImageToggleButton));

        public static readonly DependencyProperty NormalImageProperty =
            DependencyProperty.Register(
                "NormalImage", typeof(string), typeof(ImageToggleButton));

        public static readonly DependencyProperty PressedImageProperty =
            DependencyProperty.Register(
                "PressedImage", typeof(string), typeof(ImageToggleButton));

        public static readonly DependencyProperty HoverPressedImageProperty =
            DependencyProperty.Register(
                "HoverPressedImage", typeof(string), typeof(ImageToggleButton));

        #endregion
    }
}
