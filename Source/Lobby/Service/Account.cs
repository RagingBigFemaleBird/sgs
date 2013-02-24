namespace Sanguosha.Lobby.Core
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    [Serializable]
    public class Account
    {
        [Key]
        public string UserName { get; set; }
        [NonSerialized]
        private string password;

        public string Password
        {
            get { return password; }
            set { password = value; }
        }
        public int Credits { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Quits { get; set; }
        public int TotalGames { get; set; }        
        public string DisplayedName { get; set; }
        public int Experience { get; set; }
        public int Level { get; set; }
    }
}
