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
        [ProtoMember(1)]
        public int TotalPlayers { get; set; }
        [ProtoMember(2)]
        public int TimeOutSeconds { get; set; }
        [ProtoMember(3)]
        public bool CheatEnabled { get; set; }
        [ProtoMember(4)]
        public int NumberOfDefectors { get; set; }
        [ProtoMember(5)]
        public List<Account> Accounts;
        [ProtoMember(6)]
        public bool DualHeroMode { get; set; }
        [ProtoMember(7)]
        public int NumHeroPicks { get; set; }
    }
}
