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
using Sanguosha.Lobby.Core;
using System.Diagnostics;

namespace Sanguosha.UI.Controls
{
    /// <summary>
    /// Interaction logic for LobbyView.xaml
    /// </summary>
    public partial class LobbyView : Page
    {
        public LobbyView()
        {
            InitializeComponent();
        }

        public LobbyViewModel LobbyModel
        {
            get
            {
                return DataContext as LobbyViewModel;
            }
            set
            {
                DataContext = value;
            }
        }

        private void muteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            muteButton.Visibility = Visibility.Collapsed;
            soundButton.Visibility = Visibility.Visible;
            GameSoundPlayer.IsMute = false;
        }

        private void soundButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            soundButton.Visibility = Visibility.Collapsed;
            muteButton.Visibility = Visibility.Visible;
            GameSoundPlayer.IsMute = true;
        }

        private void viewRoomButton_Click(object sender, RoutedEventArgs e)
        {
            Trace.Assert(sender is Button);
            var model = (sender as Button).DataContext as RoomViewModel;
            if (model != null)
            {
                LobbyModel.CurrentRoom = model;
            }
        }
    }
}
