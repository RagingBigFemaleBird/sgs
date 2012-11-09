using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.Cards
{
    public class CardAttribute
    {
        private CardAttribute(string attrName)
        {
            Name = attrName;
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public static CardAttribute Register(string attributeName)
        {
            return new CardAttribute(attributeName);
        }

        public override bool Equals(object obj)
        {
            if (System.Object.ReferenceEquals(obj, this))
            {
                return true;
            }
            if (!(obj is CardAttribute))
            {
                return false;
            }
            CardAttribute type2 = (CardAttribute)obj;
            return name == type2.name;
        }

        public static bool operator ==(CardAttribute a, CardAttribute b)
        {
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.name == b.name;
        }

        public static bool operator !=(CardAttribute a, CardAttribute b)
        {
            return !(a == b);
        }
        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
    }
}
