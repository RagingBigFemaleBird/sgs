namespace Sanguosha.Lobby.Core
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class Account
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public int Credits { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Quits { get; set; }
        public int TotalGames { get; set; }        
        public string DisplayedName { get; set; }
    }
}
