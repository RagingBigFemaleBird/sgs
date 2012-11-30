using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Lobby
{
    public enum RoomType
    {
        RoleOneDefector,
        RoleNoDefector,
        RoleTwoDefectors,
    }
    public enum Timeout
    {
        TenSeconds = 10,
        FifteenSeconds = 15,
        TwentySeconds = 20,
    }

    public class Room
    {
        object roomLock;

        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private RoomType type;

        public RoomType Type
        {
            get { return type; }
            set { type = value; }
        }
        private Timeout timeout;

        public Timeout Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }
        private bool inProgress;

        public bool InProgress
        {
            get { return inProgress; }
            set { inProgress = value; }
        }
        private bool spectatorDisabled;

        public bool SpectatorDisabled
        {
            get { return spectatorDisabled; }
            set { spectatorDisabled = value; }
        }
        private bool chatDisabled;

        public bool ChatDisabled
        {
            get { return chatDisabled; }
            set { chatDisabled = value; }
        }

        List<Seat> seats;

        public List<Seat> Seats
        {
            get { return seats; }
            private set { seats = value; }
        }

        private int ownerId;

        public int OwnerId
        {
            get { return ownerId; }
            set { ownerId = value; }
        }
    }
}
