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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Sanguosha.Lobby.Core;

namespace Sanguosha.UI.Controls
{
    public enum LobbyMainButtonState
    {
        Ready,
        Cancel,
        Start,
        Spectate
    }

    public class CurrentRoomAndSeatToButtonStateConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == null || values.Length != 2) return LobbyMainButtonState.Start;
            var room = values[0] as RoomViewModel;
            var seat = values[1] as SeatViewModel;
            if (room == null) return LobbyMainButtonState.Start;
            if (seat == null)
            {
                if (room.State == RoomState.Gaming) return LobbyMainButtonState.Spectate;
            }
            else
            {
                switch (seat.State)
                {
                    case SeatState.GuestReady:
                        return LobbyMainButtonState.Cancel;
                    case SeatState.GuestTaken:
                        return LobbyMainButtonState.Ready;
                    case SeatState.Host:
                        return LobbyMainButtonState.Start;
                }
            }
            return LobbyMainButtonState.Start;            
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for LobbyMainButton.xaml
    /// </summary>
    public partial class LobbyMainButton : UserControl
    {
        public LobbyMainButton()
        {
            this.InitializeComponent();
        }

        public LobbyMainButtonState State
        {
            get { return (LobbyMainButtonState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for State.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(LobbyMainButtonState), typeof(LobbyMainButton), new UIPropertyMetadata(LobbyMainButtonState.Start, OnStateChanged));
        
        private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = d as LobbyMainButton;
            if (button == null) return;
            button.startButton.Visibility = Visibility.Hidden;
            button.readyButton.Visibility = Visibility.Hidden;
            button.cancelButton.Visibility = Visibility.Hidden;
            button.spectateButton.Visibility = Visibility.Hidden;
            switch(button.State)
            {
                case LobbyMainButtonState.Start:
                    button.startButton.Visibility = Visibility.Visible;
                    break;
                case LobbyMainButtonState.Cancel:
                    button.cancelButton.Visibility = Visibility.Visible;
                    break;
                case LobbyMainButtonState.Ready:
                    button.readyButton.Visibility = Visibility.Visible;
                    break;
                case LobbyMainButtonState.Spectate:
                    button.spectateButton.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}