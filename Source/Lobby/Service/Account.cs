namespace Sanguosha.Lobby.Core
{
    using System;
    using System.Collections.Generic;

    public class Account
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public long Credits { get; set; }
        public long Wins { get; set; }
        public long Losses { get; set; }
        public string DisplayedName { get; set; }
        public long Quits { get; set; }
    }
}
