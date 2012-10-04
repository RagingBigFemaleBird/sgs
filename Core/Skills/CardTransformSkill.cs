using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Core.Skills
{

    public abstract class CardTransformSkill : ISkill
    {
        public class CardTransformFailureException : SgsException
        {
        }

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
        /// 尝试使用当前技能转换一组卡牌。
        /// </summary>
        /// <param name="cards">被转化的卡牌。</param>
        /// <param name="arg">辅助转化的额外参数。</param>
        /// <param name="card">转换成的卡牌。</param>
        /// <returns>转换是否成功。</returns>
        public abstract VerifierResult TryTransform(List<Card> cards, object arg, out CompositeCard card);

        /// <summary>
        /// Transform a set of cards.
        /// </summary>
        /// <param name="cards">Cards to be transformed.</param>
        /// <param name="arg">Additional args to help the transformation.</param>
        /// <returns>False if transform is aborted.</returns>
        /// <exception cref="CardTransformFailureException"></exception>
        public bool Transform(List<Card> cards, object arg, out CompositeCard card)
        {
            if (TryTransform(cards, arg, out card) != VerifierResult.Success)
            {
                throw new CardTransformFailureException();
            }
            return DoTransformSideEffect(card, arg);
        }
        
        protected virtual bool DoTransformSideEffect(CompositeCard card, object arg)
        {
            return true;
        }

        public Players.Player Owner { get; set; }

        /// <summary>
        /// 卡牌转换技能可以转换成的卡牌类型。
        /// </summary>
//        public abstract List<CardHandler> PossibleResults { get; }
    }
}
