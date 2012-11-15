using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;

namespace Sanguosha.Expansions.Battle.Cards
{
    
    public class LeiSha : Sha
    {
        public override DamageElement ShaDamageElement
        {
            get
            {
                return DamageElement.Lightning;
            }
        }
    }
}
