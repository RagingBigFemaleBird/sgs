using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.Cards
{
    [Serializable]
    public class PrivateDeckType : DeckType
    {
        private bool publiclyVisible;

        public bool PubliclyVisible
        {
            get { return publiclyVisible; }
            set { publiclyVisible = value; }
        }

        public PrivateDeckType(string name, bool pv = false)
            : base(name)
        {
            publiclyVisible = pv;
        }
    }
}
