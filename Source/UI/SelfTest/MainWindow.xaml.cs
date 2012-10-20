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
using Sanguosha.Core.Network;


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

        const int MainSeat = 0;
        private void InitGame()
        {
            GameEngine.LoadExpansions("Expansions");
            _game = new RoleGame();
#if NETWORKING
            Client client;
            client = new Client();
            client.Start();
            client.SelfId = MainSeat;
#endif
            foreach (var g in GameEngine.Expansions.Values)
            {
                _game.LoadExpansion(g);
            }
#if NETWORKING
            for (int i = 0; i < 3; i++)
#else
            for (int i = 0; i < 8; i++)
#endif
            {
                Player player = new Player();
                player.Id = i;
                _game.Players.Add(player);
                IUiProxy proxy = new ConsoleUiProxy();
                if (i == MainSeat)
                {
                    proxy = new AsyncUiAdapter(gameView);
                }
                else
                {
                    proxy.HostPlayer = player;
                }
                if (i == 1)
                {
                    player.IsFemale = true;
                }
                else
                {
                    player.IsMale = true;
                }
#if NETWORKING
                if (i == MainSeat)
                {
                    proxy = new ClientNetworkUiProxy(proxy, client, true);
                    proxy.HostPlayer = player;
                }
                else
                {
                    proxy = new ClientNetworkUiProxy(proxy, client, false);
                    proxy.HostPlayer = player;
                }
#endif
                _game.UiProxies.Add(player, proxy);
            }
#if NETWORKING
            _game.GameClient = client;
            _game.GameServer = null;
            _game.Slave = true;
#endif
            _player = _game.Players[MainSeat];
            GameViewModel gameModel = new GameViewModel();
            gameModel.Game = _game;
            gameModel.MainPlayerSeatNumber = MainSeat;
            gameView.DataContext = gameModel;
            _game.UiProxies[_game.Players[MainSeat]].HostPlayer = _game.Players[MainSeat];
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