using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;

namespace Sanguosha.Core.Skills
{

    public abstract class CardTransformSkill : ISkill
    {
        protected VerifierResult RequireCards(List<Card> cards, int count)
        {
            if (cards == null || cards.Count < count)
            {
                return VerifierResult.Partial;
            }
            if (cards.Count > count)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        /// <summary>
        /// 卡牌转换技能
        /// </summary>
        /// <param name="cards">卡牌</param>
        /// <param name="arg">参数（可选）</param>
        /// <param name="commit">如果为true，改变牌面type</param>
        /// <param name="card">输出卡牌</param>
        /// <returns></returns>
        public abstract VerifierResult Transform(List<Card> cards, object arg, out CompositeCard card);
    }
}
