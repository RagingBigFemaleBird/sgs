#define NETWORKING
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

namespace Sanguosha.UI.Main
{
    /// <summary>
    /// Interaction logic for MainGame.xaml
    /// </summary>
    public partial class MainGame : Page
    {
        public MainGame()
        {
            this.InitializeComponent();
            InitGame();
            // Insert code required on object creation below this point.
        }

        int MainSeat = 0;
        private void InitGame()
        {
            _game = new RoleGame();
#if NETWORKING
            Client client;
            client = new Client();
            client.Start();
            MainSeat = (int)client.Receive();
            client.SelfId = MainSeat;
#endif
            foreach (var g in GameEngine.Expansions.Values)
            {
                _game.LoadExpansion(g);
            }
#if NETWORKING
            ClientNetworkUiProxy activeClientProxy = null;
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
                    proxy = activeClientProxy = new ClientNetworkUiProxy(proxy, client, true);
                }
                else
                {
                    proxy = new ClientNetworkUiProxy(proxy, client, false);
                }
                proxy.HostPlayer = player;
                proxy.TimeOutSeconds = 25;
#endif
                _game.UiProxies.Add(player, proxy);
            }
#if NETWORKING
            _game.GameClient = client;
            _game.GameServer = null;
            _game.IsSlave = true;
            _game.GlobalProxy = new GlobalClientUiProxy(_game, activeClientProxy);
#else
            _game.GlobalProxy = new GlobalDummyProxy();
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
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {            
            gameThread = new Thread(_game.Run) { IsBackground = true };
            gameThread.Start();
        }

    }
}
