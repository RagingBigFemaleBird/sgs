using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Core.Games;

namespace Sanguosha.Expansions.Woods.Skills
{
    /// <summary>
    /// 断粮-出牌阶段，你可以将一张黑色牌当【兵粮寸断】使用，此牌必须为基本牌或装备牌；你可以对距离2以内的一名其他角色使用【兵粮寸断】。
    /// </summary>
    public class DuanLiang : OneToOneCardTransformSkill
    {
        public override bool VerifyInput(Card card, object arg)
        {
            return card.SuitColor == SuitColorType.Black && (CardCategoryManager.IsCardCategory(card.Type.Category, CardCategory.Basic) || CardCategoryManager.IsCardCategory(card.Type.Category, CardCategory.Equipment));
        }

        public override CardHandler PossibleResult
        {
            get { return new BingLiangCunDuan(); }
        }

        public DuanLiang()
        {
            linkedPassiveSkill = new DuanLiangPassive();
        }

        public class DuanLiangPassive : TriggerSkill
        {
            public DuanLiangPassive()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { return a.Card.Type is BingLiangCunDuan; },
                    (p, e, a) => { (a as AdjustmentEventArgs).AdjustmentAmount += 1; },
                    TriggerCondition.OwnerIsSource
                ) { IsAutoNotify = false };
                Triggers.Add(GameEvent.CardRangeModifier, trigger);
                IsEnforced = true;
            }
        }

    }
}
