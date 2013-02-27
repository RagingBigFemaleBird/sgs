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
            chatEventHandler =  new ChatEventHandler(LobbyModel_OnChat);
            LobbyModel.OnChat += chatEventHandler;
        }

        private ChatEventHandler chatEventHandler;

        void LobbyModel_OnChat(string userName, string msg)
        {            
            chatBox.Document.Blocks.Add(LogFormatter.RichTranslateChat(string.Empty, userName, msg));
            chatBox.ScrollToEnd();
        }

        private static LobbyView _instance;

        /// <summary>
        /// Gets the singleton instance of <c>LobbyViewModel</c>.
        /// </summary>
        public static LobbyView Instance
        {
            get
            {
                if (_instance == null) _instance = new LobbyView();        
                return _instance;
            }
        }

        public LobbyViewModel LobbyModel
        {
            get
            {
                return LobbyViewModel.Instance;
            }            
        }

        public void StartGame()
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
                    client.Start(null, LobbyModel.LoginToken);
                    client.RecordStream = FileRotator.CreateFile("./Replays", "SGSREPLAY", ".sgs", 10);
                    
                    MainGame game = null;

                    Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
                    {
                        try
                        {
                            game = new MainGame();
                            game.OnNavigateBack += (oo, s) =>
                            {
                                s.Navigate(this);
                                this.Reload();
                            };
                            game.NetworkClient = client;
                            if (NavigationService != null)
                            {
                                MainGame.BackwardNavigationService = this.NavigationService;
                            }
                            else
                            {
                                ViewModelBase.IsDetached = true;
                            }
                        }
                        catch (Exception)
                        {
                            game = null;
                        }
                    });

                    if (game != null)
                    {
                        game.Start();
                        ea.Result = true;
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceError("Connection failure : " + e.StackTrace);
                    Trace.Assert(false);
                }
            };

            worker.RunWorkerCompleted += (o, ea) =>
            {                
                if ((bool)ea.Result)
                {
                    chatBox.Document.Blocks.Clear();
                    LobbyViewModel.Instance.OnChat -= chatEventHandler;                    
                    this.DataContext = null;                            
                    return;
                }
                else
                {
                    busyIndicator.IsBusy = false;
                    MessageBox.Show("Failed to create connection for " + LobbyModel.GameServerConnectionString);
                    Trace.Assert(false);
                }
            };

            worker.RunWorkerAsync();
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
            if (model != null && (LobbyViewModel.Instance.CurrentSeat == null || LobbyViewModel.Instance.ExitRoom()))
            {
                LobbyModel.CurrentRoom = model;
            }            
        }

        private void enterRoomButton_Click(object sender, RoutedEventArgs e)
        {
            Trace.Assert(sender is Button);
            var model = (sender as Button).DataContext as RoomViewModel;
            if (model != null  && (LobbyViewModel.Instance.CurrentSeat == null || LobbyViewModel.Instance.ExitRoom()))
            {
                LobbyModel.CurrentRoom = model;
                if (!LobbyModel.EnterRoom())
                {
                    LobbyModel.CurrentRoom = null; 
                }                
            }            
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LobbyViewModel.Instance.UpdateRooms();
            DataContext = LobbyViewModel.Instance;
        }

        public void NotifyKeyEvent(string p)
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                FlowDocument doc = new FlowDocument();
                doc.Blocks.Add(new Paragraph(new Run(p)));
                keyEventNotifier.AddLog(doc);
            });
        }

        public void Reload()
        {
            LobbyModel.OnChat += chatEventHandler;
            busyIndicator.IsBusy = false;
            LobbyViewModel.Instance.UpdateRooms();
        }

        private void btnCreateRoomConfirm_Click(object sender, RoutedEventArgs e)
        {
            RoomSettings settings = new RoomSettings();
            settings.IsDualHeroMode = cbDualHero.IsChecked == true;
            settings.NumberOfDefectors = cbDualDefector.IsChecked == true ? 2 : 1;
            int[] options1 = { 3, 4, 5, 6 };
            settings.NumHeroPicks = options1[cbHeroPickCount.SelectedIndex];
            int[] options2 = { 10, 15, 20, 30 };
            settings.TimeOutSeconds = options2[cbTimeOutSeconds.SelectedIndex];
            LobbyModel.CreateRoom(settings);
            createRoomWindow.Close();
        }

        private void btnCreateRoomCancel_Click(object sender, RoutedEventArgs e)
        {
            createRoomWindow.Close();
        }

        private void btnCreateRoom_Click(object sender, RoutedEventArgs e)
        {
            cbDualHero.IsChecked = false;
            createRoomWindow.Show();
        }

        public event NavigationEventHandler OnNavigateBack;

        private void btnGoback_Click(object sender, RoutedEventArgs e)
        {
            LobbyViewModel.Instance.Logout();
            var handle = OnNavigateBack;
            if (handle != null)
            {
                handle(this, NavigationService);
            }
        }
    }

    public class RoomButtonVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == null) return Visibility.Collapsed;
            Trace.Assert(values.Length == 3);
            int room;
            int currentRoom;
            try
            {
                room = (int)values[0];
                currentRoom = (int)values[1];
            }
            catch (Exception)
            {
                return Visibility.Visible;
            }
            var currentSeat = values[2];            
            if ((parameter as string) == "View")
            {
                if (room == currentRoom) return Visibility.Collapsed;
                else return Visibility.Visible;
            }
            else
            {
                Trace.Assert((parameter as string) == "Enter");
                if (room == currentRoom && currentSeat != null) return Visibility.Collapsed;
                else return Visibility.Visible;
            }            
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
