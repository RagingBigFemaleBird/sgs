using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Players;
using Sanguosha.Core.UI;
using Sanguosha.Core.Cards;
using Sanguosha.Core.Heroes;

namespace Sanguosha.Core.Skills
{
    public interface ISkill : ICloneable
    {
        Player Owner { get; set; }
        Hero HeroTag { get; set; }
        bool IsRulerOnly { get; }
        bool IsSingleUse { get; }
        bool IsAwakening { get; }
        bool IsEnforced { get; }
        UiHelper Helper { get; }
    }
}
