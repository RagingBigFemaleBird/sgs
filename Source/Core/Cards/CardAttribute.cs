using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Players;

namespace Sanguosha.Core.Cards
{
    public class CardAttribute
    {
        public static readonly CardAttribute TargetRequireTwoResponses = CardAttribute.Register("TargetRequireTwoResponses");

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

        private object internalKey = null;

        public CardAttribute this[Player key]
        {
            get
            {
                if (key == null) return this;
                if (!key.AssociatedCardAttributes.ContainsKey(this))
                {
                    var attribute = new CardAttribute(this.Name);
                    attribute.internalKey = key;
                    key.AssociatedCardAttributes.Add(this, attribute);
                }
                return key.AssociatedCardAttributes[this];
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
