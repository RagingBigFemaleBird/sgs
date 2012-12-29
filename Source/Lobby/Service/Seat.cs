using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Sanguosha.Lobby.Core
{
    [DataContract(Name = "RoomState")]
    public enum SeatState
    {
        /// <summary>
        /// Seat is closed.
        /// </summary>
        [EnumMember]
        Closed,

        /// <summary>
        /// Seat is open and not taken yet.
        /// </summary>
        [EnumMember]
        Empty,

        /// <summary>
        /// Seat is taken by a guest and the guest is not ready.
        /// </summary>
        [EnumMember]
        GuestTaken,

        /// <summary>
        /// Seat is taken by a guest and the guest is ready.
        /// </summary>
        [EnumMember]
        GuestReady,
        
        /// <summary>
        /// Seat is taken by the host.
        /// </summary>
        [EnumMember]
        Host,

        /// <summary>
        /// Seat is taken by a player who is still in a game.
        /// </summary>
        [EnumMember]
        Gaming,
    }

    public class Seat
    {
        public SeatState State
        {
            get;
            set;
        }
        
        private Account account;

        public Account Account
        {
            get { return account; }
            set { account = value; }
        }

    }
}
