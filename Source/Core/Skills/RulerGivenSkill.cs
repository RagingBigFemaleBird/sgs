using Sanguosha.Core.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.Skills
{
    public interface IRulerGivenSkill : ISkill
    {
        Player Master { get; set; }
    }
}
