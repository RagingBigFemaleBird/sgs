using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Players;

namespace Sanguosha.Core.Cards
{
    [Serializable]    
    public class DeckType
    {
        private static Dictionary<string, DeckType> registeredDeckTypes = new Dictionary<string,DeckType>();

        public static DeckType Register(string name)
        {
            return Register(name, name);
        }

        public static DeckType Register(string name, string shortName)
        {
            if (!registeredDeckTypes.ContainsKey(shortName))
            {
                registeredDeckTypes.Add(shortName, new DeckType(name, shortName));
            }
            return registeredDeckTypes[shortName];
        }

        protected DeckType(string name, string shortName)
        {
            Name = name;
            AbbriviatedName = shortName;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        private string name;

        public string Name
        {
            get { return name; }
            private set { name = value; }
        }

        /// <summary>
        /// Sets/gets abbreviated name used to uniquely identify and serialize this DeckType.
        /// </summary>
        public string AbbriviatedName
        {
            get;
            private set;
        }

        public override bool Equals(object obj)
        {
            if (System.Object.ReferenceEquals(obj, this))
            {
                return true;
            }
            if (!(obj is DeckType))
            {
                return false;
            }
            DeckType type2 = (DeckType)obj;
            return name.Equals(type2.name);
        }

        public static bool operator ==(DeckType a, DeckType b)
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

        public static bool operator !=(DeckType a, DeckType b)
        {
            return !(a == b);
        }


        public static DeckType Dealing = DeckType.Register("Dealing", "0");
        public static DeckType Discard = DeckType.Register("Discard", "1");
        public static DeckType Compute = DeckType.Register("Compute", "2");
        public static DeckType Hand = DeckType.Register("Hand", "3");
        public static DeckType Equipment = DeckType.Register("Equipment", "4");
        public static DeckType DelayedTools = DeckType.Register("DelayedTools", "5");
        public static DeckType JudgeResult = DeckType.Register("JudgeResult", "6");
        public static DeckType GuHuo = DeckType.Register("GuHuo", "7");
        public static DeckType None = DeckType.Register("None", "8");
        public static DeckType Heroes = DeckType.Register("Heroes", "9");
        public static DeckType SpecialHeroes = DeckType.Register("SpecialHeroes", "A");

        public override string ToString()
        {
            return name;
        }
    }
}
