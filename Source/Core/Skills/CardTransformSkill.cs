using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Players;

namespace Sanguosha.Core.Skills
{
    public abstract class CardTransformSkill : ISkill
    {
        public virtual UiHelper Helper { get { return new UiHelper(); } }
        public class CardTransformFailureException : SgsException
        {
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
        public bool Transform(List<Card> cards, object arg, out CompositeCard card, List<Player> targets)
        {
            if (TryTransform(cards, arg, out card) != VerifierResult.Success)
            {
                throw new CardTransformFailureException();
            }
            foreach (Card c in card.Subcards)
            {
                c.Type = card.Type;
            }
            return DoTransformSideEffect(card, arg, targets);
        }
        
        protected virtual bool DoTransformSideEffect(CompositeCard card, object arg, List<Player> targets)
        {
            return true;
        }

        public Players.Player Owner { get; set; }

        public virtual List<CardHandler> PossibleResults { get { return null; } }

        public virtual bool isRulerOnly { get { return false; } }
        public virtual bool isSingleUse { get { return false; } }
        public virtual bool isAwakening { get { return false; } }
        public bool isEnforced { get { return false; } }

    }
}
