using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.Players
{
    public class PlayerAttribute
    {
        private PlayerAttribute(string attrName, bool autoReset)
        {
            Name = attrName;
            AutoReset = autoReset;
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private bool autoReset;

        public bool AutoReset
        {
            get { return autoReset; }
            set { autoReset = value; }
        }

        public static PlayerAttribute Register(string attributeName, bool autoReset = false)
        {
            return new PlayerAttribute(attributeName, autoReset);
        }
    }
}
