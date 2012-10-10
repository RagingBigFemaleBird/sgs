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

using Sanguosha.Core.Games;

namespace Sanguosha.UI.Controls
{
    public class RoleToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return 0;
            Role role = (Role)value;
            if (role == Role.Unknown) return 0;
            else return double.NaN;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for RoleBox.xaml
    /// </summary>
    public partial class RoleBox : UserControl
    {
        public RoleBox()
        {
            InitializeComponent();
            cbRoles.SelectedItem = Role.Unknown;
        }
          
        private void cbRoles_DropDownOpened(object sender, EventArgs e)
        {
            List<Role> roles = DataContext as List<Role>;
            if (roles.Count <= 2)
            {
                cbRoles.IsDropDownOpen = false;
                if (roles.Count > 1 && (Role)cbRoles.SelectedItem == Role.Unknown)
                {
                    foreach (Role role in roles)
                    {
                        if (role != Role.Unknown)
                        {
                            cbRoles.SelectedItem = role;
                        }
                    }
                    return;
                }
                
            }
            cbRoles.SelectedItem = Role.Unknown;
        }

        private void cbRoles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbRoles.SelectedIndex == -1 && cbRoles.Items.Count > 0)
            {
                cbRoles.SelectedIndex = 0;
            }
        } 

    }
}
