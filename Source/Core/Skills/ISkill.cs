using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Players;
using Sanguosha.Core.UI;
using Sanguosha.Core.Cards;

namespace Sanguosha.Core.Skills
{
    public interface ISkill : ICloneable
    {
        Player Owner { get; set; }
        DeckType ExtraCardsDeck { get; }
        bool IsRulerOnly { get; }
        bool IsSingleUse { get; }
        bool IsAwakening { get; }
        bool IsEnforced { get; }
        UiHelper Helper { get; }
    }
}
