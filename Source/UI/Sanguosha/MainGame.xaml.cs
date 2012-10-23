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
            }
#if NETWORKING
            _game.GameClient = client;
            _game.GameServer = null;
            _game.IsSlave = true;
#else
            _game.GlobalProxy = new GlobalDummyProxy();
#endif
            GameViewModel gameModel = new GameViewModel();
            gameModel.Game = _game;
            gameModel.MainPlayerSeatNumber = MainSeat;
            gameView.DataContext = gameModel;
            _game.NotificationProxy = gameView;

            for (int i = 0; i < _game.Players.Count; i++)
            {
                var player = gameModel.PlayerModels[i].Player;                
#if NETWORKING
                var proxy = new ClientNetworkUiProxy(
                            new AsyncUiAdapter(gameModel.PlayerModels[i]), client, i == 0);
                proxy.HostPlayer = player;
                proxy.TimeOutSeconds = 25;
                if (i == 0)
                {
                    activeClientProxy = proxy;
                }
#else
                var proxy = new AsyncUiAdapter(gameModel.PlayerModels[i]);
#endif
                _game.UiProxies.Add(player, proxy);
            }
            _game.GlobalProxy = new GlobalClientUiProxy(_game, activeClientProxy);
        }

        private Game _game;
        Thread gameThread;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            InitGame();
            gameThread = new Thread(_game.Run) { IsBackground = true };
            gameThread.Start();
        }

    }
}
