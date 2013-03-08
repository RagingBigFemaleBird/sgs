using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;

namespace Sanguosha.Expansions.Basic.Cards
{
    
    public class NanManRuQin : Aoe
    {
        public NanManRuQin()
        {
            RequiredCard = new Sha();
        }

        protected override string UsagePromptString
        {
            get { return "NanManRuQin"; }
        }

        public override CardHandler RequiredCard
        {
            get;
            protected set;
        }
    }
}
