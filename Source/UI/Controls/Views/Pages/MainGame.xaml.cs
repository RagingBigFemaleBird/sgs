#define NETWORKING
using System;
using System.Linq;
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
using System.Diagnostics;
using System.IO;
using System.Windows.Navigation;
using System.Collections.ObjectModel;
using Sanguosha.Core.Cards;
using Sanguosha.Lobby.Core;
using Sanguosha.Core.Utils;
using System.ComponentModel;

namespace Sanguosha.UI.Controls
{

    public delegate void NavigationEventHandler(object sender, NavigationService service);

    /// <summary>
    /// Interaction logic for MainGame.xaml
    /// </summary>
    public partial class MainGame : Page
    {
        public MainGame()
        {
            this.InitializeComponent();
            ctrlGetCard.OnCardSelected += new CardSelectedHandler(ctrlGetCard_OnCardSelected);
            ctrlGetSkill.OnSkillNameSelected += new SkillNameSelectedHandler(ctrlGetSkill_OnSkillNameSelected);
            gameEndEventHandler = gameView_OnGameCompleted;
            gameView.OnGameCompleted += gameEndEventHandler;
            gameView.OnUiAttached += gameView_OnUiAttached;
            // Insert code required on object creation below this point.
        }

        void gameView_OnUiAttached(object sender, EventArgs e)
        {
            gameView.OnUiAttached -= gameView_OnUiAttached;
            if (BackwardNavigationService != null)
            {
                BackwardNavigationService.Navigate(this);
                BackwardNavigationService = null;
            }
        }

        public static NavigationService BackwardNavigationService
        {
            get;
            set;
        }

        EventHandler gameEndEventHandler;

        void gameView_OnGameCompleted(object sender, EventArgs e)
        {
            gameView.OnGameCompleted -= gameEndEventHandler;
            Game.CurrentGameOverride = null;
            var handle = OnNavigateBack;
            if (handle != null)
            {
                handle(this, NavigationService);
            }
        }

        void ctrlGetSkill_OnSkillNameSelected(string skillName)
        {
            var gameModel = (gameView.DataContext as GameViewModel);
            gameModel.MainPlayerModel.CheatGetSkill(skillName);
        }

        void ctrlGetCard_OnCardSelected(Card card)
        {
            var gameModel = (gameView.DataContext as GameViewModel);
            gameModel.MainPlayerModel.CheatGetCard(card);
        }

        private Client _networkClient;

        public Client NetworkClient
        {
            get { return _networkClient; }
            set { _networkClient = value; }
        }

        private int? hasSeed;

        public int? HasSeed
        {
            get { return hasSeed; }
            set { hasSeed = value; }
        }

        private void InitGame()
        {
#if DEBUG
            TextWriterTraceListener twtl = new TextWriterTraceListener(System.IO.Path.Combine(Directory.GetCurrentDirectory(), AppDomain.CurrentDomain.FriendlyName + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + ".txt"));
            twtl.Name = "TextLogger";
            twtl.TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime;

            ConsoleTraceListener ctl = new ConsoleTraceListener(false);
            ctl.TraceOutputOptions = TraceOptions.DateTime;

            Trace.Listeners.Add(twtl);
            Trace.Listeners.Add(ctl);
            Trace.AutoFlush = true;

            Trace.WriteLine("Log starting");
#endif
            _game = new RoleGame();
            if (hasSeed != null)
            {
                _game.RandomGenerator = new Random((int)hasSeed);
                _game.IsPanorama = true;
            }
            Game.CurrentGameOverride = _game;
            if (NetworkClient != null)
            {
                var pkt = NetworkClient.Receive();
                if (pkt is ConnectionResponse)
                {
                    _game.Settings = ((ConnectionResponse)pkt).Settings;
                    NetworkClient.SelfId = ((ConnectionResponse)pkt).SelfId;
                }
                Trace.Assert(_game.Settings != null);
            }
            else
            {
                _game.Settings = new GameSettings();
                _game.Settings.Accounts = new List<Account>();
                _game.Settings.NumberOfDefectors = 1;
                _game.Settings.NumHeroPicks = 3;
                _game.Settings.DualHeroMode = false;
                _game.Settings.TotalPlayers = 8;
                _game.Settings.TimeOutSeconds = 15;
                for (int i = 0; i < 8; i++)
                {
                    _game.Settings.Accounts.Add(new Account() { UserName = "Robot" + i });
                }

            }
            foreach (var g in GameEngine.Expansions.Values)
            {
                _game.LoadExpansion(g);
            }
            ClientNetworkUiProxy activeClientProxy = null;
            if (NetworkClient != null)
            {
                for (int i = 0; i < _game.Settings.TotalPlayers; i++)
                {
                    Player player = new Player();
                    player.Id = i;
                    _game.Players.Add(player);
                }
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    Player player = new Player();
                    player.Id = i;
                    _game.Players.Add(player);
                }
            }

            if (NetworkClient != null)
            {
                _game.GameClient = NetworkClient;
            }
            else
            {
                _game.GlobalProxy = new GlobalDummyProxy();
            }
            GameViewModel gameModel = new GameViewModel();
            gameModel.Game = _game;
            if (NetworkClient != null)
            {
                gameModel.MainPlayerSeatNumber = NetworkClient.SelfId >= _game.Players.Count ? 0 : NetworkClient.SelfId;
            }
            else
            {
                gameModel.MainPlayerSeatNumber = 0;
            }
            
            _game.NotificationProxy = gameView;
            List<ClientNetworkUiProxy> inactive = new List<ClientNetworkUiProxy>();
            for (int i = 0; i < _game.Players.Count; i++)
            {
                var player = gameModel.PlayerModels[i].Player;
                if (NetworkClient != null)
                {
                    var proxy = new ClientNetworkUiProxy(
                                new AsyncUiAdapter(gameModel.PlayerModels[i]), NetworkClient, i == 0);
                    proxy.HostPlayer = player;
                    proxy.TimeOutSeconds = _game.Settings.TimeOutSeconds;
                    if (i == 0)
                    {
                        activeClientProxy = proxy;
                    }
                    else
                    {
                        inactive.Add(proxy);
                    }
                    _game.UiProxies.Add(player, proxy);
                }
                else
                {
                    var proxy = new AsyncUiAdapter(gameModel.PlayerModels[i]);
                    _game.UiProxies.Add(player, proxy);
                }
            }
            if (NetworkClient != null)
            {
                _game.ActiveClientProxy = activeClientProxy;
                _game.GlobalProxy = new GlobalClientUiProxy(_game, activeClientProxy, inactive);
                _game.IsUiDetached = _game.IsUiDetached;
            }
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {                
                gameView.DataContext = gameModel;
                if (BackwardNavigationService != null && !ViewModelBase.IsDetached)
                {
                    BackwardNavigationService.Navigate(this);
                    BackwardNavigationService = null;
                }
            });
            _game.Run();            
        }

        private Game _game;
        
        Thread gameThread;

        public void Start()
        {
            gameThread = new Thread(InitGame) { IsBackground = true };
            gameThread.Start();            
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void btnGetCard_Click(object sender, RoutedEventArgs e)
        {
            windowGetCard.Show();
            ObservableCollection<CardViewModel> model = new ObservableCollection<CardViewModel>();

            foreach (var card in _game.OriginalCardSet)
            {
                if (card.Id > 0 && !(card.Type is HeroCardHandler) && !(card.Type is RoleCardHandler))
                {
                    model.Add(new CardViewModel() { Card = card });
                }
            }
            ctrlGetCard.DataContext = model;
            
        }

        private void btnGetSkill_Click(object sender, RoutedEventArgs e)
        {
            windowGetSkill.Show();
            ObservableCollection<HeroViewModel> model = new ObservableCollection<HeroViewModel>();

            foreach (var card in _game.OriginalCardSet)
            {
                if (card.Id > 0 && (card.Type is HeroCardHandler))
                {
                    string exp = string.Empty;
                    var exps = from expansion in GameEngine.Expansions.Keys
                               where GameEngine.Expansions[expansion].CardSet.Contains(card)
                               select expansion;                    
                    if (exps.Count() > 0)
                    {
                        exp = exps.First();
                    }
                    model.Add(new HeroViewModel() 
                    {
                        Id = card.Id,
                        Hero = (card.Type as HeroCardHandler).Hero,                        
                        ExpansionName = exp
                    });
                }
            }
            ctrlGetSkill.DataContext = model;
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

        public event NavigationEventHandler OnNavigateBack;

        private void btnGoBack_Click(object sender, RoutedEventArgs e)
        {
            if (_game != null)
            {
                _game.Abort();
            }
            Game.CurrentGameOverride = null;
            var handle = OnNavigateBack;
            if (handle != null)
            {
                handle(this, NavigationService);
            }
        }

    }
}
