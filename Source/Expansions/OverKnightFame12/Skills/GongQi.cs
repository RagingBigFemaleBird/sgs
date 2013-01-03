using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Players;
using System.Diagnostics;

namespace Sanguosha.Expansions.OverKnightFame12.Skills
{
    /// <summary>
    /// 弓骑-你可以将一张装备牌当【杀】使用或打出，你以此法使用的【杀】无距离限制。
    /// </summary>
    public class GongQi : OneToOneCardTransformSkill
    {
        public override bool VerifyInput(Card card, object arg)
        {
            return card.Type.IsCardCategory(CardCategory.Equipment);
        }

        public override VerifierResult TryTransform(List<Card> cards, object arg, out CompositeCard card)
        {
            var ret = base.TryTransform(cards, arg, out card);
            if (card != null) card[GongQiSha] = 1;
            return ret;
        }

        public override CardHandler PossibleResult
        {
            get { return new RegularSha(); }
        }

        public GongQi()
        {
            LinkedPassiveSkill = new GongQiShaPassive();
        }
        static CardAttribute GongQiSha = CardAttribute.Register("GongQiSha");

        class GongQiShaPassive : TriggerSkill
        {
            public void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
            {
                ShaEventArgs args = (ShaEventArgs)eventArgs;
                Trace.Assert(args != null);
                for (int i = 0; i < args.RangeApproval.Count; i++)
                {
                    args.RangeApproval[i] = true;
                }
            }

            public GongQiShaPassive()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                   this,
                   (p, e, a) => { ShaEventArgs args = (ShaEventArgs)a; return a.Card[GongQiSha] == 1; },
                   Run,
                   TriggerCondition.OwnerIsSource
                ) { IsAutoNotify = false };
                Triggers.Add(Sha.PlayerShaTargetValidation, trigger);
                IsEnforced = true;
            }
        }

    }
}
