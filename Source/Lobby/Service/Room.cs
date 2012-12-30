using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Sanguosha.Lobby.Core
{
    [DataContract(Name = "RoomType")]
    public enum RoomType
    {
        [EnumMember]
        RoleOneDefector,
        [EnumMember]
        RoleNoDefector,
        [EnumMember]
        RoleTwoDefectors,
    }

    [DataContract(Name = "Timeout")]
    public enum Timeout
    {
        [EnumMember]
        TenSeconds = 10,
        [EnumMember]
        FifteenSeconds = 15,
        [EnumMember]
        TwentySeconds = 20,
    }

    public class Room
    {
        public object RoomLock;

        public Room()
        {
            seats = new List<Seat>();
            timeout = Timeout.FifteenSeconds;
        }

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
        private bool gameInProgress;

        public bool GameInProgress
        {
            get { return gameInProgress; }
            set { gameInProgress = value; }
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

        private int ownerId;

        public int OwnerId
        {
            get { return ownerId; }
            set { ownerId = value; }
        }

        List<Seat> seats;

        public List<Seat> Seats
        {
            get { return seats; }
            set { seats = value; }
        }

        private string ipAddress;

        public string IpAddress
        {
            get { return ipAddress; }
            set { ipAddress = value; }
        }

        private string ipPort;

        public string IpPort
        {
            get { return ipPort; }
            set { ipPort = value; }
        }
    }
}
