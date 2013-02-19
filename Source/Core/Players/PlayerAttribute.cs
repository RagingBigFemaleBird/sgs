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
        private PlayerAttribute(string attrName, bool autoReset, bool isAMark, bool isStatus)
        {
            Name = attrName;
            AutoReset = autoReset;
            IsMark = isAMark;
            IsStatus = isStatus;
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

        private bool isStatus;

        public bool IsStatus
        {
            get { return isStatus; }
            set { isStatus = value; }
        }

        private bool isMark;

        public bool IsMark
        {
            get { return isMark; }
            set { isMark = value; }
        }

        static Dictionary<string, PlayerAttribute> _attributeNames;

        private object internalKey = null;

        public PlayerAttribute this[Player key]
        {
            get
            {
                if (key == null) return this;
                if (!key.AssociatedPlayerAttributes.ContainsKey(this))
                {
                    var attribute = new PlayerAttribute(this.Name, this.autoReset, this.isMark, this.isStatus);
                    attribute.internalKey = key;
                    key.AssociatedPlayerAttributes.Add(this, attribute);
                }
                return key.AssociatedPlayerAttributes[this];
            }
        }

        public static PlayerAttribute Register(string attributeName, bool autoReset = false, bool isAMark = false, bool isStatus = false)
        {
            if (_attributeNames == null)
            {
                _attributeNames = new Dictionary<string, PlayerAttribute>();
            }
            if (_attributeNames.ContainsKey(attributeName))
            {
                return _attributeNames[attributeName];
                //throw new DuplicateAttributeKeyException(attributeName);
            }
            var attr = new PlayerAttribute(attributeName, autoReset, isAMark, isStatus);
            _attributeNames.Add(attributeName, attr);
            return attr;
        }
    }
}
