using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Players;

namespace Sanguosha.Core.Skills
{
    public interface ISkill : ICloneable
    {
        Player Owner { get; set; }
        bool IsRulerOnly { get; }
        bool IsSingleUse { get; }
        bool IsAwakening { get; }
        bool IsEnforced { get; }
    }
}
