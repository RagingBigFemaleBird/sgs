using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Players;

namespace Sanguosha.Core.Cards
{
    
    public abstract class Armor : Equipment
    {
        /// <summary>
        /// 玩家无视防具
        /// </summary>
        /// <remarks>高顺</remarks>
        public static readonly string PlayerIgnoreArmor = "PlayerIgnoreArmor";
        /// <summary>
        /// 无条件防具无效
        /// </summary>
        /// <seealso cref="Sanguosha.Expansions.Woods.Skills.WuQian"/>
        public static readonly PlayerAttribute UnconditionalIgnoreArmor = PlayerAttribute.Register("UnconditionalIgnoreArmor", false);
        /// <summary>
        /// 卡牌（杀）无视所有防具
        /// </summary>
        /// <remarks>青钢剑</remarks>
        public static readonly CardAttribute IgnoreAllArmor = CardAttribute.Register("IgnoreAllArmor");
        /// <summary>
        /// 卡牌无视某个玩家防具
        /// </summary>
        /// <remarks>还没人用</remarks>
        public static readonly string IgnorePlayerArmor = "IgnorePlayerArmor";
        public override CardCategory Category
        {
            get { return CardCategory.Armor; }
        }

        public static bool ArmorIsValid(Player player, ReadOnlyCard card)
        {
            return player[PlayerAttribute.Register(PlayerIgnoreArmor + player.Id)] == 0 &&
                   player[UnconditionalIgnoreArmor] == 0 &&
                   (card == null || (card[Armor.IgnoreAllArmor] == 0 && card[CardAttribute.Register(IgnorePlayerArmor + player.Id)] != 1));
        }

        protected override void Process(Player source, Players.Player dest, ICard card, ReadOnlyCard cardr)
        {
            throw new NotImplementedException();
        }
    }
}
