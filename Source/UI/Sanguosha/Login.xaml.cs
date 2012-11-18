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
            }
            else
            {                
                _UpdateStartButton();
            }
            InitializeComponent();
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (loginTab.SelectedIndex == 0)
            {                
                Client client;
                int mainSeat = 0;

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

                busyIndicator.IsBusy = true;

                //client.Start(isReplay, FileStream = file.open(...))
                BackgroundWorker worker = new BackgroundWorker();
                
                worker.DoWork += (o, ea) =>
                {
                    try
                    {
                        ea.Result = false;
                        client.Start();
                        mainSeat = (int)client.Receive();
                        client.SelfId = mainSeat;
                        ea.Result = true;
                    }
                    catch (Exception)
                    {
                    }                        
                };

                worker.RunWorkerCompleted += (o, ea) =>
                {
                    if ((bool)ea.Result)
                    {
                        MainGame game = new MainGame();
                        game.MainSeat = mainSeat;
                        game.NetworkClient = client;
                        this.NavigationService.Navigate(game);
                        busyIndicator.IsBusy = false;
                        return;
                    }
                    else
                    {
                        MessageBox.Show("Failed to create connection");
                    }
                };

                worker.RunWorkerAsync();
            }
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
