using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Controls.Primitives;

namespace Sanguosha.UI.Controls
{
    /// <summary>
    /// Interaction logic for GuHuoSkillButton.xaml
    /// </summary>
    public partial class GuHuoSkillButton : UserControl
    {
        public GuHuoSkillButton()
        {
            InitializeComponent();
            btnSkill.Checked += new RoutedEventHandler(btnSkill_Checked);
            btnSkill.Unchecked += new RoutedEventHandler(btnSkill_Unchecked);
        }

        void btnSkill_Unchecked(object sender, RoutedEventArgs e)
        {
            IsDropDown = false;
            var command = DataContext as GuHuoSkillCommand;
            command.GuHuoChoice = null;
        }

        void btnSkill_Checked(object sender, RoutedEventArgs e)
        {
            IsDropDown = true;
        }
        
        public bool IsDropDown
        {
            get { return (bool)GetValue(IsDropDownProperty); }
            set { SetValue(IsDropDownProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDropDown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDropDownProperty =
            DependencyProperty.Register("IsDropDown", typeof(bool), typeof(GuHuoSkillButton),
            new UIPropertyMetadata(false, new PropertyChangedCallback(DropDownChanged)));

        public static void DropDownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GuHuoSkillButton button = d as GuHuoSkillButton;
            if (button != null)
            {
                button.popupChoices.IsOpen = ((bool)e.NewValue);                
            }
        }

        private void btnGuHuoType_Click(object sender, RoutedEventArgs e)
        {
            IsDropDown = false;            
        }
        
    }
}
