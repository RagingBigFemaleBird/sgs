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
            }
            catch (DirectoryNotFoundException)
            {
            }
            AsyncLoadCompleted = true;
        }

        public static string ExpansionFolder = "./";
        public static string ResourcesFolder = "Resources";

        private bool _asyncLoadCompleted;

        internal bool AsyncLoadCompleted
        {
            get { return _asyncLoadCompleted; }
            set 
            {
                _asyncLoadCompleted = value;
                _UpdateStartButton();
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
                startButton.IsEnabled = _startButtonEnabled && _asyncLoadCompleted;
            }
            else
            {
                Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
                {
                    startButton.IsEnabled = _startButtonEnabled && _asyncLoadCompleted;
                });
            }
        }

        private Thread loadingThread;

        private void _Load()
        {
            _asyncLoadCompleted = false;
            _startButtonEnabled = true; // @todo: change this.
            _LoadResources(ResourcesFolder);
            GameEngine.LoadExpansions(ExpansionFolder);

        }

        public Login()
        {
            loadingThread = new Thread(_Load) { IsBackground = true };
            loadingThread.Start();
            InitializeComponent();
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (loginTab.SelectedIndex == 0)
            {                
                Client client;
                int mainSeat = 0;
                try
                {
                    client = new Client();
                    string addr = tab0HostName.Text;
                    string[] args = addr.Split(':');
                    client.IpString = args[0];
                    if (args.Length >= 2)
                    {
                        client.PortNumber = int.Parse(args[1]);
                    }
                    else
                    {
                        client.PortNumber = 12345;
                    }
                    client.Start();
                    mainSeat = (int)client.Receive();
                    client.SelfId = mainSeat;
                }
                catch(Exception)
                {
                    MessageBox.Show("Failed to create connection");
                    return;
                }
                MainGame game = new MainGame();
                game.MainSeat = mainSeat;
                game.NetworkClient = client;
                
                this.NavigationService.Navigate(game);
            }
        }
    }
}
