using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.Skills
{
    public interface IAdditionalTypedSkill : ISkill
    {
        Cards.CardHandler AdditionalType { get; set; }
    }
}
