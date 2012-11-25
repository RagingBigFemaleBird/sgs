using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Lobby
{
    public class Seat
    {
        private bool disabled;

        public bool Disabled
        {
            get { return disabled; }
            set { disabled = value; }
        }

        private Account account;

        public Account Account
        {
            get { return account; }
            set { account = value; }
        }

        private bool ready;

        public bool Ready
        {
            get { return ready; }
            set { ready = value; }
        }
    }
}
