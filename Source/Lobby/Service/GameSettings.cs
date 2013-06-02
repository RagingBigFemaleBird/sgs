using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Lobby.Core
{
    [Serializable]
    [ProtoContract]
    public class GameSettings
    {
        public GameSettings()
        {
            PackagesEnabled = new List<string>();
            Accounts = new List<Account>();
        }
        [ProtoMember(1)]
        public int TotalPlayers { get; set; }
        [ProtoMember(2)]
        public int TimeOutSeconds { get; set; }
        [ProtoMember(3)]
        public bool CheatEnabled { get; set; }
        [ProtoMember(4)]
        public int NumberOfDefectors { get; set; }
        [ProtoMember(5)]
        public List<Account> Accounts { get; set; }
        [ProtoMember(6)]
        public bool DualHeroMode { get; set; }
        [ProtoMember(7)]
        public int NumHeroPicks { get; set; }
        [ProtoMember(8)]
        public IList<string> PackagesEnabled { get; set; }
        [ProtoMember(9)]
        public bool IsGodEnabled { get; set; }
        [ProtoMember(10)]
        public GameType GameType { get; set; }
    }
    public enum GameType
    {
        RoleGame,
        Pk1v1,
    }
}
