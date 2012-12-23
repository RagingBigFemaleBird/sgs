using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Core.Players
{
    public class PlayerAttribute
    {
        private PlayerAttribute(string attrName, bool autoReset, bool isAMark)
        {
            Name = attrName;
            AutoReset = autoReset;
            IsMark = isAMark;
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

        private bool isMark;

        public bool IsMark
        {
            get { return isMark; }
            set { isMark = value; }
        }

        static Dictionary<string, PlayerAttribute> _attributeNames;

        public static PlayerAttribute Register(string attributeName, bool autoReset = false, bool isAMark = false)
        {
            if (_attributeNames == null)
            {
                _attributeNames = new Dictionary<string, PlayerAttribute>();
            }
            if (_attributeNames.ContainsKey(attributeName))
            {
                throw new DuplicateAttributeKeyException(attributeName);
            }
            var attr = new PlayerAttribute(attributeName, autoReset, isAMark);
            _attributeNames.Add(attributeName, attr);
            return attr;
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
