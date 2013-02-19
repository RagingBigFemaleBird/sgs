using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.Cards
{
    /// <summary>
    /// 暂时存放卡牌，用于卡牌的分段移动，实际上并不存在，存在于该处的卡牌仍属于player。
    /// e.g. 缔盟，甘露，突袭，巧变②
    /// </summary>
    [Serializable]
    public class StagingDeckType : DeckType
    {
        public StagingDeckType(string name)
            : base(name)
        { }
    }
}
