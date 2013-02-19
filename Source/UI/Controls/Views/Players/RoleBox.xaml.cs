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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Sanguosha.UI.Animations;

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
            this.Unloaded += RoleBox_Unloaded;
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(RoleBox_DataContextChanged);
        }

        void RoleBox_Unloaded(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Role> roles = DataContext as ObservableCollection<Role>;
            if (roles != null)
            {
                roles.CollectionChanged -= _rolesChangedHandler;
            };
        }

        private void _UpdateRoles()
        {
            ObservableCollection<Role> roles = DataContext as ObservableCollection<Role>;
            bool doReveal = false;
            if (roles.Count == 2)
            {
                foreach (Role role in roles)
                {
                    if (role != Role.Unknown)
                    {
                        cbRoles.SelectedItem = role;
                    }
                }
                doReveal = true;
                
            }
            else if (roles.Contains(Role.Unknown))
            {
                cbRoles.SelectedItem = Role.Unknown;
            }
            else if (roles.Count == 1)
            {
                cbRoles.SelectedIndex = 0;
                doReveal = true;
            }

            if (doReveal)
            {
                RevealRoleAnimation anim = new RevealRoleAnimation();
                anim.SetValue(Canvas.LeftProperty, animationCenter.ActualWidth / 2 - anim.Width / 2);
                anim.SetValue(Canvas.TopProperty, animationCenter.ActualHeight / 2 - anim.Height / 2);
                animationCenter.Children.Add(anim);
                anim.Start();
            }
        }

        private NotifyCollectionChangedEventHandler _rolesChangedHandler;

        void RoleBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var oldRoles = e.OldValue as ObservableCollection<Role>;
            if (oldRoles != null)
            {
                oldRoles.CollectionChanged -= _rolesChangedHandler;
            }
            ObservableCollection<Role> roles = DataContext as ObservableCollection<Role>;
            if (roles != null)
            {
                _rolesChangedHandler = (o, n) => { _UpdateRoles(); };
                roles.CollectionChanged += _rolesChangedHandler;
                _UpdateRoles();
            }
        }
         
        private void cbRoles_DropDownOpened(object sender, EventArgs e)
        {
            IList<Role> roles = DataContext as IList<Role>;
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
