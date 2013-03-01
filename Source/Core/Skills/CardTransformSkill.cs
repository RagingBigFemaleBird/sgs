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
using Sanguosha.Core.Heroes;

namespace Sanguosha.Core.Skills
{
    
    public abstract class CardTransformSkill : ISkill
    {
        public UiHelper Helper
        {
            get;
            protected set;
        }
        public class CardTransformFailureException : SgsException
        {
        }

        public CardTransformSkill()
        {
            LinkedPassiveSkill = null;
            Helper = new UiHelper();
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
            NotifyAction(Owner, targets, card);
            bool ret = DoTransformSideEffect(card, arg, targets);
            if (ret)
            {
                card.Owner = Owner;
                foreach (Card c in card.Subcards)
                {
                    if (c.Place.Player != null && c.Place.DeckType == DeckType.Equipment && CardCategoryManager.IsCardCategory(c.Type.Category, CardCategory.Equipment))
                    {
                        Equipment e = c.Type as Equipment;
                        e.UnregisterTriggers(c.Place.Player);
                    }
                    c.Type = card.Type;
                }
            }
            return ret;
        }
        
        protected virtual bool DoTransformSideEffect(CompositeCard card, object arg, List<Player> targets)
        {
            return true;
        }

        /// <summary>
        /// Gets/sets passive skill linked with the card transform skill
        /// </summary>
        /// <remarks>
        /// 断粮和疬火同时拥有卡牌转换技和被动技成分，故设置此成员变量
        /// </remarks>
        public PassiveSkill LinkedPassiveSkill { get; protected set; }
        public Hero HeroTag { get; set; }

        Players.Player owner;
        public virtual Players.Player Owner
        {
            get { return owner; }
            set
            {
                if (owner == value) return;
                owner = value;
                if (LinkedPassiveSkill != null)
                {
                    LinkedPassiveSkill.HeroTag = HeroTag;
                    LinkedPassiveSkill.Owner = value;
                }
            }
        }

        public virtual List<CardHandler> PossibleResults { get { return null; } }

        public bool IsRulerOnly { get; protected set; }
        public bool IsSingleUse { get; protected set; }
        public bool IsAwakening { get; protected set; }
        public bool IsEnforced { get { return false; } }

        public virtual void NotifyAction(Players.Player source, List<Players.Player> targets, CompositeCard card)
        {
            ActionLog log = new ActionLog();
            log.GameAction = GameAction.None;
            log.CardAction = card;
            log.SkillAction = this;
            log.Source = source;
            log.SpecialEffectHint = GenerateSpecialEffectHintIndex(source, targets, card);
            Games.Game.CurrentGame.NotificationProxy.NotifySkillUse(log);
            if (card.Subcards != null)
            {
                foreach (Card c in card.Subcards)
                {
                    if (c.Log == null)
                    {
                        c.Log = new ActionLog();
                    }
                    c.Log.SkillAction = this;
                }
            }
        }

        protected virtual int GenerateSpecialEffectHintIndex(Player source, List<Player> targets, CompositeCard card)
        {
            return 0;
        }

        public object Clone()
        {
            var skill = Activator.CreateInstance(this.GetType()) as CardTransformSkill;
            skill.Owner = this.Owner;
            skill.Helper = this.Helper;
            return skill;
        }
    }
}
