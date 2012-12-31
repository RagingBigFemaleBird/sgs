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

namespace Sanguosha.UI.Main
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Page
    {
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
            busyIndicator.BusyContent = Resources["Busy.ConnectServer"];
            busyIndicator.IsBusy = true;
            ILobbyService server = null;
            LoginToken token = new LoginToken();
            string userName = tab0UserName.Text;

            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += (o, ea) =>
            {
                try
                {
                    ea.Result = false;
                    var lobbyModel = LobbyViewModel.Instance;
                    var channelFactory = new DuplexChannelFactory<ILobbyService>(lobbyModel, "GameServiceEndpoint");
                    server = channelFactory.CreateChannel();     
                    if (server.Login(1, userName, out token) == LoginStatus.Success)
                    {
                        ea.Result = true;
                    }
                }
                catch (Exception e)
                {
                    string s = e.StackTrace;
                }
            };

            worker.RunWorkerCompleted += (o, ea) =>
            {
                busyIndicator.IsBusy = false;
                bool success = false;
                if ((bool)ea.Result)
                {
                    LobbyView lobby = new LobbyView();
                    var lobbyModel = LobbyViewModel.Instance;
                    lobbyModel.Connection = server;
                    lobbyModel.LoginToken = token;
                    lobbyModel.UpdateRooms();
                    this.NavigationService.Navigate(lobby);
                    success = true;
                }

                if (!success)
                {
                    MessageBox.Show("Failed to launch client");
                }
            };

            worker.RunWorkerAsync();
        }

        private void _startServer()
        {
            busyIndicator.BusyContent = Resources["Busy.LaunchServer"];
            busyIndicator.IsBusy = true;
            ServiceHost host = null;

            //client.Start(isReplay, FileStream = file.open(...))
            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += (o, ea) =>
            {
                try
                {
                    ea.Result = false;
                    var gameService = new LobbyServiceImpl();
                    host = new ServiceHost(gameService);
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
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".sgs"; // Default file extension
            dlg.Filter = "Replay File (.sgs)|*.sgs|All Files (*.*)|*.*"; // Filter files by extension
            bool? result = dlg.ShowDialog();
            if (result != true) return;

            string fileName = dlg.FileName;
            
            Client client;
            int mainSeat = 0;
            MainGame game = null;
            try
            {
                client = new Client();
                client.Start(true, File.Open(fileName, FileMode.Open));
                mainSeat = (int)client.Receive();
                client.SelfId = mainSeat;
                game = new MainGame();
                game.MainSeat = mainSeat;
                game.NetworkClient = client;
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to open replay file.");
                return;
            }
            if (game != null)
            {            
                this.NavigationService.Navigate(game);
            }

        }
    }
}
