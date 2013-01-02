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
using Sanguosha.Lobby.Core;
using System.Diagnostics;
using Sanguosha.Core.Network;
using System.ComponentModel;
using System.Threading;
using Sanguosha.Core.Utils;

namespace Sanguosha.UI.Controls
{
    /// <summary>
    /// Interaction logic for LobbyView.xaml
    /// </summary>
    public partial class LobbyView : Page
    {
        public LobbyView()
        {
            InitializeComponent();
        }

        public LobbyViewModel LobbyModel
        {
            get
            {
                return LobbyViewModel.Instance;
            }            
        }

        private void _StartGame()
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                Client client;

                client = new Client();
                string addr = LobbyModel.GameServerConnectionString;
                string[] args = addr.Split(':');
                Trace.Assert(args.Length == 2);
                client.IpString = args[0];                
                client.PortNumber = int.Parse(args[1]);                

                busyIndicator.BusyContent = Resources["Busy.JoinGame"];
                busyIndicator.IsBusy = true;

                //client.Start(isReplay, FileStream = file.open(...))
                BackgroundWorker worker = new BackgroundWorker();

                worker.DoWork += (o, ea) =>
                {
                    try
                    {
                        ea.Result = false;
                        client.Start();
                        client.RecordStream = FileRotator.CreateFile("./Replays", "SGSREPLAY", ".sgs", 10);
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
                        MainGame game = new MainGame();
                        game.NetworkClient = client;
                        this.NavigationService.Navigate(game);
                        return;
                    }
                    else
                    {
                        MessageBox.Show("Failed to create connection for " + LobbyModel.GameServerConnectionString);
                    }
                };

                worker.RunWorkerAsync();
            });
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

        private void viewRoomButton_Click(object sender, RoutedEventArgs e)
        {
            Trace.Assert(sender is Button);
            var model = (sender as Button).DataContext as RoomViewModel;
            if (model != null)
            {
                LobbyModel.CurrentRoom = model;
            }
        }

        private void enterRoomButton_Click(object sender, RoutedEventArgs e)
        {
            Trace.Assert(sender is Button);
            var model = (sender as Button).DataContext as RoomViewModel;
            if (model != null)
            {
                LobbyModel.CurrentRoom = model;
                LobbyModel.EnterRoom();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LobbyViewModel.Instance.UpdateRooms();
            DataContext = LobbyViewModel.Instance;
            LobbyModel.OnGameInitiated += (o, ea) => _StartGame();
        }
    }
}
