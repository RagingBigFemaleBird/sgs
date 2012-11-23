using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.Players
{
    public class PlayerAttribute
    {
        private PlayerAttribute(string attrName, bool autoReset, bool isAMark)
        {
            Name = attrName;
            AutoReset = autoReset;
            IsAMark = isAMark;
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

        private bool isAMark;

        public bool IsAMark
        {
            get { return isAMark; }
            set { isAMark = value; }
        }

        public static PlayerAttribute Register(string attributeName, bool autoReset = false, bool isAMark = false)
        {
            return new PlayerAttribute(attributeName, autoReset, isAMark);
        }

        public override bool Equals(object obj)
        {
            if (System.Object.ReferenceEquals(obj, this))
            {
                return true;
            }
            if (!(obj is PlayerAttribute))
            {
                return false;
            }
            PlayerAttribute type2 = (PlayerAttribute)obj;
            return name.Equals(type2.name);
        }

        public static bool operator ==(PlayerAttribute a, PlayerAttribute b)
        {
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.name.Equals(b.name);
        }

        public static bool operator !=(PlayerAttribute a, PlayerAttribute b)
        {
            return !(a == b);
        }
        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
    }
}
