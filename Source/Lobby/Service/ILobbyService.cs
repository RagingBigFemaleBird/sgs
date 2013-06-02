using ProtoBuf;
using Sanguosha.Lobby;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Sanguosha.Lobby.Core
{
    public enum LoginStatus
    {
        Success = 0,
        OutdatedVersion = 1,
        InvalidUsernameAndPassword,
        UnknownFailure
    }

    [Serializable]
    [ProtoContract]
    public struct LoginToken
    {
        [ProtoMember(1)]
        public Guid TokenString { get; set; }
    }

    [DataContract(Name = "RoomOperationResult")]
    public enum RoomOperationResult
    {
        [EnumMember]
        Success = 0,
        [EnumMember]
        InvalidToken = -1,
        [EnumMember]
        Full = -2,
        [EnumMember]
        Password = -3,
        [EnumMember]
        Locked = -4,
        [EnumMember]
        Invalid = -5,
        [EnumMember]
        NotAutheticated = -6,
    }

    [Flags]
    [ProtoContract]
    public enum EnabledPackages : int // Change this to long if more package types are added!
    {
        None = 0,
        Wind = 1,
        Fire = 1 << 2,
        Woods = 1 << 3,
        Hills = 1 << 4,
        Gods = 1 << 5,
        SP = 1 << 6,
        OverKnightFame = 1 << 7,
        Others = 1 << 8
    }

    [Serializable]
    public struct RoomSettings
    {
        public int TimeOutSeconds { get; set; }
        public int NumberOfDefectors { get; set; }
        public bool IsDualHeroMode { get; set; }
        public int NumHeroPicks { get; set; }
        public EnabledPackages EnabledPackages { get; set; }
        public GameType GameType { get; set; }
    }

    [ServiceKnownType("GetKnownTypes", typeof(Helper))]
    [ServiceContract(Namespace = "", CallbackContract = typeof(IGameClient), SessionMode = SessionMode.Required)]
    public interface ILobbyService
    {
        [OperationContract(IsInitiating = true)]
        LoginStatus Login(int version, string username, string hash, out Account retAccount, out string reconnectionString, out LoginToken reconnectionToken);

        [OperationContract(IsInitiating = false)]
        void Logout();

        [OperationContract]
        IEnumerable<Room> GetRooms(bool notReadyRoomsOnly);

        [OperationContract]
        Room CreateRoom(RoomSettings settings, string password = null);

        [OperationContract]
        RoomOperationResult EnterRoom(int roomId, bool spectate, string password, out Room room);

        [OperationContract]
        RoomOperationResult ExitRoom();

        [OperationContract]
        RoomOperationResult ChangeSeat(int newSeat);

        [OperationContract]
        RoomOperationResult StartGame();

        [OperationContract]
        RoomOperationResult Ready();

        [OperationContract]
        RoomOperationResult CancelReady();

        [OperationContract]
        RoomOperationResult Kick(int seatNo);

        [OperationContract]
        RoomOperationResult OpenSeat(int seatNo);

        [OperationContract]
        RoomOperationResult CloseSeat(int seatNo);

        [OperationContract]
        RoomOperationResult Chat(string message);

        [OperationContract]
        RoomOperationResult Spectate(int roomId);

        [OperationContract]
        LoginStatus CreateAccount(string userName, string p);

        [OperationContract]
        void SubmitBugReport(Stream s);
    }

    public interface IGameClient
    {
        [OperationContract(IsOneWay = true)]
        void NotifyRoomUpdate(int id, Room room);

        [OperationContract(IsOneWay = true)]
        void NotifyKicked();

        [OperationContract(IsOneWay = true)]
        void NotifyGameStart(string connectionString, LoginToken token);

        [OperationContract(IsOneWay = true)]
        void NotifyChat(Account account, string message);

        [OperationContract]
        bool Ping();
    }

    static class Helper
    {
        public static IEnumerable<Type> GetKnownTypes(ICustomAttributeProvider provider)
        {
            System.Collections.Generic.List<System.Type> knownTypes =
                new System.Collections.Generic.List<System.Type>();
            // Add any types to include here.
            knownTypes.Add(typeof(Room));
            knownTypes.Add(typeof(Seat));
            knownTypes.Add(typeof(Account));
            return knownTypes;
        }
    }

}
