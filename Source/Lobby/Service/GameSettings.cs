using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Lobby.Core
{
    [Serializable]
    public class GameSettings
    {
        public int TotalPlayers { get; set; }
        public int TimeOutSeconds { get; set; }
        public bool CheatEnabled { get; set; }
        public int NumberOfDefectors { get; set; }
        private List<string> displayedNames;

        public List<string> DisplayedNames
        {
            get { return displayedNames; }
            set { displayedNames = value; }
        }
    }
}
