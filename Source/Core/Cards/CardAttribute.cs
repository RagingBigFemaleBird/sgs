using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Core.Cards
{
    public class CardAttribute
    {
        public static string TargetRequireTwoResponses = "TargetRequireTwoResponses";

        public static readonly CardAttribute SourceRequireTwoResponses = CardAttribute.Register("SourceRequireTwoResponses");

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

        static Dictionary<string, CardAttribute> _attributeNames;

        public static CardAttribute Register(string attributeName)
        {
            if (_attributeNames == null)
            {
                _attributeNames = new Dictionary<string, CardAttribute>();
            }
            if (_attributeNames.ContainsKey(attributeName))
            {
                return _attributeNames[attributeName];
            }
            var attr = new CardAttribute(attributeName);
            _attributeNames.Add(attributeName, attr);
            return attr;
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
            return name.Equals(type2.name);
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
