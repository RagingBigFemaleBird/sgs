using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.Skills
{
    public interface ISkill
    {
        Players.Player Owner { get; set; }
        bool isRulerOnly { get; }
        bool isSingleUse { get; }
        bool isAwakening { get; }
        bool isEnforced { get; }
    }
}
