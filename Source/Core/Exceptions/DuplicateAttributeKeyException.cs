using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.Exceptions
{
    public class DuplicateAttributeKeyException : SgsException
    {
        public DuplicateAttributeKeyException() { }

        public DuplicateAttributeKeyException(string name)
        {
            AttributeName = name;
        }

        public string AttributeName { get; set; }
    }
}
