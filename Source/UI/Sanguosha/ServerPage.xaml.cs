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
using System.ServiceModel;
using Sanguosha.Lobby.Server;

namespace Sanguosha.UI.Main
{
    /// <summary>
    /// Interaction logic for ServerPage.xaml
    /// </summary>
    public partial class ServerPage : Page
    {
        public ServerPage()
        {
            InitializeComponent();
        }

        private void btnGoBack_Click(object sender, RoutedEventArgs e)
        {
            Host.Close();
            this.NavigationService.Navigate(Login.Instance);
        }

        public ServiceHost Host { get; set; }

        private void cbAllowCheating_Click_1(object sender, RoutedEventArgs e)
        {
            LobbyServiceImpl.CheatEnabled = cbAllowCheating.IsChecked == true;
        }

        public Lobby.Server.LobbyServiceImpl GameService { get; set; }
    }
}
