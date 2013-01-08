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
        public static readonly CardAttribute TargetRequireTwoResponses = CardAttribute.Register("TargetRequireTwoResponses");

        public static readonly CardAttribute SourceRequireTwoResponses = CardAttribute.Register("SourceRequireTwoResponses");

        private CardAttribute(string attrName)
        {
            Name = attrName;
            internalAttributes = new Dictionary<object, CardAttribute>();
        }
        
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        static Dictionary<string, CardAttribute> _attributeNames;

        private Dictionary<object, CardAttribute> internalAttributes;
        private object internalKey = null;

        public CardAttribute this[object key]
        {
            get
            {
                if (!internalAttributes.ContainsKey(key))
                {
                    var attribute = new CardAttribute(this.Name);
                    attribute.internalKey = key;
                    internalAttributes.Add(key, attribute);
                }
                return internalAttributes[key];
            }
        }

        public static CardAttribute Register(string attributeName)
        {
            if (_attributeNames == null)
            {
                _attributeNames = new Dictionary<string, CardAttribute>();
            }
            if (_attributeNames.ContainsKey(attributeName))
            {
                throw new DuplicateAttributeKeyException(attributeName);
            }
            var attr = new CardAttribute(attributeName);
            _attributeNames.Add(attributeName, attr);
            return attr;
        }
    }
}
