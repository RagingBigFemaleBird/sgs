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
using Sanguosha.Core.Games;
using Sanguosha.Core.UI;
using System.Threading;

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
            InitGame();
			// Insert code required on object creation below this point.
		}

        private void InitGame()
        {
            GameEngine.LoadExpansions("Expansions");
            _game = new RoleGame();
            foreach (var g in GameEngine.Expansions.Values)
            {
                _game.LoadExpansion(g);
            }
            for (int i = 0; i < 8; i++)
            {
                Player player = new Player();
                _game.Players.Add(player);
                IUiProxy proxy = new ConsoleUiProxy();
                if (i == 0)
                {
                    proxy = new AsyncUiAdapter<GameView>(gameView);
                }
                else
                {
                    proxy.HostPlayer = player;
                }
                _game.UiProxies.Add(player, proxy);
            }
            _player = _game.Players[0];
            GameViewModel gameModel = new GameViewModel();
            gameModel.Game = _game;
            gameModel.MainPlayerSeatNumber = 0;
            gameView.DataContext = gameModel;
            _game.UiProxies[_game.Players[0]].HostPlayer = _game.Players[0];
        }

        private Game _game;
        private Player _player;
        Thread gameThread;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            gameThread = new Thread(_game.Run);
            gameThread.Start();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            gameThread.Abort();
        }
	}
}