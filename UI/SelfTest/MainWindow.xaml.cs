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
using System.Windows.Shapes;
using Sanguosha.UI.Controls;
using Sanguosha.Core.Heroes;
using Sanguosha.Core.Players;

namespace WpfApplication1
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			this.InitializeComponent();

			// Insert code required on object creation below this point.
		}

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            PlayerInfoViewModel model = Resources["viewModel"] as PlayerInfoViewModel;
            Player player = new Player();
            
            player.Role = Sanguosha.Core.Games.Role.Unknown;
            player.Allegiance = Allegiance.Wei;            
            player.MaxHealth = 10;
            player.Health = 4;
            
            model.Player = player;
            model.Game = new Sanguosha.Core.Games.RoleGame();
        }

        private void btnUpdate2_Click(object sender, RoutedEventArgs e)
        {
            PlayerInfoViewModel model = Resources["viewModel"] as PlayerInfoViewModel;            
            Player player = model.Player;
            player.Role = Sanguosha.Core.Games.Role.Rebel;
            player.MaxHealth = 5;
            player.Health = 1;
        }
	}
}