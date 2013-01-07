using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Lobby.Core
{
    [Serializable]
    public class AccountConfiguration
    {
        public List<LoginToken> AccountIds { get; set; }
        public List<string> DisplayedNames { get; set; }
    }
}
