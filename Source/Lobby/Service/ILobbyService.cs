using Sanguosha.Lobby;
using System;
using System.Collections.Generic;
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
    public struct LoginToken
    {
        public Guid TokenString { get; set; }
    }

    [DataContract(Name = "RoomOperationResult")]
    public enum RoomOperationResult
    {
        [EnumMember]
        Success = 0,
        [EnumMember]
        Auth = -1,
        [EnumMember]
        Full = -2,
        [EnumMember]
        Password = -3,
        [EnumMember]
        Locked = -4,
        [EnumMember]
        Invalid = -5,
    }

    [Serializable]
    public struct RoomSettings
    {
        public int TimeOutSeconds { get; set; }
        public int NumberOfDefectors { get; set; }
        public bool IsDualHeroMode { get; set; }
        public int NumHeroPicks { get; set; }
    }

    [ServiceKnownType("GetKnownTypes", typeof(Helper))]
    [ServiceContract(Namespace = "", CallbackContract = typeof(IGameClient), SessionMode = SessionMode.Required)]
    public interface ILobbyService
    {
        [OperationContract(IsInitiating = true)]
        LoginStatus Login(int version, string username, string hash, out LoginToken token, out Account retAccount, out string reconnectionString);

        [OperationContract(IsInitiating = false)]
        void Logout(LoginToken token);

        [OperationContract]
        IEnumerable<Room> GetRooms(LoginToken token, bool notReadyRoomsOnly);

        [OperationContract]
        Room CreateRoom(LoginToken token, RoomSettings settings, string password = null);

        [OperationContract]
        RoomOperationResult EnterRoom(LoginToken token, int roomId, bool spectate, string password, out Room room);

        [OperationContract]
        RoomOperationResult ExitRoom(LoginToken token);

        [OperationContract]
        RoomOperationResult ChangeSeat(LoginToken token, int newSeat);

        [OperationContract]
        RoomOperationResult StartGame(LoginToken token);

        [OperationContract]
        RoomOperationResult Ready(LoginToken token);

        [OperationContract]
        RoomOperationResult CancelReady(LoginToken token);

        [OperationContract]
        RoomOperationResult Kick(LoginToken token, int seatNo);

        [OperationContract]
        RoomOperationResult OpenSeat(LoginToken token, int seatNo);

        [OperationContract]
        RoomOperationResult CloseSeat(LoginToken token, int seatNo);

        [OperationContract]
        RoomOperationResult Chat(LoginToken token, string message);

        [OperationContract]
        RoomOperationResult Spectate(LoginToken token, int roomId);

        [OperationContract]
        LoginStatus CreateAccount(string userName, string p);
    }

    public interface IGameClient
    {
        [OperationContract(IsOneWay = true)]
        void NotifyRoomUpdate(int id, Room room);

        [OperationContract(IsOneWay = true)]
        void NotifyKicked();

        [OperationContract(IsOneWay = true)]
        void NotifyGameStart(string connectionString);

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
