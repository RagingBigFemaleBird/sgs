using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.Cards
{
    public enum CardCategory
    {
        Basic = (1 << 1),
        Equipment = (1 << 2),
        Tool = (1 << 3),
        ImmediateTool = Tool | (1 << 4),
        DelayedTool = Tool | (1 << 5),
        DefensiveHorse = Equipment | (1 << 6),
        OffsensiveHorse = Equipment | (1 << 7),
        Armor = Equipment | (1 << 8),
        Weapon = Equipment | (1 << 9),
        Unknown = (1 << 31),
    }

    public class CardCategoryManager
    {
        public static bool IsCardCategory(CardCategory a, CardCategory belongsTo)
        {
            return (a & belongsTo) == belongsTo;
        }
    }
}
