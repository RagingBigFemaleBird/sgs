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
    [Serializable]
    public class WanJianQiFa : Aoe
    {
        public WanJianQiFa()
        {
            ResponseCardVerifier = new SingleCardUsageVerifier((c) => { return c.Type is Shan; });
        }

        protected override string UsagePromptString
        {
            get { return "WanJianQiFa"; }
        }

        protected override CardHandler RequiredCard()
        {
            return new Shan();
        }
    }
}
