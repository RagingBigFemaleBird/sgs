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
using System.Windows.Threading;

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
            _updateRoomTimer = new DispatcherTimer();
            _updateRoomTimer.Interval = TimeSpan.FromSeconds(3);
            _updateRoomTimer.Tick += timer_Tick;
        }

        private static NavigationService globalNavigationService;

        public static NavigationService GlobalNavigationService
        {
            get
            {
                return globalNavigationService;
            }
            set
            {
                if (globalNavigationService == value) return;
                if (globalNavigationService != null)
                    globalNavigationService.Navigating -= LobbyView.Instance.NavigationService_Navigating;
                globalNavigationService = value;
                if (globalNavigationService != null)
                    globalNavigationService.Navigating += LobbyView.Instance.NavigationService_Navigating;
            }
        }

        
        void NavigationService_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Content == this)
            {
                LobbyModel.OnChat += LobbyModel_OnChat;
                _updateRoomTimer.Start();
                busyIndicator.IsBusy = false;
                try
                {
                    LobbyViewModel.Instance.UpdateRooms();
                }
                catch (Exception)
                {
                    var handler = OnNavigateBack;
                    if (handler != null)
                    {
                        handler(this, GlobalNavigationService);
                    }
                }
            }
            else
            {
                _updateRoomTimer.Stop();
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                LobbyViewModel.Instance.UpdateRooms();
            }
            catch (Exception)
            {
            }
        }

        DispatcherTimer _updateRoomTimer;

        void LobbyModel_OnChat(string userName, string msg)
        {
            Trace.Assert(chatBox != null && chatBox.Document != null);
            try
            {
                chatBox.Document.Blocks.Add(LogFormatter.RichTranslateChat(string.Empty, userName, msg));
                chatBox.ScrollToEnd();
            }
            catch (Exception)
            {
                Trace.Assert(false);
            }
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

            LobbyViewModel.Instance.OnChat -= LobbyModel_OnChat;
            chatBox.Document.Blocks.Clear();

            //client.Start(isReplay, FileStream = file.open(...))
            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += (o, ea) =>
            {
                try
                {
                    ea.Result = false;
                    var stream = FileRotator.CreateFile("./Replays", "SGSREPLAY", ".sgs", 10);
                    
                    stream.Write(BitConverter.GetBytes((int)0), 0, 4);
                    client.RecordStream = stream;
                    client.Start(stream, LobbyModel.LoginToken);
                    
                    MainGame game = null;

                    Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
                    {
                        try
                        {
                            game = new MainGame();
                            game.OnNavigateBack += (oo, s) =>
                            {
                                s.Navigate(this);
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
                    return;
                }
                else
                {
                    busyIndicator.IsBusy = false;
                    LobbyViewModel.Instance.OnChat += LobbyModel_OnChat;
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


        private void btnCreateRoomConfirm_Click(object sender, RoutedEventArgs e)
        {
            RoomSettings settings = new RoomSettings();
            settings.IsDualHeroMode = cbDualHero.IsChecked == true;
            settings.NumberOfDefectors = cbDualDefector.IsChecked == true ? 2 : 1;
            int[] options1 = { 3, 4, 5, 6 };
            settings.NumHeroPicks = options1[cbHeroPickCount.SelectedIndex];
            int[] options2 = { 10, 15, 20, 30 };
            settings.TimeOutSeconds = options2[cbTimeOutSeconds.SelectedIndex];
            settings.EnabledPackages = EnabledPackages.None;
            if (cbWind.IsChecked == true) settings.EnabledPackages |= EnabledPackages.Wind;
            if (cbFire.IsChecked == true) settings.EnabledPackages |= EnabledPackages.Fire;
            if (cbWoods.IsChecked == true) settings.EnabledPackages |= EnabledPackages.Woods;
            if (cbHills.IsChecked == true) settings.EnabledPackages |= EnabledPackages.Hills;
            if (cbGods.IsChecked == true) settings.EnabledPackages |= EnabledPackages.Gods;
            if (cbSP.IsChecked == true) settings.EnabledPackages |= EnabledPackages.SP;
            if (cbOverKnightFame.IsChecked == true) settings.EnabledPackages |= EnabledPackages.OverKnightFame;

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
