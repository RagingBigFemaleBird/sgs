using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Sanguosha.UI.Controls
{
    public class LobbyViewModel : ViewModelBase
    {
        public LobbyViewModel()
        {
            Rooms = new ObservableCollection<RoomViewModel>();
        }

        private RoomViewModel _currentRoom;

        public RoomViewModel CurrentRoom
        {
            get
            {
                return _currentRoom;
            }
            set
            {
                if (_currentRoom == value) return;
                _currentRoom = value;
                OnPropertyChanged("CurrentRoom");
            }
        }

        private ObservableCollection<RoomViewModel> _rooms;

        public ObservableCollection<RoomViewModel> Rooms
        {
            get
            {
                return _rooms;
            }
            private set
            {
                if (_rooms == value) return;
                _rooms = value;
                OnPropertyChanged("Rooms");
            }
        }
    }
}
