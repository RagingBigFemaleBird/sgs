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
        /// <param name="card">输出卡牌</param>
        /// <returns></returns>
        public abstract VerifierResult Transform(List<Card> cards, object arg, out CompositeCard card);
        /// <summary>
        /// 卡牌转换技能打出
        /// </summary>
        /// <param name="cards">卡牌</param>
        /// <param name="arg">参数</param>
        /// <returns>true if 可以打出, false if 不可打出</returns>
        public virtual bool Commit(List<Card> cards, object arg)
        {
            return true;
        }
    }
}
