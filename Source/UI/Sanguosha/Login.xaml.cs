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
using System.Threading;
using System.IO;
using Sanguosha.Core.Games;
using Sanguosha.Core.Network;
using Microsoft.Win32;
using System.ComponentModel;
using Sanguosha.UI.Controls;
using Sanguosha.Lobby.Core;
using System.ServiceModel;
using Sanguosha.Lobby.Server;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Security;
using Sanguosha.Core.Utils;
using System.Diagnostics;

namespace Sanguosha.UI.Main
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        public static int DefaultLobbyPort = 6080;

        private static string[] _dictionaryNames = new string[] { "Cards.xaml", "Skills.xaml", "Game.xaml" };

        private void _LoadResources(string folderPath)
        {
            try
            {
                var files = Directory.GetFiles(string.Format("{0}/Texts", folderPath));
                foreach (var filePath in files)
                {
                    if (!_dictionaryNames.Any(fileName => filePath.Contains(fileName))) continue;
                    try
                    {
                        Uri uri = new Uri(string.Format("pack://siteoforigin:,,,/{0}", filePath));
                        Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = uri });
                    }
                    catch (BadImageFormatException)
                    {
                        continue;
                    }
                }
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/Resources;component/Lobby.xaml") });
                GameSoundLocator.Initialize();
            }
            catch (DirectoryNotFoundException)
            {
            }
            PreloadCompleted = true;
            _UpdateStartButton();
        }

        public static string ExpansionFolder = "./";
        public static string ResourcesFolder = "Resources";

        private static bool _preloadCompleted = false;

        internal static bool PreloadCompleted
        {
            get { return _preloadCompleted; }
            set 
            {
                _preloadCompleted = value;
            }
        }

        private bool _startButtonEnabled;

        internal bool StartButtonEnabled
        {
            get { return _startButtonEnabled; }
            set 
            {
                _startButtonEnabled = value;
                _UpdateStartButton(); 
            }
        }

        private void _UpdateStartButton()
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                startButton.IsEnabled = _startButtonEnabled && _preloadCompleted;
            }
            else
            {
                Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
                {
                    startButton.IsEnabled = _startButtonEnabled && _preloadCompleted;
                });
            }
        }

        private Thread loadingThread;

        private void _Load()
        {
            _LoadResources(ResourcesFolder);
            
            GameEngine.LoadExpansions(ExpansionFolder);

        }

        public Login()
        {
            _startButtonEnabled = true; // @todo: change this.
            if (!PreloadCompleted)
            {

                loadingThread = new Thread(_Load) { IsBackground = true };
                loadingThread.Start();
                InitializeComponent();
            }
            else
            {
                InitializeComponent();
                _UpdateStartButton();
            }
            tab0UserName.Text = Properties.Settings.Default.LastUserName;
            tab0HostName.Text = Properties.Settings.Default.LastHostName;
            tab1Port.Text = DefaultLobbyPort.ToString();            
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (loginTab.SelectedIndex == 0)
            {
                _startClient();
            }
            else if (loginTab.SelectedIndex == 1)
            {
                _startServer();
            }
        }

        private void _startClient()
        {
            string userName = tab0UserName.Text;
            string passwd = tab0Password.Text;
            Properties.Settings.Default.LastHostName = tab0HostName.Text;
            Properties.Settings.Default.LastUserName = tab0UserName.Text;
            Properties.Settings.Default.Save();
#if !DEBUG
            if (string.IsNullOrEmpty(userName))
            {
                _Warn("Please provide a username");
                return;
            }
#endif
            busyIndicator.BusyContent = Resources["Busy.ConnectServer"];
            busyIndicator.IsBusy = true;
            ILobbyService server = null;
            LoginToken token = new LoginToken();
            string reconnect = null;
            string hostName = tab0HostName.Text;
            if (!hostName.Contains(":"))
            {
                hostName = hostName + ":" + DefaultLobbyPort;
            }

            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += (o, ea) =>
            {
                try
                {
                    ea.Result = LoginStatus.UnknownFailure;
                    var lobbyModel = LobbyViewModel.Instance;
                    var binding = new NetTcpBinding();
                    binding.Security.Mode = SecurityMode.None;
                    var endpoint = new EndpointAddress(string.Format("net.tcp://{0}/GameService", hostName));
                    var channelFactory = new DuplexChannelFactory<ILobbyService>(lobbyModel, binding, endpoint);
                    server = channelFactory.CreateChannel();
                    Account ret;
                    var stat = server.Login(Misc.ProtocolVersion, userName, passwd, out token, out ret, out reconnect);
                    if (stat == LoginStatus.Success)
                    {
                        LobbyViewModel.Instance.CurrentAccount = ret;
                        
                        if (reconnect != null)
                        {
                            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
                            {
                                MainGame.BackwardNavigationService = this.NavigationService;
                                busyIndicator.BusyContent = Resources["Busy.Reconnecting"];
                            });                            
                        }
                    }                    
                    ea.Result = stat;
                }
                catch (Exception e)
                {
                    string s = e.StackTrace;
                }
            };

            worker.RunWorkerCompleted += (o, ea) =>
            {
                bool success = false;
                if ((LoginStatus)ea.Result == LoginStatus.Success)
                {
                    LobbyView lobby = LobbyView.Instance;                    
                    LobbyView.Instance.OnNavigateBack += lobby_OnNavigateBack;
                    var lobbyModel = LobbyViewModel.Instance;
                    lobbyModel.Connection = server;
                    lobbyModel.LoginToken = token;

                    if (reconnect == null)
                    {
                        this.NavigationService.Navigate(lobby);
                        busyIndicator.IsBusy = false;
                    }
                    else
                    {
                        lobbyModel.NotifyGameStart(reconnect);
                        busyIndicator.IsBusy = true;
                    }

                    success = true;
                }
                if (!success)
                {
                    if ((LoginStatus)ea.Result == LoginStatus.OutdatedVersion)
                    {
                        MessageBox.Show("Outdated version. Please update");
                    }
                    else if ((LoginStatus)ea.Result == LoginStatus.InvalidUsernameAndPassword)
                    {
                        MessageBox.Show("Invalid Username and Password");
                    }
                    else
                    {
                        MessageBox.Show("Failed to launch client");
                    }
                    busyIndicator.IsBusy = false;
                }
            };

            worker.RunWorkerAsync();
        }

        private void _createAccount()
        {
            string userName = tab0UserName.Text;
            string passwd = tab0Password.Text;
            Properties.Settings.Default.LastHostName = tab0HostName.Text;
            Properties.Settings.Default.LastUserName = tab0UserName.Text;            
            Properties.Settings.Default.Save();
#if !DEBUG
            if (string.IsNullOrEmpty(userName))
            {
                _Warn("Please provide a username");
                return;
            }
#endif
            busyIndicator.BusyContent = Resources["Busy.ConnectServer"];
            busyIndicator.IsBusy = true;
            ILobbyService server = null;
            string hostName = tab0HostName.Text;
            if (!hostName.Contains(":"))
            {
                hostName = hostName + ":" + DefaultLobbyPort;
            }

            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += (o, ea) =>
            {
                try
                {
                    ea.Result = LoginStatus.UnknownFailure;
                    var binding = new NetTcpBinding();
                    binding.Security.Mode = SecurityMode.None;
                    var endpoint = new EndpointAddress(string.Format("net.tcp://{0}/GameService", hostName));
                    var channelFactory = new DuplexChannelFactory<ILobbyService>(LobbyViewModel.Instance, binding, endpoint);
                    server = channelFactory.CreateChannel();                    
                    var stat = server.CreateAccount(userName, passwd);
                    ea.Result = stat;
                }
                catch (Exception e)
                {
                    string s = e.StackTrace;
                }
            };

            worker.RunWorkerCompleted += (o, ea) =>
            {
                busyIndicator.IsBusy = false;
                switch((LoginStatus)ea.Result)
                {
                    case LoginStatus.Success:
                        MessageBox.Show("Account created successfully");
                        break;
                    case LoginStatus.OutdatedVersion:                    
                        MessageBox.Show("Outdated version. Please update.");
                        break;                    
                    case LoginStatus.InvalidUsernameAndPassword:
                        MessageBox.Show("Invalid Username and Password.");
                        break;
                    default: 
                        MessageBox.Show("Failed to launch client.");
                        break;
                }
            };

            worker.RunWorkerAsync();
        }

        private void _Warn(string message)
        {
            MessageBox.Show(message, "Error");
        }

        private void _startServer()
        {            
            LobbyServiceImpl gameService = null;
            ServiceHost host = null;
            IPAddress serverIp = tab1IpAddresses.SelectedItem as IPAddress;
            if (serverIp == null)
            {
                _Warn("Please select an IP address");
                return;
            }
            int portNumber;
            if (!int.TryParse(tab1Port.Text, out portNumber))
            {
                _Warn("Please enter a legal port number");
                return;
            }
            busyIndicator.BusyContent = Resources["Busy.LaunchServer"];
            busyIndicator.IsBusy = true;

            //client.Start(isReplay, FileStream = file.open(...))
            BackgroundWorker worker = new BackgroundWorker();
            bool noDatabase = !(tab1EnableDb.IsChecked == true);

            worker.DoWork += (o, ea) =>
            {
                try
                {
                    ea.Result = false;
                    gameService = new LobbyServiceImpl(noDatabase);
                    gameService.HostingIp = serverIp;

                    host = new ServiceHost(gameService);
                    //, new Uri[] { new Uri(string.Format("net.tcp://{0}:{1}/GameService", serverIp, portNumber)) });
                    var binding = new NetTcpBinding();

                    binding.ReceiveTimeout = new TimeSpan(1, 0, 0);
                    binding.SendTimeout = new TimeSpan(1, 0, 0);
                    binding.OpenTimeout = new TimeSpan(1, 0, 0);
                    binding.CloseTimeout = new TimeSpan(1, 0, 0);

                    binding.Security.Mode = SecurityMode.None;
                    binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
                    binding.Security.Transport.ProtectionLevel = ProtectionLevel.None;
                    binding.Security.Message.ClientCredentialType = MessageCredentialType.None;

                    host.AddServiceEndpoint(typeof(ILobbyService), binding, string.Format("net.tcp://{0}:{1}/GameService", serverIp, portNumber));

                    host.Open();
                    ea.Result = true;
                }
                catch (Exception)
                {
                }
            };

            worker.RunWorkerCompleted += (o, ea) =>
            {
                busyIndicator.IsBusy = false;
                if ((bool)ea.Result)
                {
                    ServerPage serverPage = new ServerPage();
                    serverPage.Host = host;
                    serverPage.GameService = gameService;
                    this.NavigationService.Navigate(serverPage);
                    return;
                }
                else
                {
                    MessageBox.Show("Failed to launch server");
                }
            };

            worker.RunWorkerAsync();
        }

        private void btnReplay_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists("./Replays"))
            {
                Directory.CreateDirectory("./Replays");
            }
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = Directory.GetCurrentDirectory() + "\\Replays";
            dlg.DefaultExt = ".sgs"; // Default file extension
            dlg.Filter = "Replay File (.sgs)|*.sgs|All Files (*.*)|*.*"; // Filter files by extension
            bool? result = dlg.ShowDialog();
            if (result != true) return;

            string fileName = dlg.FileName;
            
            Client client;
            MainGame game = null;
            try
            {
                client = new Client();                
                game = new MainGame();
                game.OnNavigateBack += game_OnNavigateBack;
                client.StartReplay(File.Open(fileName, FileMode.Open));
                game.NetworkClient = client;
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to open replay file.");
                return;
            }
            if (game != null)
            {
                MainGame.BackwardNavigationService = this.NavigationService;
                game.Start();
                // this.NavigationService.Navigate(game);
            }
        }

        private static void game_OnNavigateBack(object sender, NavigationService service)
        {
            MainGame game = sender as MainGame;
            if (game != null) game.OnNavigateBack -= game_OnNavigateBack;
            service.Navigate(new Login());            
        }
        
        private static void lobby_OnNavigateBack(object sender, NavigationService service)
        {
            LobbyView lobby = sender as LobbyView;
            if (lobby != null) lobby.OnNavigateBack -= lobby_OnNavigateBack;
            service.Navigate(new Login());
        }

        #region Network Related

        private void _ListAdaptors()
        {
            tab1Adaptors.Items.Clear();
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (!tab1ShowAllAdaptor.IsChecked == true)
                {
                    if (nic.NetworkInterfaceType != NetworkInterfaceType.Ethernet &&
                        nic.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)
                    {
                        continue;
                    }
                }
                tab1Adaptors.Items.Add(nic);
            }
        }

        private void _ListIpAddresses()
        {
            tab1IpAddresses.Items.Clear();
            NetworkInterface ni = tab1Adaptors.SelectedItem as NetworkInterface;
            if (ni == null) return;
            foreach (var ip in ni.GetIPProperties().UnicastAddresses)
            {
                tab1IpAddresses.Items.Add(ip.Address);
            }
        }
        
        private void tab1ShowAllAdaptor_Checked(object sender, RoutedEventArgs e)
        {
            _ListAdaptors();
        }

        private void tab1ShowAllAdaptor_Unchecked(object sender, RoutedEventArgs e)
        {
            _ListAdaptors();

        }

        private bool _adaptorSearched;
        private void loginTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_adaptorSearched && loginTab.SelectedIndex == 1)
            {
                _ListAdaptors();
                _adaptorSearched = true;
            }
        }

        private void tab1Adaptors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ListIpAddresses();
        }

        private void tab1ClearDb_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LobbyServiceImpl.WipeDatabase();
        }

        private void btnRegister_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _createAccount();
        }
        #endregion
    }
}
