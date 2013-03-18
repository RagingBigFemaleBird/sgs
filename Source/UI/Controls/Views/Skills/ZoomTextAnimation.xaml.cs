using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sanguosha.UI.Animations
{
	/// <summary>
	/// Interaction logic for SkillNameTextAnimation.xaml
	/// </summary>
	public partial class RegularSkillAnimation : AnimationBase
	{
		public RegularSkillAnimation()
		{
			this.InitializeComponent();
		}

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(RegularSkillAnimation), new UIPropertyMetadata(string.Empty, new PropertyChangedCallback(OnTextChanged)));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as RegularSkillAnimation;
            if (d != null) control.mainText.Text = e.NewValue as string;
        }
	}
}