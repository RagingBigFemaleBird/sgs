using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.Heroes
{
    public class UnknownHeroCardHandler : HeroCardHandler
    {
        public UnknownHeroCardHandler() : base(null)
        {
        }
        public override string Name
        {
            get
            {
                return _cardTypeString;
            }
        }

        private static string _cardTypeString = "UnknownHero";
    }
}
